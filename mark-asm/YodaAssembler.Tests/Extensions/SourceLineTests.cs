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
		var sut = new SourceLine(1, text);
		sut.IsBlank.Should().Be(expected);
	}

	/// <summary>
	/// Checks to see if the directives can be detected correctly
	/// </summary>
	/// <param name="text">The source text to check</param>
	/// <param name="expected">The expected result after calling the <see cref="SourceLineExtensions.HasDirective"/> method</param>
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("[]", false, DisplayName = "test []")]
	[DataRow("[directive]", true, DisplayName = "test [directive]")]
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
		var sut = new SourceLine(1, text).HasDirective;
		sut.Should().Be(expected);
	}
}