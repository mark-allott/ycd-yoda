using System.Reflection;
using System.Text;
using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Processor;
using YodaAssembler.Records;
using YodaAssembler.Strategies;

namespace YodaAssembler.Tests.Strategies;

[TestClass]
public class YodaByteCodeGeneratorStrategyTests
{
	#region Private fields

	/// <summary>
	/// A list of the valid parameter types for commands with parameters
	/// </summary>
	private static readonly List<ParameterTypes> ValidParameterTypes =
	[
		ParameterTypes.LiteralNumber,
		ParameterTypes.DirectNumber,
		ParameterTypes.Symbol,
		ParameterTypes.DirectSymbol,
		ParameterTypes.LiteralChar
	];

	/// <summary>
	/// Storage for the valid tokens for parameters (will be assigned in ClassInitialize)
	/// </summary>
	private static readonly Dictionary<ParameterTypes, YodaToken> ParameterTokens = new Dictionary<ParameterTypes, YodaToken>()
	{
		{ ParameterTypes.LiteralNumber, CreateToken(TokenType.LiteralNumber, null) },
		{ ParameterTypes.DirectNumber, CreateToken(TokenType.DirectNumber, null) },
		{ ParameterTypes.Symbol, CreateToken(TokenType.Symbol, null) },
		{ ParameterTypes.DirectSymbol, CreateToken(TokenType.DirectSymbol, null) },
		{ ParameterTypes.LiteralChar, CreateToken(TokenType.LiteralChar, null) },
	};

	/// <summary>
	/// Testable tokens exclude the Unknown and Command types
	/// </summary>
	private static readonly List<TokenType> TestableTokens = Enum.GetValues<TokenType>()
		.Where(t => t is not (TokenType.Unknown or TokenType.Command))
		.ToList();

	private static readonly List<TokenType> DataTokenTypes =
	[
		TokenType.LiteralString,
		TokenType.LiteralChar,
		TokenType.LiteralNumber,
		TokenType.Symbol
	];
	
	#endregion

	#region Private methods

	/// <summary>
	/// For the given <paramref name="tokenType"/>, create the appropriate <seealso cref="YodaToken"/> record
	/// </summary>
	/// <param name="tokenType">The type of the token requested</param>
	/// <param name="text">Optional text to be used for the token creation</param>
	/// <returns>A <seealso cref="YodaToken"/> record or subtype</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private static YodaToken CreateToken(TokenType tokenType, string? text)
	{
		return tokenType switch
		{
			TokenType.Blank => YodaToken.Blank(1, 0),
			TokenType.Comment => YodaToken.Comment(1, 0, text ?? ";a comment"),
			TokenType.Directive => YodaToken.Directive(1, 0, text ?? "[directive]"),
			TokenType.Label => YodaToken.Label(1, 0, text ?? ":_label"),
			TokenType.Operand => YodaToken.Operand(1, 0, text ?? "operand"),
			TokenType.LiteralString => YodaToken.LiteralString(1, 0, text ?? "\"literal string\""),
			TokenType.LiteralChar => YodaToken.LiteralChar(1, 0, text ?? "'c'"),
			TokenType.LiteralNumber => YodaToken.LiteralNumber(1, 0, text ?? $"{Random.Shared.Next(0, 255)}"),
			TokenType.Symbol => YodaToken.Symbol(1, 0, text ?? "symbol"),
			TokenType.DirectNumber => YodaToken.DirectNumber(1, 0, text ?? $"[{Random.Shared.Next(0, 255)}]"),
			TokenType.DirectSymbol => YodaToken.DirectSymbol(1, 0, text ?? "[direct_symbol]"),
			TokenType.IndirectNumber => YodaToken.IndirectNumber(1, 0, text ?? $"[[{Random.Shared.Next(0, 255)}]]"),
			TokenType.IndirectSymbol => YodaToken.IndirectSymbol(1, 0, text ?? "[[indirect_symbol]]"),
			TokenType.Command => YodaCommandToken.Create(1, 0, text ?? "command", YodaCommandSet.Commands[0]),
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
		};
	}

	private static byte ParameterMask(ParameterTypes parameterType)
	{
		return (byte)(parameterType is ParameterTypes.DirectNumber or ParameterTypes.DirectSymbol
			? 0
			: 1);
	}

	private static byte ParameterMask(YodaToken[] tokens)
	{
		byte result = 0;

		foreach (var token in tokens)
		{
			result <<= 1;
			result |= ParameterMask(token.ParameterType);
		}

		return result;
	}

