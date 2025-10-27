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
	[GeneratedRegex(@"^\s*\[([A-Za-z]\w{0,19})\]\s*(\w+)?\s*(;\s*(.*)\s*)?$",
		RegexOptions.Compiled | RegexOptions.ECMAScript)]
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
}