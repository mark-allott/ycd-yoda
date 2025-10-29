using System.Reflection;
using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Interfaces;
using YodaAssembler.Processor;
using YodaAssembler.Records;
using YodaAssembler.Strategies;

namespace YodaAssembler.Tests.Processor;

public partial class YodaTests
{
	#region Dynamic test record types

	public record DynamicDataTest
	{
		public bool ShouldFail { get; init; } = true;
		public string DisplayName { get; init; } = "";
	}

	public record ConstructorTest
		: DynamicDataTest
	{
		public required string? Path { get; init; }
		public required IYodaTokeniserStrategy Tokeniser { get; init; }
		public required IByteCodeGeneratorStrategy Generator { get; init; }
	}

	public record SourceCodeDataTest
		: DynamicDataTest
	{
		public required List<string> Lines { get; init; }
		public Type? ExpectedExceptionType { get; init; }
	}

	public record ParseTestData
		: SourceCodeDataTest
	{
		public int ExpectedTokenCount => ExpectedTokenTypes?.Length ?? 0;
		public TokenType[]? ExpectedTokenTypes { get; init; }
	}

	public record CompileTestData
		: SourceCodeDataTest
	{
		public byte[] ExpectedBytes { get; init; } = new byte[256];

		public CompileTestData()
		{
			ExpectedBytes.Initialize();
		}
	}

	public record CompileToByteCodeTestData
		: DynamicDataTest
	{
		public Type? ExpectedExceptionType { get; init; }
		public List<YodaToken> Tokens { get; init; } = [];
		public List<YodaTokenByteCode>? ByteCodeTokens { get; init; }
		public Yoda Processor { get; } = new Yoda(".");
	}

	#endregion

	#region Utility methods

	public static string DynamicDataTestDisplayNameProvider(MethodInfo methodInfo, object[] data)
	{
		var testMethodName = methodInfo.Name;
		if (data.Length == 0)
			return $"{testMethodName}: Empty data";

		if (data[0] is DynamicDataTest test)
			return $"{testMethodName}: {test.DisplayName}";
		return $"{testMethodName}: Not a valid {nameof(DynamicDataTest)} object";
	}

	#endregion

	#region Providers for constructor testing

	internal static IEnumerable<object[]> ValidateConstructorTestData()
	{
		yield return
		[
			new ConstructorTest
			{
				Path = null,
				Tokeniser = null!,
				Generator = null!,
				DisplayName = "null DefaultPath"
			}
		];
		yield return
		[
			new ConstructorTest
			{
				Path = "",
				Tokeniser = null!,
				Generator = null!,
				DisplayName = "empty DefaultPath"
			}
		];
		yield return
		[
			new ConstructorTest
			{
				Path = "  ",
				Tokeniser = null!,
				Generator = null!,
				DisplayName = "whitespace DefaultPath"
			}
		];
		yield return
		[
			new ConstructorTest
			{
				Path = ".",
				Tokeniser = null!,
				Generator = null!,
				DisplayName = "null TokeniserStrategy"
			}
		];
		yield return
		[
			new ConstructorTest
			{
				Path = ".",
				Tokeniser = new YodaTokeniserStrategy([]),
				Generator = null!,
				DisplayName = "null GeneratorStrategy"
			}
		];
		yield return
		[
			new ConstructorTest
			{
				Path = ".",
				Tokeniser = new YodaTokeniserStrategy([]),
				Generator = new YodaByteCodeGeneratorStrategy(),
				DisplayName = "All valid",
				ShouldFail = false
			}
		];
	}

	#endregion

	#region Providers for Parse testing

