using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Interfaces;

namespace YodaAssembler.Records;

public record YodaTokenByteCode
{
	#region Properties

	/// <summary>
	/// Hold the line number for the source of the tokens
	/// </summary>
	public int LineNumber { get; private init; }

	/// <summary>
	/// The start location for the bytecode
	/// </summary>
	public int MemoryLocation { get; private init; }

	/// <summary>
	/// The next memory location for the bytecode to occupy 
	/// </summary>
	public int NextLocation => MemoryLocation + Bytes.Length;

	/// <summary>
	/// The type of directive causing the bytecode to be generated
	/// </summary>
	public DirectiveType DirectiveType { get; private init; }

	/// <summary>
	/// A read-only collection of the tokens that make up the line of source code
	/// </summary>
	public IReadOnlyList<YodaToken> Tokens => _tokens.AsReadOnly();

	/// <summary>
	/// The bytecode representation for the tokens
	/// </summary>
	public byte?[] Bytes { get; private init; }

	#endregion

	#region Fields

	private readonly List<YodaToken> _tokens;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for the bytecode data
	/// </summary>
	/// <param name="lineNumber">The line number of the source code</param>
	/// <param name="memoryLocation">The starting memory location for the bytes</param>
	/// <param name="directiveType">The type of directive handling this set of tokens</param>
	/// <param name="tokens">The tokens that make up this set of instructions on the line of source code</param>
	/// <param name="generatorStrategy">The strategy used to create the actual bytecode</param>
	/// <remarks>
	/// The only valid directives that should be used to write bytecode records are <see cref="DirectiveType.Program"/>
	/// and <see cref="DirectiveType.Data"/>. Any other directive type passing token data is invalid as they do not
	/// result in bytecode being generated
	/// </remarks>
	protected YodaTokenByteCode(int lineNumber, int memoryLocation, DirectiveType directiveType,
		IEnumerable<YodaToken> tokens, IByteCodeGeneratorStrategy generatorStrategy)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lineNumber, 0, nameof(lineNumber));
		ArgumentOutOfRangeException.ThrowIfLessThan(memoryLocation, 0, nameof(memoryLocation));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(memoryLocation, 255, nameof(memoryLocation));
		if (directiveType is not (DirectiveType.Program or DirectiveType.Data))
			throw new ArgumentOutOfRangeException(nameof(directiveType), directiveType,
				$"Invalid directive for bytecode generation");
		ArgumentNullException.ThrowIfNull(tokens);
		ArgumentNullException.ThrowIfNull(generatorStrategy);

		LineNumber = lineNumber;
		_tokens = tokens.ToList();
		DirectiveType = directiveType;
		Bytes = generatorStrategy.Generate(directiveType, _tokens);
	}

	public static YodaTokenByteCode Code(int lineNumber, int memoryLocation, IEnumerable<YodaToken> tokens,
		IByteCodeGeneratorStrategy generatorStrategy)
		=> new YodaTokenByteCode(lineNumber, memoryLocation, DirectiveType.Program, tokens, generatorStrategy);

	public static YodaTokenByteCode Data(int lineNumber, int memoryLocation, IEnumerable<YodaToken> tokens,
		IByteCodeGeneratorStrategy generatorStrategy)
		=> new YodaTokenByteCode(lineNumber, memoryLocation, DirectiveType.Data, tokens, generatorStrategy);

	#endregion

	#region Methods

	public static int CalculateByteCount(int lineNumber, DirectiveType directiveType, IEnumerable<YodaToken> tokens)
	{
		var tokenList = new List<YodaToken>(tokens);
		if (directiveType is DirectiveType.Program)
		{
			return tokenList.Count;
		}
		else
		{
			if (directiveType is DirectiveType.Data)
			{
				var result = 0;

				foreach (var token in tokenList)
					result += token.TokenType switch
					{
						TokenType.LiteralString => token.Text?.Replace("\\", "").Length ?? 0,
						TokenType.LiteralChar => 1,
						TokenType.LiteralNumber => 1,
						TokenType.Symbol => 1,
						_ => throw new YodaByteCodeException(lineNumber, $"Invalid token type: '{token.TokenType}'")
					};
				return result;
			}
		}

		throw new YodaByteCodeException(lineNumber, $"Invalid directive type [{directiveType}]");
	}

	#endregion
}