using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Processor;
using YodaAssembler.Records;
using YodaAssembler.Strategies;

namespace YodaAssembler.Tests.Strategies;

[TestClass]
public class YodaTokeniserStategyTests
{
	private static readonly List<YodaCommand> TestCommands =
	[
		new YodaCommand(0x00, "halt"),
		new YodaCommand(0x01, "wait"),
		new YodaCommand(0x02, "return"),
		new YodaCommand(0x02, "ret"),
		new YodaCommand(0x03, "noop"),
		new YodaCommand(0x03, "nop"),
		new YodaCommand(0x20, "lff", 2, YodaCommandSet.ImmediateOrDirect, YodaCommandSet.ImmediateOrDirect),
	];

	private static YodaTokeniserStrategy _testStrategy = null!;
	private static YodaTokeniserStrategy _yodaStrategy = null!;

	[TestInitialize]
	public void TestInitialize()
	{
		_testStrategy ??= new YodaTokeniserStrategy(TestCommands);
		_yodaStrategy ??= new YodaTokeniserStrategy(YodaCommandSet.Commands);
	}

	[TestMethod]
	[DataRow(";comment|[code]", new[] { TokenType.Comment, TokenType.Directive }, new[] { 1, 2 }, new[] { 0, 0 },
		DisplayName = "full-line comment")]
	[DataRow(";comment;2nd comment|[code]", new[] { TokenType.Comment, TokenType.Directive }, new[] { 1, 2 },
		new[] { 0, 0 }, DisplayName = "full-line comment with 2 semicolons")]
	[DataRow(" |[code]", new[] { TokenType.Blank, TokenType.Directive }, new[] { 1, 2 }, new[] { 0, 0 },
		DisplayName = "blank line")]
	[DataRow("[code];comment", new[] { TokenType.Directive, TokenType.Comment }, new[] { 1, 1 }, new[] { 0, 1 },
		DisplayName = "inline comment")]
	[DataRow("[code] 0 ;comment", new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 1, 1 }, new[] { 0, 1, 2 }, DisplayName = "[code] 0 ; comment")]
	[DataRow("[code] 0x0 ;comment", new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 1, 1 }, new[] { 0, 1, 2 }, DisplayName = "[code] 0x0 ; comment")]
	[DataRow("[code] 0b0 ;comment", new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 1, 1 }, new[] { 0, 1, 2 }, DisplayName = "[code] 0b0 ; comment")]
	[DataRow("[prog] 0b0 ;comment", new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 1, 1 }, new[] { 0, 1, 2 }, DisplayName = "[prog] 0b0 ; comment")]
	[DataRow("[program] 0b0 ;comment", new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 1, 1 }, new[] { 0, 1, 2 }, DisplayName = "[program] 0b0 ; comment")]
	[DataRow("[program]|||[data] 0b0||;comment",
		new[] { TokenType.Directive, TokenType.Blank, TokenType.Blank, TokenType.Directive, TokenType.LiteralNumber, TokenType.Blank, TokenType.Comment },
		new[] { 1, 2, 3, 4, 4, 5, 6 }, new[] { 0, 0, 0, 0, 1, 0, 0 }, DisplayName = "[data] 0b0||; comment")]
	[DataRow("[code] 0 ;comment||:start",
		new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment, TokenType.Blank, TokenType.Label },
		new[] { 1, 1, 1, 2, 3 }, new[] { 0, 1, 2, 0, 0 }, DisplayName = "[code] 0 ;comment||:start")]
	[DataRow("[code] 0 ;comment||:start\t;\tanother comment",
		new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment, TokenType.Blank, TokenType.Label, TokenType.Comment },
		new[] { 1, 1, 1, 2, 3, 3 }, new[] { 0, 1, 2, 0, 0, 1 }, DisplayName = "[code] 0 ;comment||:start")]
	[DataRow("[code] 0 ;comment|halt",
		new[] { TokenType.Directive, TokenType.LiteralNumber, TokenType.Comment, TokenType.Command },
		new[] { 1, 1, 1, 2 }, new[] { 0, 1, 2, 0 }, DisplayName = "[code] 0 ; comment|halt")]
	[DataRow("[code]|lff 0,0",
		new[] { TokenType.Directive, TokenType.Command, TokenType.LiteralNumber, TokenType.LiteralNumber },
		new[] { 1, 2, 2, 2 }, new[] { 0, 0, 1, 2 }, DisplayName = "[code]|lff 0,0")]
	[DataRow("[code]|lff 0,1\t; comment",
		new[] { TokenType.Directive, TokenType.Command, TokenType.LiteralNumber, TokenType.LiteralNumber, TokenType.Comment },
		new[] { 1, 2, 2, 2, 2 }, new[] { 0, 0, 1, 2, 3 }, DisplayName = "[code]|lff 0,1\t; comment")]
	[DataRow("[code]|[data] 0x80|\t1 '2' \"3\" symbol",
		new[] { TokenType.Directive, TokenType.Directive, TokenType.LiteralNumber, TokenType.LiteralNumber, TokenType.LiteralChar, TokenType.LiteralString, TokenType.Symbol },
		new[] { 1, 2, 2, 3, 3, 3, 3 }, new[] { 0, 0, 1, 0, 1, 2, 3 }, DisplayName = "[code]|[data] 0x80|\t1 '2' \"3\" symbol")]
	[DataRow("[code]|[const]\t; declare some constants|a=0|b=0xf|c=0x3b|d=0b0|e=0b10101010|f=0b0110_0110|g='x'|h=123\t; plus comment|i=j",
		new[]
		{ 
			TokenType.Directive, 
			TokenType.Directive, TokenType.Comment, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralNumber, 
			TokenType.Symbol, TokenType.LiteralChar, 
			TokenType.Symbol, TokenType.LiteralNumber, TokenType.Comment,
			TokenType.Symbol, TokenType.Symbol
		},
		new[] { 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 10, 11, 11 }, 
		new[] { 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 0, 1 }, DisplayName = "[code]|[const]...")]
	public void ValidateParsedTokens(string sourceText, TokenType[] expectedTokens, int[] expectedLineNumbers,
		int[] expectedLineSequences)
	{
		//	Make sure lengths correspond
		expectedTokens.Length.Should().Be(expectedLineNumbers.Length);
		expectedTokens.Length.Should().Be(expectedLineSequences.Length);

		var source = sourceText.Split('|');
		var sut = _testStrategy.Tokenise(source)
			.ToList();
		sut.Should()
			.NotBeNull()
			.And.NotBeEmpty()
			.And.HaveCount(expectedTokens.Length);

		var i = -1;
		var items = expectedTokens
			.Select(tt => new
			{
				ExpectedTokenType = tt,
				Token = sut[++i],
				LineNumber = expectedLineNumbers[i],
				LineSequence = expectedLineSequences[i]
			})
			.ToList();
		items.ForEach(element =>
		{
			element.Token.TokenType.Should().Be(element.ExpectedTokenType);
			element.Token.LineNumber.Should().Be(element.LineNumber);
			element.Token.LineSequence.Should().Be(element.LineSequence);
		});
	}

	[TestMethod]
	[DataRow("[data]", DisplayName = "No [code] directives")]
	[DataRow("[code]|[junk]", DisplayName = "[junk] is not a valid directive")]
	[DataRow("[code|]", DisplayName = "Directive markers must be on same line")]
	[DataRow("code", DisplayName = "'code' is not a valid directive")]
	public void InvalidSourceCodeShouldThrowDirectiveExceptions(string sourceText)
	{
		var source = sourceText.Split('|');
		Assert.ThrowsException<DirectiveException>(() => _testStrategy.Tokenise(source));
	}

	[TestMethod]
	[DataRow("[code]|stop", DisplayName = "stop is not a command")]
	[DataRow(":label|[code]", DisplayName = "label before any directives")]
	[DataRow("[code]||[const]|:label", DisplayName = "label in wrong directive")]
	[DataRow("[code]|lff 0,0,0", DisplayName = "[code]|lff 0,0,0 - too many params")]
	[DataRow("[code]|lff 0", DisplayName = "[code]|lff 0 - too few params")]
	[DataRow("[code]|lff 0 \"0\"", DisplayName = "[code]|lff 0 \"0\" - wrong param type")]
	[DataRow("[code]|lff 0 [[0]]", DisplayName = "[code]|lff 0 [[0]] - wrong param type")]
	[DataRow("[code]|[const]|lff 0 0", DisplayName = "[code]|[const]|lff 0 0 - const declared wrong")]
	[DataRow("[code]|[const]|lff a 0", DisplayName = "[code]|[const]|lff a 0 - const declared wrong")]
	[DataRow("[code]|[const]|lff a=0 0", DisplayName = "[code]|[const]|lff a=0 0 - const declared wrong")]
	public void InvalidCommandsShouldThrowTokeniserExceptions(string sourceText)
	{
		var source = sourceText.Split('|');
		Assert.ThrowsException<TokeniserException>(() => _testStrategy.Tokenise(source));
	}
}