	#endregion

	#region Dynamic data providers

	internal static IEnumerable<object[]> GenerateWithIncorrectTokensData()
	{
		//	Push one of the non-command tokens as the first element in a program
		foreach (var tokenType in TestableTokens)
			yield return [DirectiveType.Program, new YodaToken[] { CreateToken(tokenType, null) }];

		//	Get	a list of commands which expect at least one parameter to be present
		var commandsWithParameters = YodaCommandSet.Commands
			.Where(q => q.ParameterCount > 0)
			.ToList();

		//	Check a command that expects at least one parameter fails with no parameters
		foreach (var command in commandsWithParameters)
			yield return [DirectiveType.Program, new YodaToken[] { YodaCommandToken.Create(1, 0, command.Mnemonic, command) }];

		//	The following are "good" token types for parameters
		var validParameterTokensTypes = new TokenType[]
		{
			TokenType.LiteralChar,
			TokenType.LiteralNumber,
			TokenType.DirectNumber,
			TokenType.Symbol,
			TokenType.DirectSymbol
		};

		//	Construct a list of invalid parameter tokens
		var invalidParameterTokens = Enum.GetValues<TokenType>()
			.Where(t => t is not (TokenType.Unknown or TokenType.Command))
			.Where(t => !validParameterTokensTypes.Contains(t))
			.Select(t => CreateToken(t, null))
			.ToList();

		//	Determine the maximum number of parameters in the commands
		var maxParameters = commandsWithParameters.Max(q => q.ParameterCount);
		//	Create a list of parameter numbers, including 0 and (max+1)
		var paramCounts = Enumerable.Range(0, 2 + maxParameters).ToList();

		//	Construct a list of commands, command tokens and range of parameters to check
		var commandAndParameterCounts = YodaCommandSet.Commands
			.Select(c => new
			{
				Command = c,
				Token = YodaCommandToken.Create(1, 0, c.Mnemonic, c),
				ParamCounts = paramCounts
					.Where(n => n <= c.ParameterCount + 1)
					.Order()
					.ToList()
			})
			.ToList();

		//	Loop for each set of commands and parameter counts to check
		foreach (var c in commandAndParameterCounts)
		foreach (var p in c.ParamCounts)
		{
			//	Commands without parameters will cause the test to fail, so skip
			if (p == 0 && c.Command.ParameterCount == 0)
				continue;
			//	Initialise with the command token itself
			var tokens = new List<YodaToken>([c.Token]);
			//	Add parameter tokens to the list for testing 
			tokens.AddRange(Enumerable.Range(1, p)
				.Select(n =>
					//	create a valid literal number (i.e. a good) token for all parameters where the current
					//	parameter number is not the required number of parameters, or where the test needs to pass
					//	an additional parameter
					n == c.Command.ParameterCount
						//	Throw in a random curveball for the final required parameter
						? invalidParameterTokens[Random.Shared.Next(0, invalidParameterTokens.Count - 1)]
						: CreateToken(TokenType.LiteralNumber, null)));
			yield return [DirectiveType.Program, tokens.ToArray()];
		}
	}

	/// <summary>
	/// Data provider supplying commands with valid parameters for testing against their resultant bytecode
	/// </summary>
	/// <returns>The set of commands and their parameters to be tested</returns>
	internal static IEnumerable<object[]> ValidateMultiParameterCommandByteCodeData()
	{
		var oneParamCommands = YodaCommandSet.Commands
			.Where(q => q.ParameterCount == 1)
			.SelectMany(_ => ValidParameterTypes, (c, p) => new { Command = c, Parameters = new[] { p } })
			.ToList();
		var twoParamCommands = YodaCommandSet.Commands
			.Where(q => q.ParameterCount == 2)
			.SelectMany(_ => ValidParameterTypes, (c, p) => new { Command = c, P1 = p })
			.SelectMany(_ => ValidParameterTypes, (cp1, p2) => new { cp1.Command, Parameters = new[] { cp1.P1, p2 } })
			.ToList();
		var threeParamCommands = YodaCommandSet.Commands
			.Where(q => q.ParameterCount == 3)
			.SelectMany(c => ValidParameterTypes, (c, p1) => new { c, P1 = p1 })
			.SelectMany(_ => ValidParameterTypes, (cp1, p2) => new { cp1.c, cp1.P1, P2 = p2 })
			.SelectMany(_ => ValidParameterTypes, (cp12, p3) => new { Command = cp12.c, Parameters = new[] { cp12.P1, cp12.P2, p3 } })
			.ToList();

		foreach (var c in oneParamCommands)
			yield return [c.Command, c.Parameters.Select(s => ParameterTokens[s]).ToArray()];
		foreach (var c in twoParamCommands)
			yield return [c.Command, c.Parameters.Select(s => ParameterTokens[s]).ToArray()];
		foreach (var c in threeParamCommands)
			yield return [c.Command, c.Parameters.Select(s => ParameterTokens[s]).ToArray()];
	}

