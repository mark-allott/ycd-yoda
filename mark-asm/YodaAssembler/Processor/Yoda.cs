using System.Diagnostics.CodeAnalysis;
using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Interfaces;
using YodaAssembler.Records;
using YodaAssembler.Strategies;

namespace YodaAssembler.Processor;

public class Yoda
	: IParser<YodaToken>, ICompiler
{
	#region Properties

	private string DefaultFilePath { get; }

	private IYodaTokeniserStrategy TokeniserStrategy { get; }

	private List<SourceLine> SourceLines { get; set; } = [];

	public IReadOnlyList<SourceLine> Lines => SourceLines.AsReadOnly();

	private static readonly List<YodaCommand> YodaCommands = YodaCommandSet.Commands;

	private IByteCodeGeneratorStrategy GeneratorStrategy { get; }

	#endregion

	#region Fields

	/// <summary>
	/// Holds the definitions for constant values
	/// </summary>
	private Dictionary<string, YodaToken?> Symbols { get; } = new();

	private readonly byte[] _bootFileData = new byte[256];

	public IReadOnlyCollection<byte> BootFileData => _bootFileData.AsReadOnly();

	#endregion

	#region Constructors

	/// <summary>
	/// Simplified constructor - requires the default path for files to be defined 
	/// </summary>
	/// <param name="defaultFilePath">The path where files are normally stored; used as a base for relative-path reading/writing</param>
	/// <remarks>Auto-defines the strategies used for tokenisation and transformation to bytecode</remarks>
	public Yoda(string defaultFilePath)
		: this(defaultFilePath, new YodaTokeniserStrategy(YodaCommands), new YodaByteCodeGeneratorStrategy())
	{
	}

	/// <summary>
	/// Standard Constructor
	/// </summary>
	/// <param name="defaultFilePath">The path where files are normally stored; used as the base for relative-path read/write operations</param>
	/// <param name="tokeniserStrategy">The class used to perform tokenisation of the source code into <see cref="YodaToken"/> elements</param>
	/// <param name="generatorStrategy">The class used to perform the transformation from <see cref="YodaToken"/> elements into physical bytecode</param>
	public Yoda(string defaultFilePath, IYodaTokeniserStrategy tokeniserStrategy,
		IByteCodeGeneratorStrategy generatorStrategy)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(defaultFilePath);
		ArgumentNullException.ThrowIfNull(tokeniserStrategy, nameof(tokeniserStrategy));
		ArgumentNullException.ThrowIfNull(generatorStrategy, nameof(generatorStrategy));
		DefaultFilePath = defaultFilePath;
		TokeniserStrategy = tokeniserStrategy;
		GeneratorStrategy = generatorStrategy;
	}

	#endregion

	#region IParser<YodaToken> Members

	public IEnumerable<YodaToken> Parse(IEnumerable<string> lines)
	{
		SourceLines = lines.Select((l, i) => new SourceLine(i + 1, l))
			.ToList();
		return TokeniserStrategy.Tokenise(Lines);
	}

	[ExcludeFromCodeCoverage]
	public IEnumerable<YodaToken> ParseFile(string filePath)
	{
		string fqn = File.Exists(filePath)
			? filePath
			: File.Exists(Path.Combine(DefaultFilePath, filePath))
				? Path.Combine(DefaultFilePath, filePath)
				: throw new FileNotFoundException($"File {filePath} does not exist");

		return Parse(File.ReadAllLines(fqn));
	}

	#endregion

	#region ICompiler implementation

	/// <inheritdoc />
	public void Compile(IEnumerable<string> lines)
	{
		var tokens = Parse(lines);
		InternalCompile(tokens);
	}

	/// <inheritdoc />
	[ExcludeFromCodeCoverage]
	public void Compile(string fileName)
	{
		var tokens = ParseFile(fileName);
		InternalCompile(tokens);
	}

	/// <inheritdoc />
	public IEnumerable<YodaTokenByteCode> CompileToByteCode(IEnumerable<YodaToken> tokens)
	{
		//	Ensure the tokens are converted to a list to prevent multiple enumerations over them.
		//	Also making	sure they are all in the correct serial sequence
		var lineTokens = tokens
			.Where(t => t.TokenType is not TokenType.Comment)
			.GroupBy(g => g.LineNumber)
			.Select(g => new
			{
				LineNumber = g.Key,
				Tokens = g.OrderBy(o => o.LineSequence).ToList()
			})
			.ToList();

		var instructionPosition = 0;
		var dataPosition = 0;
		var currentDirective = DirectiveType.Unknown;
		var byteCode = new List<YodaTokenByteCode>();

		foreach (var line in lineTokens)
		{
			var token = line.Tokens.First();

			//	Skip blank lines
			if (token.TokenType is TokenType.Blank)
				continue;
			
			//	Handle changes to the directive
			if (token is YodaDirectiveToken directiveToken)
			{
				currentDirective = HandleDirectiveChange(directiveToken.DirectiveType, line.Tokens,
					ref instructionPosition, ref dataPosition);
				continue;
			}
			
			switch (currentDirective)
			{
				case DirectiveType.Program:
				case DirectiveType.Prog:
				case DirectiveType.Code:
					if (token is YodaCommandToken commandToken)
					{
						//	Ensure the correct number of tokens for the line exist for the number of parameters required
						if (line.Tokens.Count != commandToken.YodaCommand.ParameterCount + 1)
							throw new TokeniserException(line.LineNumber, $"Incorrect number of tokens for command");

						//	Generate the instruction and update instruction pointer
						var instruction = YodaTokenByteCode.Code(line.LineNumber, instructionPosition, line.Tokens,
							GeneratorStrategy);
						byteCode.Add(instruction);
						instructionPosition = instruction.NextLocation;

						//	Check if the data position is below the instruction pointer - if so, move the data pointer as well
						if (dataPosition < instructionPosition)
							dataPosition = instructionPosition;
					}
					else if (token.TokenType is TokenType.Label)
					{
						SetSymbolValue(line.Tokens.ToArray(), instructionPosition);
					}
					else
					{
						throw new TokeniserException(line.LineNumber,
							$"Invalid token type found in code block: {token.TokenType}");
					}

					break;
				case DirectiveType.Data:
					if (token.TokenType is TokenType.Label)
					{
						SetSymbolValue(line.Tokens.ToArray(), dataPosition);
					}
					else
					{
						var dataBytecode = YodaTokenByteCode.Data(line.LineNumber, dataPosition, line.Tokens,
							GeneratorStrategy);
						byteCode.Add(dataBytecode);
						dataPosition = dataBytecode.NextLocation;
					}

					//	Check if the data position is below the instruction pointer - if so, move the data pointer as well
					if (instructionPosition < dataPosition)
						instructionPosition = dataPosition;

					break;
				case DirectiveType.Constants:
				case DirectiveType.Const:
					SetSymbolValue(line.Tokens.ToArray());
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		return byteCode;
	}

	#endregion

	#region Methods implementing the actual compilation of YodaTokens to bytecode

	/// <summary>
	/// Internal method to perform the major steps in converting the <paramref name="tokens"/> into a bytecode representation
	/// </summary>
	/// <param name="tokens">The source of the program in tokenised form</param>
	/// <remarks>If successful, the result is stored in the private field <see cref="_bootFileData"/>, also publicly accessible in a read-only form via the <see cref="BootFileData"/> property</remarks>
	private void InternalCompile(IEnumerable<YodaToken> tokens)
	{
		//	Ensure environment is "clean"
		Symbols.Clear();
		_bootFileData.Initialize();

		var tokenList = tokens
			.OrderBy(o => o.LineNumber)
			.ThenBy(o => o.LineSequence)
			.ToList();

		//	Find and initialise the list of labels and symbols
		InitialiseSymbols(tokenList);

		//	Pass 1 over the tokenised source to generate the framework for the program layout 
		var byteCodeTokens = CompilePass1(tokenList);

		//	Pass 2 needs to check if any symbols are still undefined
		CompilePass2(tokenList);

		//	Pass 3 needs to update the underlying bytecode for symbol values
		CompilePass3(byteCodeTokens);

		//	Pass 4 needs to check that the bytecode created doesn't end up overwriting areas
		byteCodeTokens = CompilePass4(byteCodeTokens);

		//	Final step - write the bytecode into the bootfile array
		foreach (var token in byteCodeTokens)
			Array.Copy(token.Bytes.Cast<byte>().ToArray(), 0, _bootFileData, token.MemoryLocation, token.Bytes.Length);
	}

	/// <summary>
	/// For the given <paramref name="tokens"/>, initialise the <see cref="Symbols"/> dictionary, ready for updating
	/// </summary>
	/// <param name="tokens">The tokenised version of the source</param>
	public void InitialiseSymbols(IEnumerable<YodaToken> tokens)
	{
		//	Ensure the tokens are converted to a list to prevent multiple enumerations over them.
		//	Also making	sure they are all in the correct serial sequence
		var tokenList = tokens
			.OrderBy(o => o.LineNumber)
			.ThenBy(o => o.LineSequence)
			.ToList();

		//	Find all unique symbolic elements - includes definitions and usage
		var symbolList = tokenList.Where(t => t.TokenType is TokenType.Symbol or TokenType.Label)
			.Select(t => new { SymbolName = t.Text ?? "", Token = t })
			.DistinctBy(d => d.SymbolName)
			.ToList();

		//	Check for a blank symbol being inserted into the definitions
		var blankSymbol = symbolList.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.SymbolName));
		if (blankSymbol is not null)
			throw new TokeniserException(blankSymbol.Token.LineNumber, $"Cannot have a blank for a symbol"); 

		//	Grab a list of the commands defined in the commandset
		var commandList = YodaCommands.Select(s => s.Mnemonic).ToList();
		
		//	Symbols should NOT match any of the commands defined, irrespective case
		var invalidSymbols = symbolList
			.Join(commandList,
				o => o.SymbolName.ToLowerInvariant(),
				i => i.ToLowerInvariant(),
				(s, c) => new { Command = c, s.SymbolName, s.Token })
			.ToList();
		
		//	Check if any were matched
		if(invalidSymbols.Count > 0)
		{
			var invalidSymbol =  invalidSymbols.First();
			throw new TokeniserException(invalidSymbol.Token.LineNumber, $"'{invalidSymbol.SymbolName}' is a reserved symbol");
		}

		//	All seems to be good, so initialise the symbol names with null values
		symbolList.ForEach(s => Symbols.TryAdd(s.SymbolName, null));
	}

	/// <summary>
	/// Performs the first pass of the compiler across the tokenised source
	/// </summary>
	/// <param name="tokens">The tokenised version of the sourcecode</param>
	/// <returns>The intermediate stage of compilation, where the <see cref="YodaToken"/> elements have been transformed
	/// into their <see cref="YodaTokenByteCode"/> equivalents</returns>
	private List<YodaTokenByteCode> CompilePass1(List<YodaToken> tokens)
	{
		return CompileToByteCode(tokens)
			.ToList();
	}

	/// <summary>
	/// Performs pass 2 over the tokenised source - checking for any symbols that are undefined
	/// </summary>
	/// <param name="tokens">The tokenised source</param>
	/// <exception cref="TokeniserException"></exception>
	/// <exception cref="AggregateException"></exception>
	private void CompilePass2(List<YodaToken> tokens)
	{
		//	Locate any undefined symbols
		var undefinedSymbols = Symbols
			.Where(q => q.Value is null)
			.Select(d => d.Key)
			.ToList();

		//	If none found, return now
		if (undefinedSymbols.Count == 0)
			return;

		//	Build the tokeniser exception list for missing symbols
		var tokeniserExceptions = tokens
			.Where(q => q.TokenType is TokenType.Label or TokenType.Symbol)
			.Where(q => undefinedSymbols.Contains(q.Text ?? ""))
			.OrderBy(o => o.LineNumber)
			.ThenBy(o => o.LineSequence)
			.Select(q => new TokeniserException(q.LineNumber, $"Undefined {q.TokenType}: '{q.Text}'"))
			.ToList();

		//	If there's only one to report, raise it as a TokeniserException
		if (tokeniserExceptions.Count == 1)
			throw tokeniserExceptions[0];

		//	Multiple problems need an AggregateException with all problems listed
		throw new AggregateException("Multiple undefined symbols found", tokeniserExceptions);
	}

	/// <summary>
	/// Performs pass 3 of the compiler over the tokenised source, checking for any symbol references that need to be updated with the correct values
	/// </summary>
	/// <param name="tokens">The intermediate tokens for the source</param>
	/// <exception cref="YodaByteCodeException"></exception>
	private void CompilePass3(List<YodaTokenByteCode> tokens)
	{
		//	Step 1 is to locate any needing updates...
		var symbolsToUpdate = tokens
			.Where(q => q.Bytes.Any(b => b is null))
			.ToList();

		//	If nothing to update, quit now
		if (symbolsToUpdate.Count == 0)
			return;

		//	Iterate over each token needing updates
		foreach (var token in symbolsToUpdate)
		{
			if (token.Tokens.Count != token.Bytes.Length)
				throw new YodaByteCodeException(token.LineNumber, $"Mismatch in token and byte counts");

			//	Loop over each byte to find nulls
			for (var i = 0; i < token.Bytes.Length; i++)
			{
				if (token.Bytes[i] is not null)
					continue;

				if (!Symbols.TryGetValue(token.Tokens[i].Text!, out var valueToken))
					throw new YodaByteCodeException(token.LineNumber, $"Undefined symbol: '{token.Tokens[i].Text}'");
				if (valueToken is not YodaNumericValueToken numericToken)
					throw new YodaByteCodeException(token.LineNumber,
						$"Invalid token type for symbol: {valueToken?.TokenType ?? TokenType.Unknown}");
				token.Bytes[i] = (byte)numericToken.NumericValue;
			}
		}
	}

	/// <summary>
	/// Performs sanity checking for the given intermediate tokens
	/// </summary>
	/// <param name="tokens">The tokenised version of the source</param>
	/// <returns></returns>
	/// <exception cref="YodaByteCodeException"></exception>
	/// <remarks>This final stage of the compile process iterates over the tokens, placing them in memory location order
	/// and ensures that there are no overwriting areas and that the total length of the resultant bytecode would not
	/// exceed 256 bytes</remarks>
	private static List<YodaTokenByteCode> CompilePass4(List<YodaTokenByteCode> tokens)
	{
		//	Order the tokens by memory location
		var orderedTokens = tokens
			.OrderBy(o => o.MemoryLocation)
			.ToList();

		//	Start at location zero - first program element should be there
		var nextLocation = 0;
		foreach (var token in orderedTokens)
		{
			//	Any bytecode generated should either be a continuation of previous elements, or skip ahead to a new memory location
			if (nextLocation > token.MemoryLocation)
				throw new YodaByteCodeException(token.LineNumber,
					$"Token results in overwriting bytecode from another line");
			nextLocation = token.NextLocation;

			if (nextLocation > 256)
				throw new YodaByteCodeException(token.LineNumber,
					"Token results in writing past the end of the permitted length");
		}

		return orderedTokens;
	}

	#endregion

	public void WriteToBootFile(string fileName)
	{
		File.WriteAllBytes(Path.Combine(DefaultFilePath, fileName), _bootFileData);
	}

	private int GetDirectiveParameterValue(YodaToken token, DirectiveType directiveType)
	{
		if (token is YodaNumericValueToken numericToken)
			return numericToken.NumericValue;
		if (token.TokenType is TokenType.Symbol)
			return GetSymbolValue(token);
		throw new DirectiveException(directiveType,
			$"Invalid token type for [{directiveType}] parameter: {token.TokenType}");
	}

	private int GetSymbolValue(YodaToken token)
	{
		ArgumentNullException.ThrowIfNull(token, nameof(token));
		
		if(string.IsNullOrEmpty(token.Text))
			throw new TokeniserException(token.LineNumber, "Invalid symbol text");

		if (!Symbols.TryGetValue(token.Text, out var value))
			throw new TokeniserException(token.LineNumber, $"Unknown symbol: '{token.Text}'");

		if (value is null)
			throw new YodaByteCodeException(token.LineNumber, $"Symbol '{token.Text}' is undefined");

		if (value is YodaNumericValueToken numericToken)
			return numericToken.NumericValue;

		if (value.TokenType is TokenType.Symbol)
			return GetSymbolValue(value);

		throw new TokeniserException(token.LineNumber, "Invalid token for symbol");
	}

	private DirectiveType HandleDirectiveChange(DirectiveType newDirective, List<YodaToken> tokens,
		ref int instructionPosition, ref int dataPosition)
	{
		switch (newDirective)
		{
			case DirectiveType.Program:
				instructionPosition = tokens.Count switch
				{
					1 => instructionPosition,
					2 => GetDirectiveParameterValue(tokens[^1], DirectiveType.Program),
					_ => throw new DirectiveException(DirectiveType.Program,
						$"{newDirective} has more parameters than permitted"),
				};
				break;
			case DirectiveType.Data:
				dataPosition = tokens.Count switch
				{
					1 => dataPosition,
					2 => GetDirectiveParameterValue(tokens[^1], DirectiveType.Data),
					_ => throw new DirectiveException(DirectiveType.Data,
						$"{newDirective} has more parameters than permitted")
				};
				break;
			case DirectiveType.Constants:
				if (tokens.Count != 1)
					throw new DirectiveException(DirectiveType.Constants,
						$"{newDirective} has more parameters than permitted");
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(newDirective), newDirective,
					$"Invalid {nameof(DirectiveType)} value");
		}

		return newDirective;
	}

	private void SetSymbolValue(YodaToken[] tokens, int numericValue = 0)
	{
		//	Tokens passed in MUST be present and there MUST be either one or two present
		ArgumentNullException.ThrowIfNull(tokens, nameof(tokens));
		ArgumentOutOfRangeException.ThrowIfZero(tokens.Length, nameof(tokens));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(tokens.Length, 2, nameof(tokens));

		var labelToken = tokens[0];
		var tokenText = labelToken.Text ?? "";
		//	Cannot have a blank symbol name
		ArgumentException.ThrowIfNullOrWhiteSpace(tokenText, nameof(tokenText));

		//	Check the token symbol has been detected during InitialiseSymbols
		if (!Symbols.TryGetValue(tokenText, out var tokenValue))
			throw new TokeniserException(labelToken.LineNumber, $"Unknown label / symbol: '{tokenText}'");
		//	Any existing value for the symbol indicates a duplicate definition
		if (tokenValue is not null)
			throw new TokeniserException(labelToken.LineNumber, $"Duplicate definition for label '{tokenText}'");

		//	Assign a value for the token
		switch (labelToken.TokenType)
		{
			//	A label needs to assign the value passed to the symbol
			case TokenType.Label:
				tokenValue = YodaToken.LiteralNumber(labelToken.LineNumber, 0, $"{numericValue}");
				break;
			//	If tokens[0] is a symbol, then it is a constant definition and the value of tokens[1] should be used as the value
			case TokenType.Symbol:
				if (tokens.Length != 2)
					throw new TokeniserException(labelToken.LineNumber, $"Incorrect number of tokens for constant");
				tokenValue = tokens[1];
				break;
			default:
				throw new TokeniserException(labelToken.LineNumber, $"Invalid symbol data provided");
		}

		Symbols[tokenText] = tokenValue;
	}
}