	internal static IEnumerable<object[]> ParseTestDataProvider()
	{
		yield return
		[
			new ParseTestData
			{
				Lines = null!,
				DisplayName = "null Lines",
				ExpectedExceptionType = typeof(ArgumentNullException),
			}
		];
		yield return
		[
			new ParseTestData
			{
				Lines = [],
				DisplayName = "empty Lines",
				ExpectedExceptionType = typeof(DirectiveException),
			}
		];
		yield return
		[
			new ParseTestData
			{
				Lines = [""],
				DisplayName = "One blank line",
				ExpectedExceptionType = typeof(DirectiveException),
			}
		];
		yield return
		[
			new ParseTestData
			{
				Lines = ["[data]"],
				DisplayName = "[data] only",
				ExpectedExceptionType = typeof(DirectiveException),
			}
		];
		yield return
		[
			new ParseTestData
			{
				ShouldFail = false,
				DisplayName = "[Code] only",
				Lines = ["[code]"],
				ExpectedTokenTypes = [TokenType.Directive],
			}
		];
		yield return
		[
			new ParseTestData
			{
				ShouldFail = false,
				DisplayName = "blank line, then [code]",
				Lines = ["", "[code]"],
				ExpectedTokenTypes = [TokenType.Blank, TokenType.Directive],
			}
		];
		yield return
		[
			new ParseTestData
			{
				ShouldFail = false,
				DisplayName = "[Code] 0x10",
				Lines = ["[code] 0x10"],
				ExpectedTokenTypes = [TokenType.Directive, TokenType.LiteralNumber],
			}
		];
		yield return
		[
			new ParseTestData
			{
				ShouldFail = false,
				DisplayName = "[Code] 0x10 | wait",
				Lines = ["[code] 0x10", "wait"],
				ExpectedTokenTypes = [TokenType.Directive, TokenType.LiteralNumber, TokenType.Command],
			}
		];
		yield return
		[
			new ParseTestData
			{
				ShouldFail = false,
				DisplayName = "[const] | [code] | :label | cmd",
				Lines =
				[
					"[const]",
					"x=123",
					"[code] 0x10",
					":start_of_program",
					"wait",
					"inc x"
				],
				ExpectedTokenTypes =
				[
					TokenType.Directive,
					TokenType.Symbol, TokenType.LiteralNumber,
					TokenType.Directive, TokenType.LiteralNumber,
					TokenType.Label,
					TokenType.Command,
					TokenType.Command, TokenType.Symbol
				],
			}
		];
	}

	#endregion

	#region Providers for Compile testing

	//	Extract commands to use for tests
	private static readonly YodaCommand HaltCommand = YodaCommandSet.Commands.First(q => q.Mnemonic == "halt");
	private static readonly YodaCommand WaitCommand = YodaCommandSet.Commands.First(q => q.Mnemonic == "wait");

	private static readonly YodaCommand JumpIfZeroCommand =
		YodaCommandSet.Commands.First(q => q.Mnemonic == "jumpifzero");

	private static IEnumerable<CompileTestData> InitialiseSymbolsTestProvider()
	{
		//	Valid program, should perform the following:
		//		Define data_area with value 0x40
		//		yield the wait command and halt command in positions 0 & 1
		//		Move the [data] area to 0x40
		//		Write Text starting at position 0x40
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "InitialiseSymbols: [code] | wait | halt | [data] data_area",
			Lines =
			[
				"[const]",
				"data_area = 0x40",
				"[code]",
				WaitCommand.Mnemonic,
				HaltCommand.Mnemonic,
				"",
				"[data] data_area",
				"\"Text\""
			],
			ExpectedBytes =
			{
				[0] = WaitCommand.OpCode,
				[1] = HaltCommand.OpCode,
				[0x40] = (byte)'T',
				[0x41] = (byte)'e',
				[0x42] = (byte)'x',
				[0x43] = (byte)'t'
			}
		};
		//	Invalid program - symbol 'b' is undefined
		yield return new CompileTestData
		{
			DisplayName = "InitialiseSymbols: [code] with undefined symbol",
			Lines =
			[
				"[const]",
				"a = b",
				"[code] b",
				WaitCommand.Mnemonic
			],
			ExpectedExceptionType = typeof(YodaByteCodeException),
		};