	/// <summary>
	/// DisplayName method for the tests that are executed by <see cref="ValidateMultiParameterCommandByteCode"/>
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <param name="data">The parameters under test</param>
	/// <returns>The string representation of the tokens being tested</returns>
	/// <remarks>the data is expected to be in two parts: the first is the command, the second an array of parameters</remarks>
	public static string ValidateMultiParameterCommandByteCodeDisplayName(MethodInfo methodInfo, object[] data)
	{
		//	First element in the data array should be the command
		YodaCommand? command = data[0] as YodaCommand;

		if (command is null)
			return "Command is null";

		//	Add the mnemonic for the command
		var sb = new StringBuilder($"{command.Mnemonic} ");

		//	Check there is an array of parameters to try and interrogate - if not, return with the mnemonic
		if (data.Length == 1 || data[1] is not YodaToken[] paramTokens)
			return sb.ToString().Trim();

		//	Add the ToString representation of each parameter
		foreach (var o in paramTokens)
			sb.Append($"{o} ");
		return sb.ToString().Trim();
	}

	internal static IEnumerable<object[]> GenerateWithIncorrectDataTokensData()
	{
		//	Work out which tokens are non-data ones
		var nonDataTokenTypes = TestableTokens
			.Where(t => !DataTokenTypes.Contains(t))
			.ToList();

		//	Push one of the non-data tokens as the first element in a data area
		foreach (var tokenType in nonDataTokenTypes)
			yield return [DirectiveType.Data, new[] { CreateToken(tokenType, null) }];
	}

	public record DataByteCodeTestData
	{
		public required YodaToken Token { get; init; }
		public required int ExpectedLength {get; init;}
		public required byte?[] ExpectedData { get; init; }
	}
	
