using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Extensions;

namespace YodaAssembler.Tests.Extensions;

[TestClass]
public class TokenTypeExtensionsTests
{
	#region Comment handling

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
	/// Checks the extension method correctly identifies a line with an inline comment in it
	/// </summary>
	/// <param name="text">The text for the test</param>
	/// <param name="expected">The expected result from the <see cref="YodaAssembler.Extensions.SourceLineExtensions.IsComment"/> method</param>
	[TestMethod]
	[DataRow(";", "", DisplayName = "test comment char only")]
	[DataRow(" ;", "", DisplayName = "test comment char only with space prefix")]
	[DataRow("\t;", "", DisplayName = "test comment char only with tab prefix")]
	[DataRow(";comment", "comment", DisplayName = "test comment")]
	[DataRow(" ;comment", "comment", DisplayName = "test comment with initial space")]
	[DataRow("\t;comment", "comment", DisplayName = "test comment with initial tab")]
	[DataRow("[directive] ;comment", "comment", DisplayName = "test inline comment with space")]
	[DataRow("[directive]\t;comment", "comment", DisplayName = "test inline comment with tab")]
	[DataRow(";multi-word comment", "multi-word comment", DisplayName = "test multi-word comment")]
	[DataRow(" ;multi-word comment", "multi-word comment", DisplayName = "test multi-word comment with initial space")]
	[DataRow("\t;multi-word comment", "multi-word comment", DisplayName = "test multi-word comment with initial tab")]
	[DataRow("[directive] ;multi-word comment", "multi-word comment",
		DisplayName = "test inline multi-word comment with space")]
	[DataRow("[directive]\t;multi-word comment", "multi-word comment",
		DisplayName = "test inline multi-word comment with tab")]
	[DataRow("; comment", "comment", DisplayName = "test comment with space between semi-colon and comments")]
	[DataRow(" ; comment", "comment", DisplayName = "test comment with space before and after semi-colon")]
	[DataRow(" ;\tcomment", "comment", DisplayName = "test comment with space before semi-colon and tab after")]
	[DataRow(";\tcomment", "comment", DisplayName = "test comment with tab after semi-colon")]
	[DataRow("\t; comment", "comment", DisplayName = "test comment with tab before and space after semi-colon")]
	[DataRow("\t;\tcomment", "comment", DisplayName = "test comment with tabs before and after semi-colon")]
	public void ValidateGetComment(string text, string expected)
	{
		var sut = text.GetComment();
		sut.Should().Be(expected);
	}

	#endregion

	#region Directive handling

