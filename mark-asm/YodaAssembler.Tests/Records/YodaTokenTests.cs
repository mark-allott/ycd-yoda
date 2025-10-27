using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Tests.Records;

[TestClass]
public class YodaTokenTests
{
	[TestMethod]
	[DataRow(0, 0, false, DisplayName = "0, 0")]
	[DataRow(1, 0, false, DisplayName = "1, 0")]
	[DataRow(0, 1, false, DisplayName = "0, 1")]
	[DataRow(1, 1, false, DisplayName = "1, 1")]
	[DataRow(int.MaxValue, 0, false, DisplayName = "int.MaxValue, 0")]
	[DataRow(0, int.MaxValue, false, DisplayName = "0, int.MaxValue")]
	[DataRow(int.MaxValue, int.MaxValue, false, DisplayName = "int.MaxValue, int.MaxValue")]
	[DataRow(-1, 0, true, DisplayName = "-1, 0")]
	[DataRow(0, -1, true, DisplayName = "0, -1")]
	[DataRow(-1, -1, true, DisplayName = "-1, -1")]
	[DataRow(int.MinValue, 0, true, DisplayName = "int.MinValue, 0")]
	[DataRow(0, int.MinValue, true, DisplayName = "0, int.MinValue")]
	[DataRow(int.MinValue, int.MinValue, true, DisplayName = "int.MinValue, int.MinValue")]
	public void ValidateInvalidTokenCreation(int lineNumber, int lineSequence, bool exceptionExpected)
	{
		if (exceptionExpected)
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => YodaToken.Blank(lineNumber, lineSequence));
			return;
		}

		var sut = YodaToken.Blank(lineNumber, lineSequence);
		sut.Should().NotBeNull();
		sut.LineNumber.Should().Be(lineNumber);
		sut.LineSequence.Should().Be(lineSequence);
	}

	[TestMethod]
	public void ValidateBlankToken()
	{
		YodaToken sut = YodaToken.Blank(0, 0);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.Blank);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.Text.Should().BeNull();
		sut.ToString().Should().BeEmpty();
		sut.ParameterType.Should().Be(ParameterTypes.None);
	}

	[TestMethod]
	[DataRow(";comment", "comment", "; comment", DisplayName = "';comment'")]
	[DataRow(";comment ", "comment", "; comment", DisplayName = "';comment '")]
	[DataRow(";comment\t", "comment", "; comment", DisplayName = "';comment\t'")]
	[DataRow("; comment2", "comment2", "; comment2", DisplayName = "'; comment2'")]
	[DataRow("; comment2 ", "comment2", "; comment2", DisplayName = "'; comment2 '")]
	[DataRow("; comment2\t", "comment2", "; comment2", DisplayName = "'; comment2\t'")]
	[DataRow(";\tcomment3", "comment3", "; comment3", DisplayName = "'; comment3'")]
	[DataRow(";\tcomment3 ", "comment3", "; comment3", DisplayName = "'; comment3 '")]
	[DataRow(";\tcomment3\t", "comment3", "; comment3", DisplayName = "'; comment3\t'")]
	[DataRow(";multiword comment", "multiword comment", "; multiword comment", DisplayName = "';multiword comment'")]
	[DataRow(";multiword comment ", "multiword comment", "; multiword comment", DisplayName = "';multiword comment '")]
	[DataRow(";multiword comment\t", "multiword comment", "; multiword comment", DisplayName = "';multiword comment\t'")]
	[DataRow(" ;comment4", "comment4", "; comment4", DisplayName = "' ;comment4'")]
	[DataRow("\t;comment4 ", "comment4", "; comment4", DisplayName = "'\t;comment4 '")]
	[DataRow(" \t;comment4\t", "comment4", "; comment4", DisplayName = "' \t;comment4\t'")]
	public void ValidateCommentToken(string comment, string expectedText, string expectedString)
	{
		YodaToken sut = YodaToken.Comment(0, 0, comment);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.Comment);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.LineNumber.Should().Be(0);
		sut.Text.Should().Be(expectedText);
		sut.ToString().Should().Be(expectedString);
		sut.ParameterType.Should().Be(ParameterTypes.None);
	}

	[TestMethod]
	[DataRow("prefix;comment", DisplayName = "Check with 'prefix;comment'")]
	[DataRow("prefix ;comment", DisplayName = "Check with 'prefix ;comment'")]
	[DataRow("prefix\t;comment", DisplayName = "Check with 'prefix\t;comment'")]
	[DataRow(" prefix ;comment", DisplayName = "Check with ' prefix ;comment'")]
	[DataRow("\tprefix\t;comment", DisplayName = "Check with '\tprefix\t;comment'")]
	public void ValidateBadCommentToken(string testText)
	{
		Assert.ThrowsException<ArgumentException>(() => YodaToken.Comment(0, 0, testText));
	}

	[TestMethod]
	[DataRow(null, true, DisplayName = "test null")]
	[DataRow("", true, DisplayName = "test ''")]
	[DataRow("[]", true, DisplayName = "test '[]'")]
	[DataRow("[_]", true, DisplayName = "test '[_]'")]
	[DataRow("[0]", true, DisplayName = "test '[0]'")]
	[DataRow("[a ]", true, DisplayName = "test '[a ]'")]
	[DataRow("[ a]", true, DisplayName = "test '[ a]'")]
	[DataRow("[ a ]", true, DisplayName = "test '[ a ]'")]
	[DataRow("[a.]", true, DisplayName = "test '[a.]'")]
	[DataRow("[a.b]", true, DisplayName = "test '[a.b]'")]
	[DataRow("[a+b]", true, DisplayName = "test '[a+b]'")]
	[DataRow("[a]", false, "a", DisplayName = "test [a]")]
	[DataRow("[a] param", false, "a", DisplayName = "test [a] param")]
	[DataRow("[a];comment", false, "a", DisplayName = "test [a];comment")]
	[DataRow("[a] ;comment", false, "a", DisplayName = "test [a] ;comment")]
	[DataRow("[a]\t;comment", false, "a", DisplayName = "test [a]\t;comment")]
	[DataRow("[a] param;comment", false, "a", DisplayName = "test [a] param;comment")]
	[DataRow("[a] param ;comment", false, "a", DisplayName = "test [a] param ;comment")]
	[DataRow("[a] param ; comment", false, "a", DisplayName = "test [a] param ; comment")]
	[DataRow("[a] param\t;comment", false, "a", DisplayName = "test [a] param\t;comment")]
	[DataRow("[a] param\t;\tcomment", false, "a", DisplayName = "test [a] param\t;\tcomment")]
	[DataRow("[a] param\t; multiword comment", false, "a", DisplayName = "test [a] param\t; multiword comment")]
	[DataRow(" [a] param\t; multiword comment", false, "a", DisplayName = "test ' [a] param\t; multiword comment'")]
	[DataRow("\t[a] param\t; multiword comment", false, "a", DisplayName = "test '\t[a] param\t; multiword comment'")]
	[DataRow("\t [a] param\t; multiword comment", false, "a", DisplayName = "test'\t  [a] param\t; multiword comment'")]
	[DataRow("[a_b]", false, "a_b", DisplayName = "test [a_b]")]
	[DataRow("[a00]", false, "a00", DisplayName = "test [a00]")]
	[DataRow("[aLongDirectiveNameOk]", false, "aLongDirectiveNameOk", DisplayName = "test [aLongDirectiveNameOk]")]
	[DataRow("[aLongDirectiveNameBad]", true, DisplayName = "test [aLongDirectiveNameBad]")]
	public void ValidateDirectiveToken(string? text, bool throwsException, string? expectedDirective = null)
	{
		if (throwsException)
		{
			if (text is null)
				Assert.ThrowsException<ArgumentNullException>(() => YodaToken.Directive(0, 0, text!));
			else
				Assert.ThrowsException<ArgumentException>(() => YodaToken.Directive(0, 0, text));
			return;
		}

		//	For all non-throwing tests, there should be a non-null expectedDirective value
		expectedDirective.Should().NotBeNull();

		YodaToken sut = YodaToken.Directive(0, 0, text!);
		sut.Should().NotBeNull();
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.Text.Should().Be(expectedDirective);
		sut.TokenType.Should().Be(TokenType.Directive);
		sut.ToString().Should().Be($"[{expectedDirective}]");
		sut.ParameterType.Should().Be(ParameterTypes.None);
		
		//	Verify that a directive is of type YodaDirectiveToken
		sut.Should().BeOfType<YodaDirectiveToken>();
		YodaDirectiveToken dt = (sut as  YodaDirectiveToken)!;
		dt.Should().NotBeNull();
		//	None of the tests should yield a valid DirectiveType value 
		dt.DirectiveType.Should().Be(DirectiveType.Unknown);
	}

	private static readonly string[] IndentStrings = ["", " ", "\t"];
	private static readonly char[] LabelGoodFirstChars = ['A', 'Z', 'a', 'z', '_'];
	private static readonly char[] LabelGoodOtherChars = ['A', 'Z', 'a', 'z', '_', '0', '9'];

	private static string GenerateRandomLabel(int totalLength)
	{
		//	Need to generate a valid, but random label that tests the limit of the label length
		Random rng = new Random();
		StringBuilder sb = new StringBuilder();
		sb.Append(LabelGoodFirstChars[rng.Next(LabelGoodFirstChars.Length)]);
		Enumerable.Range(0, totalLength - 1).ToList()
			.ForEach(i => sb.Append(LabelGoodOtherChars[rng.Next(LabelGoodOtherChars.Length)]));
		return sb.ToString();
	}
	
	internal static IEnumerable<object[]> LabelTokenGoodData()
	{
		//	Assemble a list of initial characters for the label, with varying whitespace prefix and suffix
		var singleCharData = IndentStrings
			.SelectMany(fc => LabelGoodFirstChars, (pc, fc) => new { prefix = pc, firstChar = fc })
			//	Append additional whitespace
			.SelectMany(suffix => IndentStrings, (id, suffix) => new { id.prefix, id.firstChar, suffix })
			.ToList();

		//	Convert the selections into an object[] of text to pass into the YodaToken constructor, plus expected label
		List<object[]> data = singleCharData
			.Select(s => new object[] { $"{s.prefix}:{s.firstChar}{s.suffix}", $"{s.firstChar}" })
			.ToList();
		//	Using the same single-character label data, append an inline comment
		data.AddRange(singleCharData.Select(s => new object[]
			{ $"{s.prefix}:{s.firstChar}{s.suffix};comment text", $"{s.firstChar}" }));

		//	Assemble another list of additional 2nd valid characters for a label, with varying whitespace prefix and suffix 
		var secondData = singleCharData
			.SelectMany(sc => LabelGoodOtherChars, (id, sc) => new { id.prefix, label = $"{id.firstChar}{sc}", id.suffix })
			.ToList();
		//	Add this extended 2-character combination for testing
		data.AddRange(secondData.Select(s => new object[] { $"{s.prefix}:{s.label}{s.suffix}", s.label }));
		//	Using the same 2-character label data, append an inline comment
		data.AddRange(secondData.Select(s => new object[]
			{ $"{s.prefix}:{s.label}{s.suffix} ; 2char comment text", s.label }));

		//	Need to generate a valid, but random label that tests the limit of the label length
		string longLabel = GenerateRandomLabel(32);

		//	Add long label test
		data.Add([$":{longLabel}", longLabel]);
		//	Add long label + comment test
		data.Add([$":{longLabel}\t;\tLong label comment", longLabel]);
		return data;
	}

	public static string LabelTokenTestDataDisplayName(MethodInfo methodInfo, object[] data)
	{
		return $"Testing '{data[0]}'";
	}

	[TestMethod]
	[DynamicData(nameof(LabelTokenGoodData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(LabelTokenTestDataDisplayName))]
	public void ValidateLabelToken(string text, string expectedLabel)
	{
		YodaToken sut = YodaToken.Label(0, 0, text);
		sut.Should().NotBeNull();
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.Text.Should().Be(expectedLabel);
		sut.TokenType.Should().Be(TokenType.Label);
		sut.ToString().Should().Be($":{expectedLabel}");
		sut.ParameterType.Should().Be(ParameterTypes.None);
	}

	/// <summary>
	/// Test data for bad labels needs to take account of the LabelRegex detection using ECMAScript flags, which restricts the \w token to the equivalent of [A-Za-z0-9_]
	/// </summary>
	/// <returns></returns>
	internal static IEnumerable<object[]> LabelTokenBadData()
	{
		string[] badCharacters = ["", ".", "@", "#", "?", "^", "*"];

		//	Assemble a list of initial characters for the label, with varying whitespace prefix and suffix
		var singleCharData = IndentStrings
			.SelectMany(fc => badCharacters, (pc, fc) => new { prefix = pc, firstChar = fc })
			//	Append additional whitespace
			.SelectMany(ts => IndentStrings, (id, ts) => new { id.prefix, id.firstChar, suffix = ts })
			.ToList();

		//	Convert the selections into an object[] of text to pass into the YodaToken constructor, plus expected label
		List<object[]> data = singleCharData
			.Select(s => new object[] { $"{s.prefix}:{s.firstChar}{s.suffix}" })
			.ToList();
		//	Add same selections with an inline comment appended to it
		data.AddRange(singleCharData.Select(s => new object[]
			{ $"{s.prefix}:{s.firstChar}{s.suffix};comment text" }));

		//	Start with a good character, then throw a bad one in there
		var doubleCharData = IndentStrings
			.SelectMany(ic => LabelGoodFirstChars, (id, ic) => new { prefix = id, label = ic })
			.SelectMany(b => badCharacters.Where(q => !string.IsNullOrEmpty(q)),
				(d, b) => new { d.prefix, label = $"{d.label}{b}" })
			.SelectMany(s => IndentStrings, (d, s) => new { d.prefix, d.label, suffix = s })
			.ToList();

		//	Add the bad label checks
		data.AddRange(doubleCharData.Select(s => new object[] { $"{s.prefix}:{s.label}{s.suffix}" }));
		//	Add the same bad labels, appending an inline comment
		data.AddRange(doubleCharData.Select(s => new object[] { $"{s.prefix}:{s.label}{s.suffix} ; 2char comment text" }));

		//	Need to generate a valid, but random label that tests the limit of the label length
		string longLabel = GenerateRandomLabel(33);

		//	Add long label test
		data.Add([$":{longLabel}"]);
		//	Add long label + comment test
		data.Add([$":{longLabel}\t;\tBad long label comment"]);
		return data;
	}

	[TestMethod]
	[DynamicData(nameof(LabelTokenBadData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(LabelTokenTestDataDisplayName))]
	public void ValidateBadLabelToken(string text)
	{
		Assert.ThrowsException<ArgumentException>(() => YodaToken.Label(0, 0, text));
	}

	private void TestGenericRegex(TokenType tokenType, string text, bool isValid, string? expected)
	{
		Func<int, int, string, YodaToken> creator = tokenType switch
		{
			TokenType.Command => YodaToken.Command,
			TokenType.Operand => YodaToken.Operand,
			TokenType.Symbol => YodaToken.Symbol,
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
		};

		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => creator(0, 0, text));
			return;
		}

		YodaToken sut = creator(0, 0, text);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(tokenType);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.ToString().Should().Be(expected);
		sut.ParameterType.Should().Be(tokenType == TokenType.Symbol ? ParameterTypes.Symbol : ParameterTypes.None);
	}
	
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", false, DisplayName = "test quoted string")]
	[DataRow("'q'", false, DisplayName = "test quoted character")]
	[DataRow("word1", true, "word1", DisplayName = "test 'word1'")]
	[DataRow("thisCommandIsVeryLong", true, "thisCommandIsVeryLong", DisplayName = "test 'thisCommandIsVeryLong'")]
	[DataRow("word p1", true, "word", DisplayName = "test 'word p1'")]
	[DataRow("word p1 p2", true, "word", DisplayName = "test 'word p1 p2'")]
	[DataRow("word p1 p2,p3", true, "word", DisplayName = "test 'word p1 p2,p3'")]
	[DataRow("word p1\t;comment", true, "word", DisplayName = "test 'word p1\t;comment'")]
	[DataRow("word p1 p2; comment", true, "word", DisplayName = "test 'word p1 p2; comment'")]
	[DataRow("word p1 p2,p3 ;\tcomment", true, "word", DisplayName = "test 'word p1 p2,p3 ;\tcomment'")]
	[DataRow("word = 0x00 ;\tcomment", true, "word", DisplayName = "test 'word = 0x00 ;\tcomment'")]
	public void ValidateCommandToken(string text, bool isValid, string? expected = null)
	{
		TestGenericRegex(TokenType.Command, text, isValid, expected);
	}
	
	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", false, DisplayName = "test quoted string")]
	[DataRow("'q'", false, DisplayName = "test quoted character")]
	[DataRow("word1", true, "word1", DisplayName = "test 'word1'")]
	[DataRow("thisCommandIsVeryLong", true, "thisCommandIsVeryLong", DisplayName = "test 'thisCommandIsVeryLong'")]
	[DataRow("word p1", true, "word", DisplayName = "test 'word p1'")]
	[DataRow("word p1 p2", true, "word", DisplayName = "test 'word p1 p2'")]
	[DataRow("word p1 p2,p3", true, "word", DisplayName = "test 'word p1 p2,p3'")]
	[DataRow("word p1\t;comment", true, "word", DisplayName = "test 'word p1\t;comment'")]
	[DataRow("word p1 p2; comment", true, "word", DisplayName = "test 'word p1 p2; comment'")]
	[DataRow("word p1 p2,p3 ;\tcomment", true, "word", DisplayName = "test 'word p1 p2,p3 ;\tcomment'")]
	[DataRow("word = 0x00 ;\tcomment", true, "word", DisplayName = "test 'word = 0x00 ;\tcomment'")]
	public void ValidateOperandToken(string text, bool isValid, string? expected = null)
	{
		TestGenericRegex(TokenType.Operand, text, isValid, expected);
	}

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", true, "quoted string", DisplayName = "test quoted string")]
	[DataRow("'q'", false, DisplayName = "test quoted character")]
	[DataRow("\"quoted string\";with comment", true, "quoted string", DisplayName = "test \"quoted string\";with comment")]
	[DataRow("  \"quoted string\";with comment", true, "quoted string", DisplayName = "test '  \"quoted string\";with comment'")]
	[DataRow("\t\"quoted string\" ;with comment", true, "quoted string", DisplayName = "test '\t\"quoted string\" ;with comment'")]
	public void ValidateLiteralStringToken(string text, bool isValid, string? expected = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.LiteralString(0, 0, text));
			return;
		}

		YodaToken sut = YodaToken.LiteralString(0, 0, text);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.LiteralString);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.Text.Should().Be(expected);
		sut.ToString().Should().Be($"\"{expected}\"");
		sut.ParameterType.Should().Be(ParameterTypes.LiteralString);
	}

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", false, DisplayName = "test quoted string")]
	[DataRow("'q'", true, "q", DisplayName = "test quoted character")]
	[DataRow("'quoted string';with comment", false, DisplayName = "test 'quoted string';with comment")]
	[DataRow("  'q';with comment", true, "q", DisplayName = "test '  'q';with comment'")]
	[DataRow("\t\'q' ;with comment", true, "q", DisplayName = "test '\t\'q' ;with comment'")]
	public void ValidateLiteralCharToken(string text, bool isValid, string? expected = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.LiteralChar(0, 0, text));
			return;
		}

		YodaToken sut = YodaToken.LiteralChar(0, 0, text);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.LiteralChar);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.Text.Should().Be(expected);
		sut.ToString().Should().Be($"'{expected}'");
		sut.ParameterType.Should().Be(ParameterTypes.LiteralChar);
	}

	internal static IEnumerable<object[]> LiteralNumberData()
	{
		yield return ["", false, null!, null!, "empty"];
		yield return ["    ", false, null!, null!, "spaces"];
		yield return ["\t\t\t", false, null!, null!, "tabs"];
		yield return ["; comment", false, null!, null!, "comment"];
		yield return ["0x", false, null!, null!, "0x"];
		yield return ["0x 00", false, null!, null!, "0x 00"];
		yield return ["0X", false, null!, null!, "0X"];
		yield return ["0X 00", false, null!, null!, "0X 00"];
		yield return ["0xg", false, null!, null!, "0xg"];
		yield return ["0x123", false, null!, null!, "0x123"];
		yield return ["0b", false, null!, null!, "0b"];
		yield return ["0b 00", false, null!, null!, "0b 00"];
		yield return ["0B", false, null!, null!, "0B"];
		yield return ["0B 00", false, null!, null!, "0B 00"];
		yield return ["0b110011001", false, null!, null!, "0b110011001"];
		yield return ["0b0110_", false, null!, null!, "0b0110_"];
		yield return ["0b0110_1", false, null!, null!, "0b0110_1"];
		yield return ["0b0110_11", false, null!, null!, "0b0110_11"];
		yield return ["0b0110_110", false, null!, null!, "0b0110_110"];
		yield return ["0b110_1101", false, null!, null!, "0b110_1101"];
		yield return ["1234", false, null!, null!, "1234"];
		yield return ["0x00 ; comment", false, null!, null!, "0x00 ; comment"];
		yield return ["0X00\t; comment", false, null!, null!, "0X00 ;\tcomment"];
		yield return ["0b00 ; comment", false, null!, null!, "0b00 ; comment"];
		yield return ["0B00\t; comment", false, null!, null!, "0B00 ;\tcomment"];
		yield return ["0 ; comment", false, null!, null!, "0 ; comment"];
		yield return ["0\t; comment", false, null!, null!, "0 ;\tcomment"];
		yield return ["-1", false, null!, null!, "-1"];
		yield return ["+1", false, null!, null!, "+1"];
		yield return ["0x0", true, "0", "0x00", "0x0"];
		yield return ["0x1", true, "1", "0x01", "0x1"];
		yield return ["0x20", true, "20", "0x20", "0x20"];
		yield return ["0xA0", true, "A0", "0xa0", "0xa0"];
		yield return ["0xFf", true, "Ff", "0xff", "0xFf"];
		yield return ["0b0", true, "0", "0b00000000", "0b0"];
		yield return ["0b10", true, "10", "0b00000010", "0b10"];
		yield return ["0b110", true, "110", "0b00000110", "0b110"];
		yield return ["0b0110", true, "0110", "0b00000110", "0b0110"];
		yield return ["0b10110", true, "10110", "0b00010110", "0b10110"];
		yield return ["0b110110", true, "110110", "0b00110110", "0b110110"];
		yield return ["0b0110110", true, "0110110", "0b00110110", "0b0110110"];
		yield return ["0b10110110", true, "10110110", "0b10110110", "0b10110110"];
		yield return ["0b0110_1101", true, "01101101", "0b01101101", "0b0110_1101"];
	}
	
	public static string LiteralNumberTokenTestDataDisplayName(MethodInfo methodInfo, object[] data)
	{
		return $"Testing '{data[^1]}'";
	}

	[TestMethod]
	[DynamicData(nameof(LiteralNumberData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(LiteralNumberTokenTestDataDisplayName))]
	public void ValidateLiteralNumberToken(string text, bool isValid, string? expectedText = null,
		string? expectedToString = null, string? displayName = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.LiteralNumber(0, 0, text));
			return;
		}

		YodaToken sut = YodaToken.LiteralNumber(0, 0, text);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.LiteralNumber);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		//	Text should have prefixes stripped for hex/binary notations
		sut.Text.Should().Be(expectedText);
		//	ToString should have prefixes present for hex/binary and have 2-digits for hex and 8-digits for binary; decimal shall be numeric only
		sut.ToString().Should().Be($"{expectedToString}");
		sut.ParameterType.Should().Be(ParameterTypes.LiteralNumber);
	}

	[TestMethod]
	[DataRow("256", false, DisplayName = "256")]
	[DataRow("0", true, 0, DisplayName = "0")]
	[DataRow("00", true, 0, DisplayName = "00")]
	[DataRow("000", true, 0, DisplayName = "000")]
	[DataRow("001", true, 1, DisplayName = "001")]
	[DataRow("020", true, 20, DisplayName = "020")]
	[DataRow("255", true, 255, DisplayName = "255")]
	[DataRow("0x0", true, 0, DisplayName = "test 0x0")]
	[DataRow("0xf", true, 15, DisplayName = "test 0xf")]
	[DataRow("0xf0", true, 240, DisplayName = "test 0xf0")]
	[DataRow("0b0", true, 0,  DisplayName = "test 0b0")]
	[DataRow("0b10", true, 2,  DisplayName = "test 0b10")]
	[DataRow("0b1010", true, 10,  DisplayName = "test 0b1010")]
	[DataRow("0b10101010", true, 170,  DisplayName = "test 0b10101010")]
	[DataRow("0b0101_0101", true, 85,  DisplayName = "test 0b0101_0101")]
	public void ValidateLiteralNumberTokenValue(string text, bool isValid, int? expectedValue = 0)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => YodaToken.LiteralNumber(0, 0, text));
			return;
		}

		YodaToken sut = YodaToken.LiteralNumber(0, 0, text);
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.LiteralNumber);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		sut.ParameterType.Should().Be(ParameterTypes.LiteralNumber);
		sut.Should().BeAssignableTo<YodaNumericValueToken>();
		YodaNumericValueToken? numericToken = sut as YodaNumericValueToken;
		numericToken.Should().NotBeNull();
		numericToken.NumericValue.Should().Be(expectedValue);
	}

	[TestMethod]
	[DataRow("", false, DisplayName = "test empty")]
	[DataRow("    ", false, DisplayName = "test spaces")]
	[DataRow(@"\t\t\t", false, DisplayName = "test tabs")]
	[DataRow("; comment", false, DisplayName = "test comment")]
	[DataRow("\"quoted string\"", false, DisplayName = "test quoted string")]
	[DataRow("'q'", false, DisplayName = "test quoted character")]
	[DataRow("word1", true, "word1", DisplayName = "test 'word1'")]
	[DataRow("thisCommandIsVeryLong", true, "thisCommandIsVeryLong", DisplayName = "test 'thisCommandIsVeryLong'")]
	[DataRow("word p1", true, "word", DisplayName = "test 'word p1'")]
	[DataRow("word p1 p2", true, "word", DisplayName = "test 'word p1 p2'")]
	[DataRow("word p1 p2,p3", true, "word", DisplayName = "test 'word p1 p2,p3'")]
	[DataRow("word p1\t;comment", true, "word", DisplayName = "test 'word p1\t;comment'")]
	[DataRow("word p1 p2; comment", true, "word", DisplayName = "test 'word p1 p2; comment'")]
	[DataRow("word p1 p2,p3 ;\tcomment", true, "word", DisplayName = "test 'word p1 p2,p3 ;\tcomment'")]
	[DataRow("word = 0x00 ;\tcomment", true, "word", DisplayName = "test 'word = 0x00 ;\tcomment'")]
	public void ValidateSymbolToken(string text, bool isValid, string? expected = null)
	{
		TestGenericRegex(TokenType.Symbol, text, isValid, expected);
	}

	public static string DirectNumberTokenTestDataDisplayName(MethodInfo methodInfo, object[] data)
	{
		var textParts = $"{data[^1]}".Split(';', StringSplitOptions.TrimEntries);
		var valueText = $"[{textParts[0]}]";
		var text = textParts.Length == 1
			? valueText
			: string.Concat(valueText, " ; ", string.Join("; ", textParts[1..]));
		var isFailure = data[1] is bool && !((bool)data[1]);
		return $"Testing {text} {(isFailure ? "fails" : "validates")}";
	}

	private static (string? data, string? value) GenerateDirectTokenText(string? input, bool isSymbol = false)
	{
		//	Null, empty or whitespace is unchanged
		//	If it starts with a comment (with or without whitespace prefix), also return unchanged
		if (string.IsNullOrEmpty(input) || input.Trim().StartsWith(';'))
			return (input, "");

		//	Use a regex to split the input into desired elements
		var splitter = new Regex(@"^(\s*)(\S*)(\s*)(;.*)?$", RegexOptions.Compiled);
		var m = splitter.Match(input);
		string[] parts = [m.Groups[1].Value, $"[{m.Groups[2].Value}]", m.Groups[3].Value, m.Groups[4].Value];
		return (string.Join("", parts), isSymbol ? m.Groups[2].Value : parts[1]);
	}

	[TestMethod]
	[DynamicData(nameof(LiteralNumberData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DirectNumberTokenTestDataDisplayName))]
	public void ValidateDirectNumberToken(string text, bool isValid, string? expectedText = null,
		string? expectedToString = null, string? displayName = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.DirectNumber(0, 0, $"[{text}]"));
			return;
		}

		var (data, value) = GenerateDirectTokenText(text);
		YodaToken sut = YodaToken.DirectNumber(0, 0, data ?? "");
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.DirectNumber);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		//	Text for direct number should NOT have prefixes stripped for hex/binary notations
		sut.Text.Should().Be(value);
		//	ToString should have prefixes present for hex/binary and have 2-digits for hex and 8-digits for binary; decimal shall be numeric only
		sut.ToString().Should().Be($"[{expectedToString}]");
		sut.ParameterType.Should().Be(ParameterTypes.DirectNumber);
	}
	
	internal static IEnumerable<object[]> SymbolData()
	{
		yield return ["", false, null!, "empty"];
		yield return ["    ", false, null!, "spaces"];
		yield return ["\t\t\t", false, null!, "tabs"];
		yield return ["; comment", false, null!, "comment only"];
		yield return ["0x", false, null!, "0x"];
		yield return ["1234", false, null!, "1234"];
		yield return ["-1", false, null!, "-1"];
		yield return ["+1", false, null!, "+1"];
		yield return ["_", true, "_", "_"];
		yield return ["A", true, "A", "A"];
		yield return ["Z", true, "Z", "Z"];
		yield return ["a", true, "a", "a"];
		yield return ["z", true, "z", "z"];
		yield return ["aLongSymbol", true, "aLongSymbol", "aLongSymbol"];
		yield return ["\tsymbol", true, "symbol", "\tsymbol"];
		yield return ["symbol ", true, "symbol", "symbol "];
		yield return ["symbol;comment", false, null!, "symbol;comment"];
		yield return ["\tsymbol;comment", false, null!, "\tsymbol;comment"];
		yield return ["\tsymbol ;comment", false, null!, "\tsymbol ;comment"];
		yield return ["\tsymbol; comment", false, null!, "\tsymbol; comment"];

		//	Test longer labels, plus one out-of-bounds length
		foreach (var i in new[] { 15, 20, 25, 32, 33 })
		{
			var randomLabel = GenerateRandomLabel(i);
			yield return [randomLabel, i != 33, randomLabel, randomLabel];
		}
	}

	[TestMethod]
	[DynamicData(nameof(SymbolData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(DirectNumberTokenTestDataDisplayName))]
	public void ValidateDirectSymbolToken(string text, bool isValid, string? expectedSymbol = null,
		string? displayName = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.DirectSymbol(0, 0, $"[{text}]"));
			return;
		}

		var (data, value) = GenerateDirectTokenText(text, true);
		YodaToken sut = YodaToken.DirectSymbol(0, 0, data ?? "");
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.DirectSymbol);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		//	Text for direct symbol should be trimmed
		sut.Text.Should().Be(value);
		//	ToString should be expressed in form [symbol] 
		sut.ToString().Should().Be($"[{value}]");
		sut.ParameterType.Should().Be(ParameterTypes.DirectSymbol);
	}
	
	private static (string? data, string? value) GenerateIndirectTokenText(string? input, bool isSymbol = false)
	{
		//	Null, empty or whitespace is unchanged
		//	If it starts with a comment (with or without whitespace prefix), also return unchanged
		if (string.IsNullOrEmpty(input) || input.Trim().StartsWith(';'))
			return (input, "");

		//	Use a regex to split the input into desired elements
		var splitter = new Regex(@"^(\s*)(\S*)(\s*)(;.*)?$", RegexOptions.Compiled);
		var m = splitter.Match(input);
		string[] parts = [m.Groups[1].Value, $"[[{m.Groups[2].Value}]]", m.Groups[3].Value, m.Groups[4].Value];
		return (string.Join("", parts), isSymbol ? m.Groups[2].Value : parts[1]);
	}

	public static string IndirectNumberTokenTestDataDisplayName(MethodInfo methodInfo, object[] data)
	{
		var textParts = $"{data[^1]}".Split(';', StringSplitOptions.TrimEntries);
		var valueText = $"[[{textParts[0]}]]";
		var text = textParts.Length == 1
			? valueText
			: string.Concat(valueText, " ; ", string.Join("; ", textParts[1..]));
		var isFailure = data[1] is bool && !((bool)data[1]);
		return $"Testing {text} {(isFailure ? "fails" : "validates")}";
	}

	[TestMethod]
	[DynamicData(nameof(LiteralNumberData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(IndirectNumberTokenTestDataDisplayName))]
	public void ValidateIndirectNumberToken(string text, bool isValid, string? expectedText = null,
		string? expectedToString = null, string? displayName = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.IndirectNumber(0, 0, $"[{text}]"));
			return;
		}

		var (data, value) = GenerateIndirectTokenText(text);
		YodaToken sut = YodaToken.IndirectNumber(0, 0, data ?? "");
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.IndirectNumber);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		//	Text for direct number should NOT have prefixes stripped for hex/binary notations
		sut.Text.Should().Be(value);
		//	ToString should have prefixes present for hex/binary and have 2-digits for hex and 8-digits for binary; decimal shall be numeric only
		sut.ToString().Should().Be($"[[{expectedToString}]]");
		sut.ParameterType.Should().Be(ParameterTypes.IndirectNumber);
	}
	
	[TestMethod]
	[DynamicData(nameof(SymbolData), DynamicDataSourceType.Method,
		DynamicDataDisplayName = nameof(IndirectNumberTokenTestDataDisplayName))]
	public void ValidateIndirectSymbolToken(string text, bool isValid, string? expectedText = null,
		string? displayName = null)
	{
		if (!isValid)
		{
			Assert.ThrowsException<ArgumentException>(() => YodaToken.IndirectSymbol(0, 0, $"[{text}]"));
			return;
		}

		var (data, value) = GenerateIndirectTokenText(text, true);
		YodaToken sut = YodaToken.IndirectSymbol(0, 0, data ?? "");
		sut.Should().NotBeNull();
		sut.TokenType.Should().Be(TokenType.IndirectSymbol);
		sut.LineNumber.Should().Be(0);
		sut.LineSequence.Should().Be(0);
		//	Text for indirect symbol should be trimmed
		sut.Text.Should().Be(value);
		//	ToString should be expressed in form [[symbol]] 
		sut.ToString().Should().Be($"[[{value}]]");
		sut.ParameterType.Should().Be(ParameterTypes.IndirectSymbol);
	}
}