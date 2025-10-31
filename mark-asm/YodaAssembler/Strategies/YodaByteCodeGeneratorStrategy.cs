using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Interfaces;
using YodaAssembler.Records;

namespace YodaAssembler.Strategies;

/// <summary>
/// Generates byte code for the given tokens based on details listed in <see href="https://github.com/YorkCodeDojo/yoda/blob/main/manual.md">the manual</see>
/// </summary>
public class YodaByteCodeGeneratorStrategy
	: IByteCodeGeneratorStrategy
{
	private int LineNumber { get; set; }

	#region IByteCodeGeneratorStrategy implementation

	/// <inheritdoc />
	public byte?[] Generate(DirectiveType directiveType, IEnumerable<IToken> tokens)
	{
		ArgumentNullException.ThrowIfNull(tokens);
		var tokenList = tokens.ToList();
		ArgumentOutOfRangeException.ThrowIfZero(tokenList.Count, nameof(tokens));
		LineNumber = tokenList[0].LineNumber;

		return directiveType switch
		{
			DirectiveType.Program or
				DirectiveType.Prog or
				DirectiveType.Code => GetProgramByteCode(tokenList),
			DirectiveType.Data => GetDataByteCode(tokenList),
			_ => throw new ArgumentOutOfRangeException(nameof(directiveType), directiveType, null)
		};
	}

	#endregion

	#region Methods

	/// <summary>
	/// Utility method that returns an offset value to mask against the opCode for the byte based upon the number and
	/// type of <paramref name="parameters"/> passed
	/// </summary>
	/// <param name="parameters">The parameters for the command</param>
	/// <returns>A byte mask to be applied to the base opCode</returns>
	private byte GetCommandOffsetValue(IEnumerable<IToken> parameters)
	{
		byte offset = 0;
		foreach (var parameter in parameters)
		{
			offset <<= 1;
			if (parameter.ParameterType is ParameterTypes.LiteralNumber or ParameterTypes.Symbol
			    or ParameterTypes.LiteralChar)
				offset |= 1;
		}

		return offset;
	}

	/// <summary>
	/// Converts the tokens within a <see cref="DirectiveType.Program"/> area into their bytecode equivalent
	/// </summary>
	/// <param name="tokens">The tokens to be converted</param>
	/// <returns>The array of bytes that represent the value of the tokens</returns>
	/// <exception cref="TokeniserException"></exception>
	/// <remarks>Symbols are converted to null bytes for replacement elsewhere</remarks>
	private byte?[] GetProgramByteCode(List<IToken> tokens)
	{
		if (tokens[0] is not YodaCommandToken command)
			throw new TokeniserException(LineNumber, $"Attempt to generate program bytecode without a command");

		if (tokens.Count != 1 + command.YodaCommand.ParameterCount)
			throw new TokeniserException(LineNumber, $"Incorrect number of parameters for {command}");

		var parameters = Enumerable.Range(0, tokens.Count)
			.Select(i => new { ParamNumber = i, Token = tokens[i] })
			.Where(q => q.ParamNumber > 0)
			.ToList();

		var bytes = new List<byte?>();

		//	Extract the opcode for the command
		byte commandByte = command.YodaCommand.OpCode;

		//	If parameters are associated with the command - e.g. LoadFromFile - then mask the relevant bits for the parameter types 
		if (command.YodaCommand.ParameterCount > 0)
			commandByte |= GetCommandOffsetValue(tokens[1..]);
		bytes.Add(commandByte);

		//	Any subsequent tokens get handled now
		foreach (var parameter in parameters)
		{
			var token = parameter.Token;
			var allowedTypes = command.YodaCommand.ParameterTypes[parameter.ParamNumber - 1];
			var paramType = token.ParameterType;

			if(paramType.Equals(ParameterTypes.None) || !allowedTypes.HasFlag(paramType))
				throw new TokeniserException(LineNumber, $"Incorrect parameter type {paramType} for {command.YodaCommand.Mnemonic}");
			
			switch (token.TokenType)
			{
				case TokenType.LiteralChar:
					bytes.Add((byte)char.Parse(token.Text ?? ""));
					break;
				case TokenType.LiteralNumber:
					if (token is not YodaNumericValueToken numericToken)
						throw new TokeniserException(LineNumber, $"Invalid token value for {token.TokenType}");
					bytes.Add((byte)numericToken.NumericValue);
					break;
				case TokenType.DirectNumber:
					if (token is not YodaCompositeToken dnToken)
						throw new TokeniserException(LineNumber, $"Invalid token value for {token.TokenType}");
					bytes.Add((byte?)dnToken.InnerNumericValue);
					break;
				//	Conversion from symbols to values happens elsewhere - just mark the point with null for now
				case TokenType.Symbol:
				case TokenType.DirectSymbol:
					bytes.Add(null);
					break;
				default:
					throw new TokeniserException(LineNumber, $"Invalid token type: {token.TokenType}");
			}
		}

		return bytes.ToArray();
	}

	/// <summary>
	/// Converts the tokens within a <see cref="DirectiveType.Data"/> area into their bytecode equivalent
	/// </summary>
	/// <param name="tokens">The tokens to be converted</param>
	/// <returns>The array of bytes that represent the value of the tokens</returns>
	/// <remarks>Symbols are converted to null bytes for replacement elsewhere</remarks>
	private byte?[] GetDataByteCode(List<IToken> tokens)
	{
		var bytes = new List<byte?>();
		foreach (var token in tokens)
		{
			switch (token.TokenType)
			{
				case TokenType.LiteralString:
					if (string.IsNullOrEmpty(token.Text))
						continue;

					token.Text.ToCharArray()
						.ToList()
						.ForEach(c => bytes.Add((byte)c));
					break;
				case TokenType.LiteralChar:
					bytes.Add((byte)char.Parse(token.Text ?? ""));
					break;
				case TokenType.LiteralNumber:
					if (token is not YodaNumericValueToken numericToken)
						throw new TokeniserException(LineNumber, $"Invalid token type for {token.TokenType}");
					bytes.Add((byte)numericToken.NumericValue);
					break;
				case TokenType.Symbol:
					bytes.Add(null);
					break;
				default:
					throw new TokeniserException(LineNumber,
						$"Invalid token type: {token.TokenType} for [{DirectiveType.Data}] directive");
			}
		}

		return bytes.ToArray();
	}

	#endregion
}