	[TestMethod]
	[DataRow("[program]", true, DisplayName = "test [program]")]
	[DataRow(null, false, DisplayName = "test null")]
	[DataRow("", false, DisplayName = "test ''")]
	[DataRow("[]", false, DisplayName = "test []")]
	[DataRow("[a]", true, DisplayName = "test [a]")]
	[DataRow("[a+b]", false, DisplayName = "test [a+b]")]
	[DataRow("[a b]", false, DisplayName = "test [a b]")]
	[DataRow("[a] param", true, DisplayName = "test [a] param")]
	[DataRow("[a];comment", true, DisplayName = "test [a];comment")]
	[DataRow("[a] ;comment", true, DisplayName = "test [a] ;comment")]
	[DataRow("[a]\t;comment", true, DisplayName = "test [a]\t;comment")]
	[DataRow("[a] param;comment", true, DisplayName = "test [a] param;comment")]
	[DataRow("[a] param ;comment", true, DisplayName = "test [a] param ;comment")]
	[DataRow("[a] param ; comment", true, DisplayName = "test [a] param ; comment")]
	[DataRow("[a] param\t;comment", true, DisplayName = "test [a] param\t;comment")]
	[DataRow("[a] param\t;\tcomment", true, DisplayName = "test [a] param\t;\tcomment")]
	public void ValidateHasDirective(string text, bool expected)
	{
		var sut = text.HasDirective();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("[program]", true, DisplayName = "test [program]")]
	[DataRow("[data]", true, DisplayName = "test [data]")]
	[DataRow("[constants]", true, DisplayName = "test [constants]")]
	[DataRow(" [program]", true, DisplayName = "test ' [program]'")]
	[DataRow("\t[data]", true, DisplayName = "test \t[data]")]
	[DataRow(" [program] ", true, DisplayName = "test ' [program] '")]
	[DataRow("\t[data] ", true, DisplayName = "test '\t[data] '")]
	[DataRow(" [program] 0", true, DisplayName = "test ' [program] 0'")]
	[DataRow("\t[data] 0x80", true, DisplayName = "test '\t[data] 0x80'")]
	[DataRow(" [program] 0 ", true, DisplayName = "test ' [program] 0 '")]
	[DataRow("\t[data] 0x80 ", true, DisplayName = "test '\t[data] 0x80 '")]
	[DataRow(" [program] 0 ;comment", true, DisplayName = "test ' [program] 0 ;comment'")]
	[DataRow("\t[data] 0x80 ; comment", true, DisplayName = "test '\t[data] 0x80 ; comment'")]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("[]", false, DisplayName = "test empty brackets")]
	[DataRow("program", false, DisplayName = "test program without brackets")]
	[DataRow("[program", false, DisplayName = "test [program")]
	[DataRow("data]", false, DisplayName = "test data]")]
	[DataRow(" [program] 0 fail", false, DisplayName = "test ' [program] 0 fail'")]
	[DataRow("\t[data] 0x80 fail", false, DisplayName = "test '\t[data] 0x80 fail'")]
	public void ValidateIsDirective(string text, bool expected)
	{
		var sut = text.IsDirective<DirectiveType>();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("[program]", "program", DisplayName = "test [program]")]
	[DataRow("[Program]", "Program", DisplayName = "test [Program]")]
	[DataRow("[PROGRAM]", "PROGRAM", DisplayName = "test [PROGRAM]")]
	[DataRow("[data]", "data", DisplayName = "test [data]")]
	[DataRow("[Data]", "Data", DisplayName = "test [Data]")]
	[DataRow("[DATA]", "DATA", DisplayName = "test [DATA]")]
	[DataRow("[constants]", "constants", DisplayName = "test [constants]")]
	[DataRow("[Constants]", "Constants", DisplayName = "test [Constants]")]
	[DataRow("[CONSTANTS]", "CONSTANTS", DisplayName = "test [CONSTANTS]")]
	[DataRow("[prog]", "prog", DisplayName = "test [prog]")]
	[DataRow("[Prog]", "Prog", DisplayName = "test [Prog]")]
	[DataRow("[PROG]", "PROG", DisplayName = "test [PROG]")]
	[DataRow("[const]", "const", DisplayName = "test [const]")]
	[DataRow("[Const]", "Const", DisplayName = "test [Const]")]
	[DataRow("[CONST]", "CONST", DisplayName = "test [CONST]")]
	[DataRow("[code]", "code", DisplayName = "test [code]")]
	[DataRow("[Code]", "Code", DisplayName = "test [Code]")]
	[DataRow("[CODE]", "CODE", DisplayName = "test [CODE]")]
	public void ValidateGetDirectiveName(string text, string expected)
	{
		var sut = text.GetDirectiveName<DirectiveType>();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("[program]", "program", null, null, DisplayName = "test [program]")]
	[DataRow("[program]\t;comment", "program", null, "comment", DisplayName = "test [program]\t;comment")]
	[DataRow("[Program] 0", "Program", "0", null, DisplayName = "test [Program] 0")]
	[DataRow("[PROGRAM] 0 ;comment", "PROGRAM", "0", "comment", DisplayName = "test [PROGRAM] 0 ;comment")]
	[DataRow("[data]", "data", null, null, DisplayName = "test [data]")]
	[DataRow("[data] 0x80", "data", "0x80", null, DisplayName = "test [data] 0x80")]
	[DataRow("[Data] ;\tcomment", "Data", null, "comment", DisplayName = "test [Data] ;\tcomment")]
	[DataRow("[DATA] 0xC0\t;\tcomment", "DATA", "0xC0", "comment", DisplayName = "test [DATA] 0xC0\t;\tcomment")]
	[DataRow("[constants]", "constants", null, null, DisplayName = "test [constants]")]
	[DataRow("[Constants] abc", "Constants", "abc", null, DisplayName = "test [Constants] abc")]
	[DataRow("[CONSTANTS] def\t;\tcomment", "CONSTANTS", "def", "comment",
		DisplayName = "test [CONSTANTS] def\t;\tcomment")]
	[DataRow("[prog]", "prog", null, null, DisplayName = "test [prog]")]
	[DataRow("[Prog] 0b0000", "Prog", "0b0000", null, DisplayName = "test [Prog] 0b0000")]
	[DataRow("[PROG] ;another comment", "PROG", null, "another comment", DisplayName = "test [PROG] ;another comment")]
	[DataRow("[const]", "const", null, null, DisplayName = "test [const]")]
	[DataRow("[Const] xyz ; comment", "Const", "xyz", "comment", DisplayName = "test [Const] xyz ; comment")]
	[DataRow("[CONST] XYZ\t; comment", "CONST", "XYZ", "comment", DisplayName = "test [CONST] XYZ\t; comment")]
	[DataRow("[code]", "code", null, null, DisplayName = "test [code]")]
	[DataRow("[Code]\tdef", "Code", "def", null, DisplayName = "test [Code]\tdef")]
	[DataRow("[CODE] def ;\tcomment", "CODE", "def", "comment", DisplayName = "test [CODE] def ;\tcomment")]
	public void ValidateGetDirectiveParts(string text, string expectedDirective, string expectedParam,
		string expectedComment)
	{
		var (name, parameter, comment) = text.GetDirectiveParts<DirectiveType>();
		name.Should().Be(expectedDirective);
		parameter.Should().Be(expectedParam);
		comment.Should().Be(expectedComment);
	}

	/// <summary>
	/// Checks to make sure the correct directive type is detected
	/// </summary>
	/// <param name="text">The source text to test</param>
	/// <param name="expected">The expected <see cref="DirectiveType"/> after calling <see cref="YodaAssembler.Extensions.TokenTypeExtensions.GetDirective"/> method</param>
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
	[DataRow("[prog]", DirectiveType.Prog, DisplayName = "test [prog]")]
	[DataRow("[const]", DirectiveType.Const, DisplayName = "test [const]")]
	[DataRow("[code]", DirectiveType.Code, DisplayName = "test [code]")]
	[DataRow("[PROG]", DirectiveType.Prog, DisplayName = "test [PROG]")]
	[DataRow("[CONST]", DirectiveType.Const, DisplayName = "test [CONST]")]
	[DataRow("[CODE]", DirectiveType.Code, DisplayName = "test [CODE]")]
	[DataRow("[Prog]", DirectiveType.Prog, DisplayName = "test [Prog]")]
	[DataRow("[Const]", DirectiveType.Const, DisplayName = "test [Const]")]
	[DataRow("[Code]", DirectiveType.Code, DisplayName = "test [Code]")]
	[DataRow("[dummy]", DirectiveType.Unknown, DisplayName = "test [dummy]")]
	public void ValidateGetDirectiveType(string text, DirectiveType expected)
	{
		var sut = text.GetDirective<DirectiveType>();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("[program]", DirectiveType.Program, null, null, DisplayName = "test [program]")]
	[DataRow("[program]\t;comment", DirectiveType.Program, null, "comment", DisplayName = "test [program]\t;comment")]
	[DataRow("[Program] 0", DirectiveType.Program, "0", null, DisplayName = "test [Program] 0")]
	[DataRow("[PROGRAM] 0 ;comment", DirectiveType.Program, "0", "comment", DisplayName = "test [PROGRAM] 0 ;comment")]
	[DataRow("[data]", DirectiveType.Data, null, null, DisplayName = "test [data]")]
	[DataRow("[data] 0x80", DirectiveType.Data, "0x80", null, DisplayName = "test [data] 0x80")]
	[DataRow("[Data] ;\tcomment", DirectiveType.Data, null, "comment", DisplayName = "test [Data] ;\tcomment")]
	[DataRow("[DATA] 0xC0\t;\tcomment", DirectiveType.Data, "0xC0", "comment", DisplayName = "test [DATA] 0xC0\t;\tcomment")]
	[DataRow("[constants]", DirectiveType.Constants, null, null, DisplayName = "test [constants]")]
	[DataRow("[Constants] abc", DirectiveType.Constants, "abc", null, DisplayName = "test [Constants] abc")]
	[DataRow("[CONSTANTS] def\t;\tcomment", DirectiveType.Constants, "def", "comment", DisplayName = "test [CONSTANTS] def\t;\tcomment")]
	[DataRow("[prog]", DirectiveType.Prog, null, null, DisplayName = "test [prog]")]
	[DataRow("[Prog] 0b0000", DirectiveType.Prog, "0b0000", null, DisplayName = "test [Prog] 0b0000")]
	[DataRow("[PROG] ;another comment", DirectiveType.Prog, null, "another comment", DisplayName = "test [PROG] ;another comment")]
	[DataRow("[const]", DirectiveType.Const, null, null, DisplayName = "test [const]")]
	[DataRow("[Const] xyz ; comment", DirectiveType.Const, "xyz", "comment", DisplayName = "test [Const] xyz ; comment")]
	[DataRow("[CONST] XYZ\t; comment", DirectiveType.Const, "XYZ", "comment", DisplayName = "test [CONST] XYZ\t; comment")]
	[DataRow("[code]", DirectiveType.Code, null, null, DisplayName = "test [code]")]
	[DataRow("[Code]\tdef", DirectiveType.Code, "def", null, DisplayName = "test [Code]\tdef")]
	[DataRow("[CODE] def ;\tcomment", DirectiveType.Code, "def", "comment", DisplayName = "test [CODE] def ;\tcomment")]
	[DataRow("[dummy]", DirectiveType.Unknown, null, null, DisplayName = "test [dummy]")]
	[DataRow("[dummy]\t;comment", DirectiveType.Unknown, null, null, DisplayName = "test [dummy]\t;comment")]
	[DataRow("[dummy] 0", DirectiveType.Unknown, null, null, DisplayName = "test [dummy] 0")]
	public void ValidateGetDirectiveDetail(string text, DirectiveType expectedDirective, string expectedParam, string expectedComment)
	{
		var (directive, parameter, comment) = text.GetDirectiveDetail<DirectiveType>();
		directive.Should().Be(expectedDirective);
		parameter.Should().Be(expectedParam);
		comment.Should().Be(expectedComment);
	}

	#endregion

	#region Label Handling

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("dummy", false, DisplayName = "test 'dummy'")]
	[DataRow(":", false, DisplayName = "test colon with no symbols")]
	[DataRow("a:", false, DisplayName = "test colon with a prefix")]
	[DataRow(":0", false, DisplayName = "test ':0'")]
	[DataRow(": a", false, DisplayName = "test ': a'")]
	[DataRow(":_theShortestLabelThatWillFailTest", false, DisplayName = "test ':_theShortestLabelThatWillFailTest'")]
	[DataRow(":__", true, DisplayName = "test ':__'")]
	[DataRow(":_a_", true, DisplayName = "test ':_a_'")]
	[DataRow(":a", true, DisplayName = "test ':a'")]
	[DataRow(":z", true, DisplayName = "test ':z'")]
	[DataRow(":A", true, DisplayName = "test ':A'")]
	[DataRow(":Z", true, DisplayName = "test ':Z'")]
	[DataRow(":_", true, DisplayName = "test ':_'")]
	[DataRow(" :a", true, DisplayName = "test ' :a'")]
	[DataRow("\t:a", true, DisplayName = "test '\t:a'")]
	[DataRow(":a ", true, DisplayName = "test ':a '")]
	[DataRow(" :a ", true, DisplayName = "test ' :a '")]
	[DataRow("\t:a ", true, DisplayName = "test '\t:a '")]
	[DataRow("\t:a\t", true, DisplayName = "test '\t:a\t'")]
	[DataRow(":a ;comment", true, DisplayName = "test ':a ;comment'")]
	[DataRow(" :a ;comment", true, DisplayName = "test ' :a ;comment'")]
	[DataRow("\t:a ;comment", true, DisplayName = "test '\t:a ;comment'")]
	[DataRow("\t:a\t;comment", true, DisplayName = "test '\t:a\t;comment'")]
	[DataRow(":_aLongLabel", true, DisplayName = "test ':_aLongLabel'")]
	[DataRow(":_aLongerLabel", true, DisplayName = "test ':_aLongerLabel'")]
	[DataRow(":_anEvenLongerLabel", true, DisplayName = "test ':_anEvenLongerLabel'")]
	[DataRow(":_theLongestLabelThatIsPossible00", true, DisplayName = "test ':_theLongestLabelThatIsPossible00'")]
	public void ValidateHasLabel(string text, bool expected)
	{
		var sut = text.HasLabel();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("", "", DisplayName = "test empty")]
	[DataRow("dummy", "", DisplayName = "test 'dummy'")]
	[DataRow(":", "", DisplayName = "test colon with no symbols")]
	[DataRow("a:", "", DisplayName = "test colon with a prefix")]
	[DataRow(":0", "", DisplayName = "test ':0'")]
	[DataRow(": a", "", DisplayName = "test ': a'")]
	[DataRow(":_theShortestLabelThatWillFailTest", "", DisplayName = "test ':_theShortestLabelThatWillFailTest'")]
	[DataRow(":__", "__", DisplayName = "test ':__'")]
	[DataRow(":_a_", "_a_", DisplayName = "test ':_a_'")]
	[DataRow(":a", "a", DisplayName = "test ':a'")]
	[DataRow(":z", "z", DisplayName = "test ':z'")]
	[DataRow(":A", "A", DisplayName = "test ':A'")]
	[DataRow(":Z", "Z", DisplayName = "test ':Z'")]
	[DataRow(":_", "_", DisplayName = "test ':_'")]
	[DataRow(" :a", "a", DisplayName = "test ' :a'")]
	[DataRow("\t:a", "a", DisplayName = "test '\t:a'")]
	[DataRow(":a ", "a", DisplayName = "test ':a '")]
	[DataRow(" :a ", "a", DisplayName = "test ' :a '")]
	[DataRow("\t:a ", "a", DisplayName = "test '\t:a '")]
	[DataRow("\t:a\t", "a", DisplayName = "test '\t:a\t'")]
	[DataRow(":a ;comment", "a", DisplayName = "test ':a ;comment'")]
	[DataRow(" :a ;comment", "a", DisplayName = "test ' :a ;comment'")]
	[DataRow("\t:a ;comment", "a", DisplayName = "test '\t:a ;comment'")]
	[DataRow("\t:a\t;comment", "a", DisplayName = "test '\t:a\t;comment'")]
	[DataRow(":_aLongLabel", "_aLongLabel", DisplayName = "test ':_aLongLabel'")]
	[DataRow(":_aLongerLabel", "_aLongerLabel", DisplayName = "test ':_aLongerLabel'")]
	[DataRow(":_anEvenLongerLabel", "_anEvenLongerLabel", DisplayName = "test ':_anEvenLongerLabel'")]
	[DataRow(":_theLongestLabelThatIsPossible00", "_theLongestLabelThatIsPossible00",
		DisplayName = "test ':_theLongestLabelThatIsPossible00'")]
	public void ValidateGetLabel(string text, string expected)
	{
		var sut = text.GetLabel();
		sut.Should().Be(expected);
	}

	#endregion

	#region Command / Generic word handling

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", false, DisplayName = "test quoted string")]
	[DataRow("'q'", false, DisplayName = "test quoted character")]
	[DataRow("word1", true, DisplayName = "test 'word1'")]
	[DataRow("thisCommandIsTooLong", true, DisplayName = "test 'thisCommandIsTooLong'")]
	[DataRow("word p1", true, DisplayName = "test 'word p1'")]
	[DataRow("word p1 p2", true, DisplayName = "test 'word p1 p2'")]
	[DataRow("word p1 p2,p3", true, DisplayName = "test 'word p1 p2,p3'")]
	[DataRow("word p1\t;comment", true, DisplayName = "test 'word p1\t;comment'")]
	[DataRow("word p1 p2; comment", true, DisplayName = "test 'word p1 p2; comment'")]
	[DataRow("word p1 p2,p3 ;\tcomment", true, DisplayName = "test 'word p1 p2,p3 ;\tcomment'")]
	[DataRow("word = 0x00 ;\tcomment", true, DisplayName = "test 'word = 0x00 ;\tcomment'")]
	public void ValidateHasGeneric(string text, bool expected)
	{
		var sut = text.HasGeneric();
		sut.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("", null, null, null, DisplayName = "test empty")]
	[DataRow("    ", null, null, null, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", null, null, null, DisplayName = "test tabs")]
	[DataRow("; comment", null, null, null, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", null, null, null, DisplayName = "test quoted string")]
	[DataRow("'q'", null, null, null, DisplayName = "test quoted character")]
	[DataRow("word1", "word1", null, null, DisplayName = "test 'word1'")]
	[DataRow("word1\t;comment", "word1", null, "comment", DisplayName = "test 'word1\t;comment'")]
	[DataRow("thisCommandIsTooLong", "thisCommandIsTooLong", null, null, DisplayName = "test 'thisCommandIsTooLong'")]
	[DataRow("word p1", "word", "p1", null, DisplayName = "test 'word p1'")]
	[DataRow("word p1 p2", "word", "p1 p2", null, DisplayName = "test 'word p1 p2'")]
	[DataRow("word p1 p2,p3", "word", "p1 p2,p3", null, DisplayName = "test 'word p1 p2,p3'")]
	[DataRow("word p1\t;comment", "word", "p1", "comment", DisplayName = "test 'word p1\t;comment'")]
	[DataRow("word p1 p2; comment", "word", "p1 p2", "comment", DisplayName = "test 'word p1 p2; comment'")]
	[DataRow("word p1 p2,p3 ;\tcomment", "word", "p1 p2,p3", "comment", DisplayName = "test 'word p1 p2,p3 ;\tcomment'")]
	[DataRow("word = 0x00 ;\tcomment", "word", "= 0x00", "comment", DisplayName = "test 'word = 0x00 ;\tcomment'")]
	public void ValidateGetGeneric(string text, string expectedWord, string expectedParameters, string expectedComment)
	{
		var (word, parameters, comment) = text.GetGeneric();
		word.Should().Be(expectedWord);
		parameters.Should().Be(expectedParameters);
		comment.Should().Be(expectedComment);
	}

	#endregion
}