	internal static IEnumerable<object[]> GenerateValidDataByteCodeData()
	{
		//	Single blank string
		var blankLiteralString = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralString, "\"\""),
			ExpectedLength = 0,
			ExpectedData = []
		};
		//	Single-character string of a control-character
		var controlCharLiteralString = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralString, "\"\\t\""),
			ExpectedLength = 1,
			ExpectedData = [9]
		};
		//	A larger segment of text
		var textBlockLiteralString = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralString, "\"This is some text\""),
			ExpectedLength = 17,
			ExpectedData = [84, 104, 105, 115, 32, 105, 115, 32, 115, 111, 109, 101, 32, 116, 101, 120, 116]
		};
		//	Literal character of a control character
		var controlCharacterLiteralChar = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralChar, "'\\n'"),
			ExpectedLength = 1,
			ExpectedData = [10],
		};
		var decimalNumber = new DataByteCodeTestData
		{
			Token = CreateToken(TokenType.LiteralNumber, "123"),
			ExpectedLength = 1,
			ExpectedData = [123]
		};
		var hexNumber = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralNumber, "0x12"),
			ExpectedLength = 1,
			ExpectedData = [0x12]
		};
		var binaryNumber170 = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralNumber, "0b10101010"),
			ExpectedLength = 1,
			ExpectedData = [170]
		};
		var binaryNumber153 = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.LiteralNumber, "0b1001_1001"),
			ExpectedLength = 1,
			ExpectedData = [153]
		};
		var symbol = new DataByteCodeTestData()
		{
			Token = CreateToken(TokenType.Symbol, "symbol_name"),
			ExpectedLength = 1,
			ExpectedData = [null]
		};

		yield return [new[] { blankLiteralString }];
		yield return [new[] { controlCharLiteralString }];
		yield return [new[] { textBlockLiteralString }];
		yield return [new[] { controlCharacterLiteralChar }];
		yield return [new[] { decimalNumber }];
		yield return [new[] { hexNumber }];
		yield return [new[] { binaryNumber170 }];
		yield return [new[] { binaryNumber153 }];
		yield return [new[] { decimalNumber, controlCharacterLiteralChar, textBlockLiteralString }];
		yield return [new[] { symbol }];
	}
	
	#endregion

	[TestMethod]
	public void GenerateWithNullTokensThrowsException()
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		Assert.ThrowsException<ArgumentNullException>(() => sut.Generate(DirectiveType.Unknown, null!));
	}

	[TestMethod]
	public void GenerateWithNoTokensThrowsException()
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => sut.Generate(DirectiveType.Unknown, []));
	}

	[TestMethod]
	[DataRow(DirectiveType.Unknown)]
	[DataRow(DirectiveType.Constants)]
	[DataRow(DirectiveType.Const)]
	public void GenerateWithIncorrectDirectiveThrowsException(DirectiveType directiveType)
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => sut.Generate(directiveType, []));
	}

	[TestMethod]
	[DynamicData(nameof(GenerateWithIncorrectTokensData), DynamicDataSourceType.Method)]
	public void GenerateWithIncorrectTokensThrowsException(DirectiveType directiveType, YodaToken[] tokens)
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		Assert.ThrowsException<TokeniserException>(() => sut.Generate(directiveType, tokens));
	}

	[TestMethod]
	[DynamicData(nameof(ValidateMultiParameterCommandByteCodeData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(ValidateMultiParameterCommandByteCodeDisplayName))]
	public void ValidateMultiParameterCommandByteCode(YodaCommand command, YodaToken[] paramTokens)
	{
		byte expectedOpCode = command.OpCode;
		expectedOpCode |= ParameterMask(paramTokens);

		var allTokens = new List<YodaToken>([YodaCommandToken.Create(1, 0, command.Mnemonic, command)]);
		allTokens.AddRange(paramTokens);
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		var byteCode = sut.Generate(DirectiveType.Program, allTokens.ToArray());

		//	First check: the bytecode has expected size
		byteCode.Should().NotBeNull();
		byteCode.Length.Should().Be(allTokens.Count);
		//	First byte of the array is the command
		byteCode[0].Should().Be(expectedOpCode);

		//	Subsequent bytes are parameters and should be as expected:
		//	Literal values are the underlying numeric value
		//	Direct values are the value of the underlying numeric value
		//	Symbolic values are null at this stage
		var offset = 1;
		foreach (var token in paramTokens)
		{
			byte? expectedByte = token.TokenType switch
			{
				TokenType.LiteralChar => (byte)char.Parse(ParameterTokens[ParameterTypes.LiteralChar].Text ?? ""),
				TokenType.LiteralNumber => (byte?)((ParameterTokens[ParameterTypes.LiteralNumber] as YodaNumericValueToken)?.NumericValue ?? null),
				TokenType.DirectNumber => (byte?)((ParameterTokens[ParameterTypes.DirectNumber] as YodaCompositeToken)?.InnerNumericValue ?? null),
				TokenType.Symbol or
					TokenType.DirectSymbol => null,
				_ => throw new ArgumentOutOfRangeException()
			};
			byteCode[offset].Should().Be(expectedByte);
			
			offset++;
		}
	}

	[TestMethod]
	[DynamicData(nameof(GenerateWithIncorrectDataTokensData), DynamicDataSourceType.Method)]
	public void GenerateWithIncorrectDataTokensThrowsException(DirectiveType directiveType, YodaToken[] tokens)
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		Assert.ThrowsException<TokeniserException>(() => sut.Generate(directiveType, tokens));
	}

	[TestMethod]
	[DynamicData(nameof(GenerateValidDataByteCodeData), DynamicDataSourceType.Method)]
	public void ValidateDataByteCode(DataByteCodeTestData[] testData)
	{
		YodaByteCodeGeneratorStrategy sut = new YodaByteCodeGeneratorStrategy();
		var testTokens = testData.Select(s => s.Token).ToArray();
		var expectedLength = testData.Sum(d => d.ExpectedLength);
		var expectedBytes = testData.SelectMany(d => d.ExpectedData).ToArray();

		expectedBytes.Length.Should().Be(expectedLength);
		
		byte?[] actualBytes = sut.Generate(DirectiveType.Data, testTokens);
		actualBytes.Should().NotBeNull();
		actualBytes.Length.Should().Be(expectedLength);
		actualBytes.Should().ContainInConsecutiveOrder(expectedBytes);
	}
}