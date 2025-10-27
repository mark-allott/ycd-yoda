using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Extensions;
using YodaAssembler.Records;

namespace YodaAssembler.Tests.Extensions;

[TestClass]
public sealed class SourceLineTests
{
	/// <summary>
	/// Checks the extension detects blank lines correctly
	/// </summary>
	/// <param name="text">The text of the whole line</param>
	/// <param name="expected">The expected result from the <see cref="YodaAssembler.Extensions.SourceLineExtensions.IsBlank"/> method</param>
	[TestMethod]
	[DataRow("", true, DisplayName = "test string.Empty")]
	[DataRow("\t", true, DisplayName = "test tab")]
	[DataRow(" ", true, DisplayName = "test space")]
	[DataRow(" a bc ", false, DisplayName = "test ' a bc '")]
	public void ValidateIsBlank(string text, bool expected)
	{
		var sut = new SourceLine() { LineNumber = 0, Text = text };
		sut.IsBlank.Should().Be(expected);
	}

	/// <summary>
	/// Checks the extension method correctly identifies a line of comment
	/// </summary>
	/// <param name="text">The text for the test</param>
	/// <param name="expected">The expected result from the <see cref="YodaAssembler.Extensions.SourceLineExtensions.IsComment"/> method</param>
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow(";", true, DisplayName = "test comment char only")]
	[DataRow(" ;", true, DisplayName = "test comment char only with space prefix")]
	[DataRow("\t;", true, DisplayName = "test comment char only with tab prefix")]
	[DataRow(";comment", true, DisplayName = "test comment")]
	[DataRow(" ;comment", true, DisplayName = "test comment with initial space")]
	[DataRow("\t;comment", true, DisplayName = "test comment with initial tab")]
	[DataRow("[directive] ;comment", false, DisplayName = "test inline comment with space")]
	[DataRow("[directive]\t;comment", false, DisplayName = "test inline comment with tab")]
	public void ValidateIsComment(string text, bool expected)
	{
		var sut = text.IsComment();
		sut.Should().Be(expected);
	}

	/// <summary>
	/// Checks the extension method correctly identifies a line with an inline comment in it
	/// </summary>
	/// <param name="text">The text for the test</param>
	/// <param name="expected">The expected result from the <see cref="YodaAssembler.Extensions.SourceLineExtensions.IsComment"/> method</param>
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("[directive]", false, DisplayName = "test directive")]
	[DataRow("load", false, DisplayName = "test pseudo command")]
	[DataRow(";", true, DisplayName = "test comment char only")]
	[DataRow(" ;", true, DisplayName = "test comment char only with space prefix")]
	[DataRow("\t;", true, DisplayName = "test comment char only with tab prefix")]
	[DataRow(";comment", true, DisplayName = "test comment")]
	[DataRow(" ;comment", true, DisplayName = "test comment with initial space")]
	[DataRow("\t;comment", true, DisplayName = "test comment with initial tab")]
	[DataRow("[directive] ;comment", true, DisplayName = "test inline comment with space")]
	[DataRow("[directive]\t;comment", true, DisplayName = "test inline comment with tab")]
	public void ValidateHasComment(string text, bool expected)
	{
		var sut = text.HasComment();
		sut.Should().Be(expected);
	}

	/// <summary>
	/// Checks to see if the directives can be detected correctly
	/// </summary>
	/// <param name="text">The source text to check</param>
	/// <param name="expected">The expected result after calling the <see cref="SourceLineExtensions.HasDirective"/> method</param>
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("[]", false, DisplayName = "test []")]
	[DataRow("[directive]", false, DisplayName = "test [directive]")]
	[DataRow("[prog]", true, DisplayName = "test prog")]
	[DataRow("[program]", true, DisplayName = "test program")]
	[DataRow("[const]", true, DisplayName = "test const")]
	[DataRow("[constants]", true, DisplayName = "test constants")]
	[DataRow("[data]", true, DisplayName = "test data")]
	[DataRow("[PROG]", true, DisplayName = "test PROG")]
	[DataRow("[PROGRAM]", true, DisplayName = "test PROGRAM")]
	[DataRow("[CONST]", true, DisplayName = "test CONST")]
	[DataRow("[CONSTANTS]", true, DisplayName = "test CONSTANTS")]
	[DataRow("[DATA]", true, DisplayName = "test DATA")]
	[DataRow("[Prog]", true, DisplayName = "test Prog")]
	[DataRow("[Program]", true, DisplayName = "test Program")]
	[DataRow("[Const]", true, DisplayName = "test Const")]
	[DataRow("[Constants]", true, DisplayName = "test Constants")]
	[DataRow("[Data]", true, DisplayName = "test Data")]
	public void ValidateHasDirective(string text, bool expected)
	{
		var sut = new SourceLine() { LineNumber = 0, Text = text }.HasDirective;
		sut.Should().Be(expected);
	}

	/// <summary>
	/// Checks to make sure the correct directive type is detected - even when using abbreviations
	/// </summary>
	/// <param name="text">The source text to test</param>
	/// <param name="expected">The expected <see cref="DirectiveType"/> after calling <see cref="YodaAssembler.Extensions.SourceLineExtensions.GetDirectiveType"/> method</param>
	[TestMethod]
	[DataRow("", DirectiveType.Unknown, DisplayName = "test empty")]
	[DataRow("[]", DirectiveType.Unknown, DisplayName = "test empty brackets")]
	[DataRow("program", DirectiveType.Unknown, DisplayName = "test program without brackets")]
	[DataRow("[prog]", DirectiveType.Program, DisplayName = "test [prog]")]
	[DataRow("[program]", DirectiveType.Program, DisplayName = "test [program]")]
	[DataRow("[const]", DirectiveType.Constants, DisplayName = "test [const]")]
	[DataRow("[constants]", DirectiveType.Constants, DisplayName = "test [constants]")]
	[DataRow("[data]", DirectiveType.Data, DisplayName = "test [data]")]
	[DataRow("[PROG]", DirectiveType.Program, DisplayName = "test [PROG]")]
	[DataRow("[PROGRAM]", DirectiveType.Program, DisplayName = "test [PROGRAM]")]
	[DataRow("[CONST]", DirectiveType.Constants, DisplayName = "test [CONST]")]
	[DataRow("[CONSTANTS]", DirectiveType.Constants, DisplayName = "test [CONSTANTS]")]
	[DataRow("[DATA]", DirectiveType.Data, DisplayName = "test [DATA]")]
	[DataRow("[Prog]", DirectiveType.Program, DisplayName = "test [Prog]")]
	[DataRow("[Program]", DirectiveType.Program, DisplayName = "test [Program]")]
	[DataRow("[Const]", DirectiveType.Constants, DisplayName = "test [Const]")]
	[DataRow("[Constants]", DirectiveType.Constants, DisplayName = "test [Constants]")]
	[DataRow("[Data]", DirectiveType.Data, DisplayName = "test [Data]")]
	public void ValidateGetDirectiveType(string text, DirectiveType expected)
	{
		var sut = text.GetDirectiveType();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("dummy", false, DisplayName = "test 'dummy'")]
	[DataRow(":", false, DisplayName = "test colon with no symbols")]
	[DataRow("a:", false, DisplayName = "test colon with a prefix")]
	[DataRow(":a", true,  DisplayName = "test ':a'")]
	[DataRow(": a", false,  DisplayName = "test ': a'")]
	[DataRow(" :a", true,  DisplayName = "test ' :a'")]
	[DataRow("\t:a", true,  DisplayName = "test '\t:a'")]
	[DataRow(":a ", true,  DisplayName = "test ':a '")]
	[DataRow(" :a ", true,  DisplayName = "test ' :a '")]
	[DataRow("\t:a ", true,  DisplayName = "test '\t:a '")]
	[DataRow("\t:a\t", true,  DisplayName = "test '\t:a\t'")]
	[DataRow(":a ;comment", true,  DisplayName = "test ':a ;comment'")]
	[DataRow(" :a ;comment", true,  DisplayName = "test ' :a ;comment'")]
	[DataRow("\t:a ;comment", true,  DisplayName = "test '\t:a ;comment'")]
	[DataRow("\t:a\t;comment", true,  DisplayName = "test '\t:a\t;comment'")]
	public void ValidateHasLabel(string text, bool expected)
	{
		var sut = text.HasLabel();
		sut.Should().Be(expected);
	}
}