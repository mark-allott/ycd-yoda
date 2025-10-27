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

	/*
	 * Checks for the IsComment and HasComment extension methods are already performed in the TokenTypeExtensionsTests
	 * as the equivalent in SourceLineExtensions is simply a wrapper around the existing extension methods
	 */
	
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
	[DataRow("[code]", true, DisplayName = "test code")]
	[DataRow("[Code]", true, DisplayName = "test Code")]
	[DataRow("[CODE]", true, DisplayName = "test CODE")]
	public void ValidateHasDirective(string text, bool expected)
	{
		var sut = new SourceLine(1, text).HasDirective;
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("", DirectiveType.Unknown, DisplayName = "test empty")]
	[DataRow("[]", DirectiveType.Unknown, DisplayName = "test []")]
	[DataRow("[directive]", DirectiveType.Unknown, DisplayName = "test [directive]")]
	[DataRow("[program]", DirectiveType.Program, DisplayName = "test [program]")]
	[DataRow("[PROGRAM]", DirectiveType.Program, DisplayName = "test [PROGRAM]")]
	[DataRow("[Program]", DirectiveType.Program, DisplayName = "test [Program]")]
	[DataRow("[data]", DirectiveType.Data, DisplayName = "test [data]")]
	[DataRow("[DATA]", DirectiveType.Data, DisplayName = "test [DATA]")]
	[DataRow("[Data]", DirectiveType.Data, DisplayName = "test [Data]")]
	[DataRow("[constants]", DirectiveType.Constants, DisplayName = "test [constants]")]
	[DataRow("[CONSTANTS]", DirectiveType.Constants, DisplayName = "test [CONSTANTS]")]
	[DataRow("[Constants]", DirectiveType.Constants, DisplayName = "test [Constants]")]
	[DataRow("[prog]", DirectiveType.Program, DisplayName = "test [prog]")]
	[DataRow("[PROG]", DirectiveType.Program, DisplayName = "test [PROG]")]
	[DataRow("[Prog]", DirectiveType.Program, DisplayName = "test [Prog]")]
	[DataRow("[const]", DirectiveType.Constants, DisplayName = "test [const]")]
	[DataRow("[CONST]", DirectiveType.Constants, DisplayName = "test [CONST]")]
	[DataRow("[Const]", DirectiveType.Constants, DisplayName = "test [Const]")]
	[DataRow("[code]", DirectiveType.Program, DisplayName = "test [code]")]
	[DataRow("[CODE]", DirectiveType.Program, DisplayName = "test [CODE]")]
	[DataRow("[Code]", DirectiveType.Program, DisplayName = "test [Code]")]
	public void ValidateGetDirectiveType(string  text, DirectiveType expected)
	{
		var sut = new SourceLine(1, text);
		sut.GetDirectiveType().Should().Be(expected);
	}
}