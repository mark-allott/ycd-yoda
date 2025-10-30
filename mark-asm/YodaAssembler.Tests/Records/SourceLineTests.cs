using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Tests.Records;

[TestClass]
public class SourceLineTests
{
	[TestMethod]
	[DataRow(0, DisplayName = "Check line 0")]
	[DataRow(-1, DisplayName = "Check line -1")]
	[DataRow(int.MinValue, DisplayName = "Check line int.MinValue")]
	public void ValidateSourceLineThrowsExceptionForInvalidLineNumber(int lineNumber)
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SourceLine(lineNumber, ""));
	}

	[TestMethod]
	[DataRow(null, "", DisplayName = "null string")]
	[DataRow("", "", DisplayName = "string.Empty")]
	[DataRow("  ", "", DisplayName = "spaces")]
	[DataRow("\t", "", DisplayName = "tab")]
	[DataRow("  \t \t", "", DisplayName = "Mix of spaces and tab")]
	[DataRow("\t[directive]", "\t[directive]", DisplayName = "Check pseudo-directives")]
	[DataRow("\tcommand", "\tcommand", DisplayName = "Check pseudo-command")]
	[DataRow(";comment", ";comment", DisplayName = "Check comment")]
	[DataRow("command ;comment", "command ;comment", DisplayName = "Check inline comment")]
	public void ValidateSourceLine(string? text, string expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.Text.Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, true, DisplayName = "null string")]
	[DataRow("", true, DisplayName = "string.Empty")]
	[DataRow("  ", true, DisplayName = "spaces")]
	[DataRow("\t", true, DisplayName = "tab")]
	[DataRow("  \t \t", true, DisplayName = "Mix of spaces and tab")]
	[DataRow(" a ", false, DisplayName = "letter with spaces")]
	[DataRow("\tb", false, DisplayName = "tab with letter")]
	public void ValidateIsBlank(string? text, bool expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.IsBlank.Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, false, DisplayName = "null string")]
	[DataRow("", false, DisplayName = "string.Empty")]
	[DataRow(";", true, DisplayName = "empty comment")]
	[DataRow(" ;", true, DisplayName = "indented comment")]
	[DataRow("  \t \t", false, DisplayName = "Mix of spaces and tab")]
	[DataRow(" a ", false, DisplayName = "letter with spaces")]
	[DataRow("\tb", false, DisplayName = "tab with letter")]
	public void ValidateIsComment(string? text, bool expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.IsComment.Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, false, DisplayName = "null string")]
	[DataRow("", false, DisplayName = "string.Empty")]
	[DataRow(":", false, DisplayName = "label prefix only")]
	[DataRow("000:", false, DisplayName = "000:")]
	[DataRow("_00 :", false, DisplayName = "_00 :")]
	[DataRow("_00:", true, DisplayName = "_00:")]
	[DataRow("aLongSymbol:", true, DisplayName = "aLongSymbol:")]
	[DataRow("aVeryLongSymbolNameThatShouldNotWork:", false, DisplayName = "aVeryLongSymbolNameThatShouldNotWork:")]
	public void ValidateIsLabel(string? text, bool expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.IsLabel.Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, false, DisplayName = "null string")]
	[DataRow("", false, DisplayName = "string.Empty")]
	[DataRow(";", true, DisplayName = "empty comment")]
	[DataRow(" ;", true, DisplayName = "indented comment")]
	[DataRow("  \t \t", false, DisplayName = "Mix of spaces and tab")]
	[DataRow(" a ", false, DisplayName = "letter with spaces")]
	[DataRow("\tb", false, DisplayName = "tab with letter")]
	[DataRow(":aLongSymbol\t; with comment", true, DisplayName = ":aLongSymbol\t; with comment")]
	public void ValidateHasComment(string? text, bool expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.HasComment.Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, false, DisplayName = "null string")]
	[DataRow("", false, DisplayName = "string.Empty")]
	[DataRow(";", false, DisplayName = "empty comment")]
	[DataRow(" ;", false, DisplayName = "indented comment")]
	[DataRow("  \t \t", false, DisplayName = "Mix of spaces and tab")]
	[DataRow(" a ", false, DisplayName = "letter with spaces")]
	[DataRow("\tb", false, DisplayName = "tab with letter")]
	[DataRow(":aLongSymbol\t; with comment", false, DisplayName = ":aLongSymbol\t; with comment")]
	[DataRow("[aLongDirective]\t; with comment", true, DisplayName = "[aLongDirective]\t; with comment")]
	[DataRow("\t[aReallyLongDirective]\t; with comment", true, DisplayName = "\t[aReallyLongDirective]\t; with comment")]
	[DataRow("[aLongDirectiveThatShouldFail]\t; with comment", false, DisplayName = "[aLongDirectiveThatShouldFail]\t; with comment")]
	public void ValidateIsDirective(string? text, bool expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.IsDirective.Should().Be(expected);
	}

	/// <summary>
	/// Validates the expected directive from the source is masked into one of 4 possible outcomes
	/// </summary>
	/// <param name="text">The line of code to test</param>
	/// <param name="expected">The expected <see cref="DirectiveType"/> value</param>
	/// <remarks>
	/// Similar to the test for the token itself, but the result is then masked to the lower-nibble values, making
	/// checking for the directive types easier when tokenising, even if using one of the alternate names for the
	/// directive. Any text that does not map to a known <see cref="DirectiveType"/> value, including <c>null</c> and
	/// <c>string.Empty</c> values will always result in the value <see cref="DirectiveType.Unknown"/>
	/// </remarks>
	[TestMethod]
	[DataRow("", DirectiveType.Unknown, DisplayName = "test empty")]
	[DataRow("[]", DirectiveType.Unknown, DisplayName = "test empty brackets")]
	[DataRow("program", DirectiveType.Unknown, DisplayName = "test program without brackets")]
	[DataRow("[program]", DirectiveType.Program, DisplayName = "test [program]")]
	[DataRow("[data]", DirectiveType.Data, DisplayName = "test [data]")]
	[DataRow("[constants]", DirectiveType.Constants, DisplayName = "test [constants]")]
	[DataRow("[PROGRAM]", DirectiveType.Program, DisplayName = "test [PROGRAM]")]
	[DataRow("[DATA]", DirectiveType.Data, DisplayName = "test [DATA]")]
	[DataRow("[CONSTANTS]", DirectiveType.Constants, DisplayName = "test [CONSTANTS]")]
	[DataRow("[Program]", DirectiveType.Program, DisplayName = "test [Program]")]
	[DataRow("[Data]", DirectiveType.Data, DisplayName = "test [Data]")]
	[DataRow("[Constants]", DirectiveType.Constants, DisplayName = "test [Constants]")]
	[DataRow("[prog]", DirectiveType.Program, DisplayName = "test [prog]")]
	[DataRow("[const]", DirectiveType.Constants, DisplayName = "test [const]")]
	[DataRow("[code]", DirectiveType.Program, DisplayName = "test [code]")]
	[DataRow("[PROG]", DirectiveType.Program, DisplayName = "test [PROG]")]
	[DataRow("[CONST]", DirectiveType.Constants, DisplayName = "test [CONST]")]
	[DataRow("[CODE]", DirectiveType.Program, DisplayName = "test [CODE]")]
	[DataRow("[Prog]", DirectiveType.Program, DisplayName = "test [Prog]")]
	[DataRow("[Const]", DirectiveType.Constants, DisplayName = "test [Const]")]
	[DataRow("[Code]", DirectiveType.Program, DisplayName = "test [Code]")]
	[DataRow("[dummy]", DirectiveType.Unknown, DisplayName = "test [dummy]")]
	public void ValidateGetDirectiveType(string text, DirectiveType expected)
	{
		var sut = new SourceLine(1, text);
		sut.Should().NotBeNull();
		sut.Directive.Should().Be(expected);
	}
}