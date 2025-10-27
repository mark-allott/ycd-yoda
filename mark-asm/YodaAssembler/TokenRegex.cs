using System.Text.RegularExpressions;

namespace YodaAssembler;

public static partial class TokenRegex
{
	/// <summary>
	/// Detects and allows extraction of a comment from the supplied text
	/// </summary>
	/// <returns></returns>
	/// <remarks>Works to determine if the entire line contains a comment</remarks>
	[GeneratedRegex(@"^\s*;\s*(.*)\s*$", RegexOptions.Compiled)]
	public static partial Regex IsComment();

	/// <summary>
	/// Detects and allows extraction of a comment within the supplied text
	/// </summary>
	/// <returns></returns>
	/// <remarks>Works to determine whether the text terminates in an inline comment</remarks>
	[GeneratedRegex(@";\s*(.*)\s*$", RegexOptions.Compiled)]
	public static partial Regex HasComment();

	/// <summary>
	/// Detects and allows extraction of a directive and optional parameter and comment from the supplied text
	/// </summary>
	/// <returns></returns>
	/// <remarks><para>
	/// Directives must follow certain rules:
	/// <ul>
	/// <li>The first character MUST be alpha or underscore</li>
	/// <li>Subsequent characters can be made up of alphanumeric and underscore</li>
	/// <li>The total length of the directive is at least one character and no more than twenty</li>
	/// <li>Whitespace between differing elements is optional</li>
	/// </ul></para>
	/// <para>
	/// Results from the regex matching exercise (i.e. calling <c>Regex.Matches</c>) can be extracted as follows:
	/// <ul>
	/// <li>Group[1] => the directive</li>
	/// <li>Group[2] => the "parameter" for the directive</li>
	/// <li>Group[4] => the inline comment</li>
	/// </ul>
	/// </para>
	/// </remarks>
	[GeneratedRegex(@"^\s*\[([A-Za-z]\w{0,19})\]\s*(\w+)?\s*(;\s*(.*)\s*)?$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex IsDirective();

	/// <summary>
	/// Detects and allows extraction of a label and optional comment from the supplied text
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// <para>
	/// Results from the regex matching exercise (i.e. calling <c>Regex.Matches</c>) can be extracted as follows:
	/// <ul>
	/// <li>Group[1] => the label</li>
	/// <li>Group[3] => the inline comment</li>
	/// </ul>
	/// </para>
	/// </remarks>
	[GeneratedRegex(@"^\s*:([A-Za-z_]\w{0,31})\s*(;(.*))?\s*$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex HasLabel();

	/// <summary>
	/// Extracts a generic single-word, minus any preceding whitespace, followed by any optional parameters and/or comment
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// <ul>
	/// <li>Group[0] => all possible matched elements</li>
	/// <li>Group[1] => the word / command text</li>
	/// <li>Group[2] => any text between the end of the first word and any comment</li>
	/// <li>Group[4] => any inline comment in the text</li>
	/// </ul>
	/// </remarks>
	[GeneratedRegex(@"^\s*(\w+)\s*(.*?)\s*(;\s*(.*))?$", RegexOptions.Compiled)]
	public static partial Regex GenericWord();

	/// <summary>
	/// Extracts a literal string from the supplied text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"^\s*""(.*)""", RegexOptions.Compiled)]
	public static partial Regex LiteralString();

	/// <summary>
	/// Extracts a literal char from the text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"'(\\?.)'", RegexOptions.Compiled)]
	public static partial Regex LiteralChar();

	/// <summary>
	/// Detects and extracts a numeric value from the text
	/// </summary>
	/// <returns></returns>
	/// <remarks>Numbers can be expressed in either hex, binary or decimal forms:
	/// <ul>
	/// <li>Hex form: 0x prefix, with one or two hexadecimal digits</li>
	/// <li>Binary form: 0b prefix, followed by 1 or 2 groups of 4-digit nibble groups, separated by an underscore, or 1 to 8 digits</li>
	/// <li>Decimal form: no prefix, but between 1 and 3 digits</li>
	/// </ul>
	/// </remarks>
	[GeneratedRegex(@"^\s*((0[Xx][\dA-Fa-f]{1,2})|(0[Bb][01]{4}(_[01]{4})?)|(0[Bb][01]{1,8})|(\d{1,3}))\s*$", RegexOptions.Compiled)]
	public static partial Regex LiteralNumber();

	/// <summary>
	/// Detects and extracts a direct numeric value from the text
	/// </summary>
	/// <returns></returns>
	/// <remarks>Numbers can be expressed in either hex, binary or decimal forms, surrounded by square brackets to indicate they are direct:
	/// <ul>
	/// <li>Hex form: 0x prefix, with one or two hexadecimal digits</li>
	/// <li>Binary form: 0b prefix, followed by 1 or 2 groups of 4-digit nibble groups, separated by an underscore, or 1 to 8 digits</li>
	/// <li>Decimal form: no prefix, but between 1 and 3 digits</li>
	/// </ul>
	/// </remarks>
	[GeneratedRegex(@"^\s*\[((0[Xx][\dA-Fa-f]{1,2})|(0[Bb][01]{4}(_[01]{4})?)|(0[Bb][01]{1,8})|(\d{1,3}))\]\s*$", RegexOptions.Compiled)]
	public static partial Regex DirectNumber();

	/// <summary>
	/// Detects and extracts a direct symbol from the text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"^\s*\[(\w{1,32})\]\s*$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex DirectSymbol();
	
	/// <summary>
	/// Detects and extracts an indirect numeric value from the text
	/// </summary>
	/// <returns></returns>
	/// <remarks>Numbers can be expressed in either hex, binary or decimal forms, surrounded by double square brackets to indicate indirection:
	/// <ul>
	/// <li>Hex form: 0x prefix, with one or two hexadecimal digits</li>
	/// <li>Binary form: 0b prefix, followed by 1 or 2 groups of 4-digit nibble groups, separated by an underscore, or 1 to 8 digits</li>
	/// <li>Decimal form: no prefix, but between 1 and 3 digits</li>
	/// </ul>
	/// </remarks>
	[GeneratedRegex(@"^\s*\[\[((0[Xx][\dA-Fa-f]{1,2})|(0[Bb][01]{4}(_[01]{4})?)|(0[Bb][01]{1,8})|(\d{1,3}))\]\]\s*$", RegexOptions.Compiled)]
	public static partial Regex IndirectNumber();

	/// <summary>
	/// Detects and extracts a direct symbol from the text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"^\s*\[\[(\w{1,32})\]\]\s*$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	public static partial Regex IndirectSymbol();

}