		//	Valid program, should yield the following:
		//		Set symbol a to the value of symbol b
		//		Set symbol b to the value of symbol c
		//		Set symbol c to the value 50
		//		Set the code position to 50
		//		Write the wait command to position 50
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "InitialiseSymbols: [code] with recursive symbol",
			Lines =
			[
				"[const]",
				"\ta = b",
				"\tb=c",
				"\tc = 50",
				"[code] c",
				WaitCommand.Mnemonic
			],
			ExpectedBytes =
			{
				[50] = WaitCommand.OpCode
			}
		};

		//	Invalid program - symbol 'a' is redefined
		yield return new CompileTestData
		{
			DisplayName = "InitialiseSymbols: [code] with redefined symbol",
			Lines =
			[
				"[const]",
				"a = b",
				" a = 10",
				"[code] a",
				WaitCommand.Mnemonic
			],
			ExpectedExceptionType = typeof(TokeniserException),
		};

		//	Make sure reserved words in the commandset are not valid symbols/labels
		foreach (var command in YodaCommandSet.Commands)
		{
			yield return new CompileTestData()
			{
				DisplayName = $"InitialiseSymbols: [code] with reserved symbol '{command.Mnemonic}'",
				Lines =
				[
					"[const]",
					$"\t{command.Mnemonic} = 0xff",
					"",
					"[code]"
				],
				ExpectedExceptionType = typeof(TokeniserException)
			};
			yield return new CompileTestData()
			{
				DisplayName = $"InitialiseSymbols: [code] with reserved symbol '{command.Mnemonic}' as label",
				Lines =
				[
					"[code]",
					$":{command.Mnemonic}",
					WaitCommand.Mnemonic
				],
				ExpectedExceptionType = typeof(TokeniserException)
			};
		}
	}

	private static IEnumerable<CompileTestData> CompileGoodProgramTestProvider()
	{
		//	Valid program, should yield 256 zero-bytes as the result
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "No code",
			Lines = ["[code]"],
		};

		//	Valid program, should yield a single non-zero byte at position 0
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "[code] | wait",
			Lines = ["[code]", WaitCommand.Mnemonic],
			ExpectedBytes =
			{
				[0] = WaitCommand.OpCode
			}
		};

		//	Valid program, should yield a single non-zero byte at position 0x10 / 16
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "[code] 0x10 | wait",
			Lines = ["[code] 0x10", WaitCommand.Mnemonic],
			ExpectedBytes =
			{
				[0x10] = WaitCommand.OpCode
			}
		};

		//	Valid program, should yield the wait command and halt command in positions 0 & 1, followed by Text
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "[code] | wait | halt | [data]",
			Lines =
			[
				"[code]",
				WaitCommand.Mnemonic,
				HaltCommand.Mnemonic,
				"[data]",
				"\"Text\""
			],
			ExpectedBytes =
			{
				[0] = WaitCommand.OpCode,
				[1] = HaltCommand.OpCode,
				[2] = (byte)'T',
				[3] = (byte)'e',
				[4] = (byte)'x',
				[5] = (byte)'t'
			}
		};

		//	Valid program, should yield:
		//		the wait command in position 0
		//		the halt command in position 1
		//		the [data] area moved to 0x20
		//		the word Text written from byte 0x20
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "[code] | wait | halt | [data] 0x20",
			Lines =
			[
				"[code]",
				WaitCommand.Mnemonic,
				HaltCommand.Mnemonic,
				"",
				"[data] 0x20",
				"\"Text\""
			],
			ExpectedBytes =
			{
				[0] = WaitCommand.OpCode,
				[1] = HaltCommand.OpCode,
				[0x20] = (byte)'T',
				[0x21] = (byte)'e',
				[0x22] = (byte)'x',
				[0x23] = (byte)'t'
			}
		};

		//	Valid program, should yield:
		//		the wait command in position 0
		//		the jumpifzero command in position 1, with 0x40 as address to check and 0x00 as jump location
		//		the halt command in position 4
		//		the [data] area moved to 0x20
		//		the word Text written from byte 0x20
		//		explicitly set address 0x40 as zero
		yield return new CompileTestData
		{
			ShouldFail = false,
			DisplayName = "[code] with labelled loop",
			Lines =
			[
				"[code]",
				":start",
				WaitCommand.Mnemonic,
				$"{JumpIfZeroCommand.Mnemonic} jump_flag, start",
				HaltCommand.Mnemonic,
				"",
				"[data] 0x20",
				"\"Text\"",
				"[data] 0x40",
				":jump_flag",
				"0"
			],
			ExpectedBytes =
			{
				[0] = WaitCommand.OpCode,
				//	Jump with direct/direct addressing, so adds 3 to the base opCode
				[1] = (byte)(JumpIfZeroCommand.OpCode + 3),
				//	:jump_flag label's address
				[2] = 0x40,
				//	:start label's address
				[3] = 0,
				[4] = HaltCommand.OpCode,
				[0x20] = (byte)'T',
				[0x21] = (byte)'e',
				[0x22] = (byte)'x',
				[0x23] = (byte)'t',
				[0x40] = 0 //	Should be zero already, but make sure!!
			}
		};
	}

	private static IEnumerable<CompileTestData> CompileBadProgramTestProvider()
	{
		//	Invalid program: no equivalent of DirectiveType.Program present
		yield return new CompileTestData
		{
			DisplayName = "No [code] directive",
			Lines = ["[data]"],
			ExpectedExceptionType = typeof(DirectiveException),
		};

		//	Invalid program: [data] segment would write past end of file
		yield return new CompileTestData
		{
			DisplayName = "[data] writes over 256 byte",
			Lines =
			[
				"[code]",
				"[data] 0xf0",
				"\"This text should cause an overwrite\""
			],
			ExpectedExceptionType = typeof(YodaByteCodeException),
		};

		//	Invalid program: [const] with parameter
		yield return new CompileTestData
		{
			DisplayName = "[const] parameter not permitted",
			Lines =
			[
				"[const] 0",
				"[code]"
			],
			ExpectedExceptionType = typeof(DirectiveException),
		};
	}

	internal static IEnumerable<object[]> CompileTestDataProvider()
	{
		foreach (var testData in CompileGoodProgramTestProvider())
			yield return [testData];
		foreach (var testData in InitialiseSymbolsTestProvider())
			yield return [testData];
		foreach (var testData in CompileBadProgramTestProvider())
			yield return [testData];
	}

	//	Define some directive tokens for repeated use
	private static readonly YodaDirectiveToken CodeDirective = YodaDirectiveToken.Create(1, 0, $"{DirectiveType.Code}");

	private static readonly YodaDirectiveToken DataDirective =
		YodaDirectiveToken.Create(100, 0, $"{DirectiveType.Data}");

	private static readonly YodaDirectiveToken ConstantsDirective =
		YodaDirectiveToken.Create(200, 0, $"{DirectiveType.Constants}");

	private static readonly YodaDirectiveToken UnknownDirective =
		YodaDirectiveToken.Create(1, 0, $"{DirectiveType.Unknown}");

	private static readonly IByteCodeGeneratorStrategy GeneratorStrategy = new YodaByteCodeGeneratorStrategy();

	internal static IEnumerable<object[]> CompileToByteCodeTestProvider()
	{
		yield return
		[
			new CompileToByteCodeTestData
			{
				ShouldFail = false,
				DisplayName = "No tokens",
				ByteCodeTokens = null
			}
		];

		foreach (var testData in CompileByteCodeHandleDirectiveChangeProvider())
			yield return [testData];
		foreach (var testData in CompileByteCodeGetDirectiveParameterValueProvider())
			yield return [testData];
		foreach (var testData in CompileByteCodeCodeProvider())
			yield return [testData];
	}

	private static IEnumerable<CompileToByteCodeTestData> CompileByteCodeHandleDirectiveChangeProvider()
	{
		//	No actual "code", but directive present
		yield return new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [code] directive with 0 params",
			ByteCodeTokens = [],
			Tokens =
			[
				CodeDirective,
			]
		};

		//	Code directive, explicitly set to zero, plus a wait command
		var codeWithParameter = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [code] directive with 1 param (0)",
			ByteCodeTokens = [],
		};
		codeWithParameter.Tokens.AddRange(codeWithParameter.Processor.Parse(["[code] 0", "wait"]));
		codeWithParameter.ByteCodeTokens.Add(YodaTokenByteCode.Code(CodeDirective.LineNumber + 1, 0,
			[codeWithParameter.Tokens[2]], GeneratorStrategy));
		yield return codeWithParameter;

		//	Code directive, explicitly set to 0x10, plus a wait command
		codeWithParameter = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [code] directive with 1 param (0x10)",
			ByteCodeTokens = [],
		};
		codeWithParameter.Tokens.AddRange(codeWithParameter.Processor.Parse(["[code] 0x10", "wait"]));
		codeWithParameter.ByteCodeTokens.Add(YodaTokenByteCode.Code(CodeDirective.LineNumber + 1, 0x10,
			[codeWithParameter.Tokens[2]], GeneratorStrategy));
		yield return codeWithParameter;

		//	Add an extra parameter to the directive - should cause a failure
		//	Cannot use the supplied parser as this will cause an exception earlier than required
		yield return new CompileToByteCodeTestData
		{
			DisplayName = "HandleDirectiveChange: [code] directive with 2 params",
			ExpectedExceptionType = typeof(DirectiveException),
			ByteCodeTokens = null,
			Tokens =
			[
				CodeDirective,
				YodaToken.LiteralNumber(CodeDirective.LineNumber, 1, "1"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber, 2, "1"),
			]
		};

		//	No actual "code", but [data] directive present
		//	Cannot use the supplied parser as this will cause an exception earlier than required
		yield return new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [data] directive with 0 params",
			ByteCodeTokens = [],
			Tokens =
			[
				DataDirective,
			]
		};

		//	Data directive, explicitly set to zero, plus a numeric value
		//	Cannot use the supplied parser as this will cause an exception earlier than required
		var dataWithParameter = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [data] directive with 1 param (0)",
			ByteCodeTokens = [],
			Tokens =
			[
				DataDirective,
				YodaToken.LiteralNumber(DataDirective.LineNumber, 1, "0"),
				YodaToken.LiteralNumber(DataDirective.LineNumber + 1, 0, "123")
			]
		};
		dataWithParameter.ByteCodeTokens.Add(YodaTokenByteCode.Data(DataDirective.LineNumber + 1, 0,
			[dataWithParameter.Tokens[2]], GeneratorStrategy));
		yield return dataWithParameter;

		//	Data directive, explicitly set to 0x80, plus a literal string
		//	Cannot use the supplied parser as this will cause an exception earlier than required
		dataWithParameter = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "HandleDirectiveChange: [data] directive with 1 param (0x80)",
			ByteCodeTokens = [],
			Tokens =
			[
				DataDirective,
				YodaToken.LiteralNumber(DataDirective.LineNumber, 1, "0x80"),
				YodaToken.LiteralString(DataDirective.LineNumber + 1, 0, "\"Text\"")
			]
		};
		dataWithParameter.ByteCodeTokens.Add(YodaTokenByteCode.Data(DataDirective.LineNumber + 1, 0x80,
			[dataWithParameter.Tokens[2]], GeneratorStrategy));
		yield return dataWithParameter;

		//	Add an extra parameter to the directive - should cause a failure
		//	Cannot use the supplied parser as this will cause an exception earlier than required
		yield return new CompileToByteCodeTestData
		{
			DisplayName = "HandleDirectiveChange: [data] directive with 2 params",
			ExpectedExceptionType = typeof(DirectiveException),
			ByteCodeTokens = null,
			Tokens =
			[
				DataDirective,
				YodaToken.LiteralNumber(DataDirective.LineNumber, 1, "1"),
				YodaToken.LiteralNumber(DataDirective.LineNumber, 2, "1"),
			]
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "HandleDirectiveChange: [const] directive with 1 param",
			ExpectedExceptionType = typeof(DirectiveException),
			ByteCodeTokens = null,
			Tokens =
			[
				ConstantsDirective,
				YodaToken.LiteralNumber(ConstantsDirective.LineNumber, 1, "1"),
				YodaToken.LiteralNumber(ConstantsDirective.LineNumber, 2, "1"),
			]
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "HandleDirectiveChange: [Unknown] directive",
			ExpectedExceptionType = typeof(ArgumentOutOfRangeException),
			ByteCodeTokens = null,
			Tokens = [UnknownDirective]
		};
	}

	private static IEnumerable<CompileToByteCodeTestData> CompileByteCodeGetDirectiveParameterValueProvider()
	{
		yield return new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "GetDirectiveParameterValue: [code] directive with 1 param (10)",
			ByteCodeTokens = [],
			Tokens =
			[
				CodeDirective,
				YodaToken.LiteralNumber(CodeDirective.LineNumber, 1, "10"),
			]
		};

		//	Pass a symbol as value for directive - should fail as symbol is not defined
		yield return new CompileToByteCodeTestData
		{
			ExpectedExceptionType = typeof(TokeniserException),
			DisplayName = "GetDirectiveParameterValue: [code] directive with symbol param",
			ByteCodeTokens = [],
			Tokens =
			[
				CodeDirective,
				YodaToken.Symbol(CodeDirective.LineNumber, 1, "start"),
			]
		};

		//	Pass a literal string as value for directive - should fail as not a valid parameter type
		yield return new CompileToByteCodeTestData
		{
			ExpectedExceptionType = typeof(DirectiveException),
			DisplayName = "GetDirectiveParameterValue: [code] directive with literal string param",
			ByteCodeTokens = [],
			Tokens =
			[
				CodeDirective,
				YodaToken.LiteralString(CodeDirective.LineNumber, 1, "\"start\""),
			]
		};

		//	Pass a literal char as value for directive - should fail as not a valid parameter type
		yield return new CompileToByteCodeTestData
		{
			ExpectedExceptionType = typeof(DirectiveException),
			DisplayName = "GetDirectiveParameterValue: [code] directive with literal char param",
			ByteCodeTokens = [],
			Tokens =
			[
				CodeDirective,
				YodaToken.LiteralChar(CodeDirective.LineNumber, 1, "'c'"),
			]
		};
	}

	private static IEnumerable<CompileToByteCodeTestData> CompileByteCodeCodeProvider()
	{
		yield return new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => command with missing required parameters",
			ExpectedExceptionType = typeof(TokeniserException),
			Tokens =
			[
				CodeDirective,
				YodaCommandToken.Create(CodeDirective.LineNumber + 1, 0, JumpIfZeroCommand.Mnemonic, JumpIfZeroCommand)
			],
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => command with 1 parameter (needs 3)",
			ExpectedExceptionType = typeof(TokeniserException),
			Tokens =
			[
				CodeDirective,
				YodaCommandToken.Create(CodeDirective.LineNumber + 1, 0, JumpIfZeroCommand.Mnemonic, JumpIfZeroCommand),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 1, "10")
			],
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => command with 1 extra parameter (needs 3)",
			ExpectedExceptionType = typeof(TokeniserException),
			Tokens =
			[
				CodeDirective,
				YodaCommandToken.Create(CodeDirective.LineNumber + 1, 0, JumpIfZeroCommand.Mnemonic, JumpIfZeroCommand),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 1, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 2, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 3, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 4, "10"),
			],
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => jz command with invalid parameter type",
			ExpectedExceptionType = typeof(TokeniserException),
			Tokens =
			[
				CodeDirective,
				YodaCommandToken.Create(CodeDirective.LineNumber + 1, 0, JumpIfZeroCommand.Mnemonic, JumpIfZeroCommand),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 1, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 2, "10"),
				YodaToken.LiteralChar(CodeDirective.LineNumber + 1, 3, "'a'"),
			],
		};

		yield return new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => [code] directive with invalid token type",
			ExpectedExceptionType = typeof(TokeniserException),
			Tokens =
			[
				CodeDirective,
				YodaCommandToken.Create(CodeDirective.LineNumber + 1, 0, JumpIfZeroCommand.Mnemonic, JumpIfZeroCommand),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 1, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 2, "10"),
				YodaToken.LiteralNumber(CodeDirective.LineNumber + 1, 3, "10"),
				YodaToken.LiteralString(CodeDirective.LineNumber + 2, 0, "\"text\""),
			],
		};

		//	Code directive, with a label
		var test = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "CompileToByteCode: Code checks => [code] directive with label",
			ByteCodeTokens = []
		};
		test.Tokens.AddRange(test.Processor.Parse(["[code]", ":start", "jz 10 10"]));
		test.Processor.InitialiseSymbols(test.Tokens);
		test.ByteCodeTokens.Add(YodaTokenByteCode.Code(CodeDirective.LineNumber + 2, 0,
			test.Tokens[2..], GeneratorStrategy));
		yield return test;

		//	Code directive, with label, terminated with invalid non-code token
		test = new CompileToByteCodeTestData
		{
			DisplayName = "CompileToByteCode: Code checks => [code] directive with label and invalid non-code token",
			ByteCodeTokens = [],
			ExpectedExceptionType = typeof(TokeniserException)
		};
		test.Tokens.AddRange(test.Processor.Parse(["[code]", ":start", "jz 10 10"]));
		test.Tokens.Add(YodaToken.LiteralChar(1000, 0, "'a'"));
		test.Processor.InitialiseSymbols(test.Tokens);
		test.ByteCodeTokens.Add(YodaTokenByteCode.Code(CodeDirective.LineNumber + 2, 0,
			test.Tokens[2..^1], GeneratorStrategy));
		yield return test;

		//	Code and data that would end up occupying the same memory locations
		//	N.B	-	this is not an error as yet. Compilation to bytecode segments doesn't check for overlaps automatically
		//			that check is performed later, when attempting to bring everything together
		test = new CompileToByteCodeTestData
		{
			ShouldFail = false,
			DisplayName = "CompileToByteCode: Code checks => [code] and [data] directive with same memory location",
			ByteCodeTokens = [],
		};
		test.Tokens.AddRange(test.Processor.Parse(["[code] 0x10", ":start", "jz 10 10", "[data] 0x10", "\"Text\""]));
		test.Processor.InitialiseSymbols(test.Tokens);
		test.ByteCodeTokens.Add(YodaTokenByteCode.Code(test.Tokens[3].LineNumber, 0x10,
			test.Tokens.GetRange(3, 3), GeneratorStrategy));
		test.ByteCodeTokens.Add(YodaTokenByteCode.Data(test.Tokens[^1].LineNumber, 0x10,
			[test.Tokens[^1]], GeneratorStrategy));
		yield return test;
	}

	internal static IEnumerable<object[]> CompilePass2TestProvider()
	{
		yield return
		[
			new SourceCodeDataTest
			{
				ShouldFail = false,
				DisplayName = "Code + data with labels",
				Lines =
				[
					"[code]",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x80",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
			}
		];

		yield return
		[
			new SourceCodeDataTest
			{
				ExpectedExceptionType = typeof(TokeniserException),
				DisplayName = "Code + data with 1 undefined label",
				Lines =
				[
					"[const]",
					"a=b",
					"c=2",
					"[code]",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x80",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
			}
		];

		yield return
		[
			new SourceCodeDataTest
			{
				ExpectedExceptionType = typeof(AggregateException),
				DisplayName = "Code + data with 2 undefined labels",
				Lines =
				[
					"[const]",
					"a=b",
					"c=2",
					"[code]",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x80",
					"\"test string\"",
					"0xC0, start_of_data, 0xC0"
				],
			}
		];
	}

	internal static IEnumerable<object[]> CompilePass3TestProvider()
	{
		yield return
		[
			new CompileTestData
			{
				ShouldFail = false,
				DisplayName = "Check chain resolves correctly",
				Lines =
				[
					"[const]",
					"a=b",
					"b=c",
					"c=0x80",
					"[code]",
					"wait",
					"jz a 0",
					"halt",
					"[data] c",
					"1"
				],
				ExpectedBytes =
				{
					[0] = WaitCommand.OpCode,
					[1] = (byte)(JumpIfZeroCommand.OpCode | 3),
					[2] = 0x80,
					[3] = 0,
					[4] = 0,
					[0x80] = 1
				}
			}
		];
		yield return
		[
			new CompileTestData
			{
				DisplayName = "Chain has recursive value",
				Lines =
				[
					"[const]",
					"a=b",
					"b=c",
					"c=a",
					"[code]",
					"wait",
					"jz a 0",
					"halt",
					"[data] 0x80",
					"1"
				],
				ExpectedExceptionType = typeof(YodaByteCodeException)
			}
		];
	}

	internal static IEnumerable<object[]> CompilePass4TestProvider()
	{
		yield return
		[
			new CompileTestData
			{
				ShouldFail = false,
				DisplayName = "Code + data with labels",
				Lines =
				[
					"[code]",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x80",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedBytes =
				{
					[0] = WaitCommand.OpCode,
					[1] = HaltCommand.OpCode,
					[0x80] = (byte)'t',
					[0x81] = (byte)'e',
					[0x82] = (byte)'s',
					[0x83] = (byte)'t',
					[0x84] = (byte)' ',
					[0x85] = (byte)'s',
					[0x86] = (byte)'t',
					[0x87] = (byte)'r',
					[0x88] = (byte)'i',
					[0x89] = (byte)'n',
					[0x8A] = (byte)'g',
					[0x8B] = 0xC0,
					[0x8C] = 0,
					[0x8D] = 0xC0,
				}
			}
		];

		yield return
		[
			new CompileTestData
			{
				ShouldFail = false,
				DisplayName = "[data] then [code]",
				Lines =
				[
					"[code] 0x10",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedBytes =
				{
					[0x0] = (byte)'t',
					[0x1] = (byte)'e',
					[0x2] = (byte)'s',
					[0x3] = (byte)'t',
					[0x4] = (byte)' ',
					[0x5] = (byte)'s',
					[0x6] = (byte)'t',
					[0x7] = (byte)'r',
					[0x8] = (byte)'i',
					[0x9] = (byte)'n',
					[0xA] = (byte)'g',
					[0xB] = 0xC0,
					[0xC] = 0x10,
					[0xD] = 0xC0,
					[0x10] = WaitCommand.OpCode,
					[0x11] = HaltCommand.OpCode,
				}
			}
		];

		yield return
		[
			new CompileTestData
			{
				DisplayName = "[code] and [data] at same location",
				Lines =
				[
					"[code] 0x10",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x10",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedExceptionType = typeof(YodaByteCodeException)
			}
		];

		yield return
		[
			new CompileTestData
			{
				DisplayName = "[code] and [data] overlap",
				Lines =
				[
					"[code] 0x18",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x10",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedExceptionType = typeof(YodaByteCodeException)
			}
		];

		yield return
		[
			new CompileTestData
			{
				DisplayName = "[code] outside bounds",
				Lines =
				[
					"[code] 0x100",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x18",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedExceptionType = typeof(YodaByteCodeException)
			}
		];

		yield return
		[
			new CompileTestData
			{
				DisplayName = "[data] outside bounds",
				Lines =
				[
					"[code]",
					":start_of_program",
					"wait",
					"halt",
					"[data] 0x100",
					"\"test string\"",
					"0xC0, start_of_program, 0xC0"
				],
				ExpectedExceptionType = typeof(YodaByteCodeException)
			}
		];
	}

	#endregion
}