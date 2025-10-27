using System.Text.RegularExpressions;
using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Extensions;

public static partial class SourceLineExtensions
{
	/// <summary>
	/// Determines whether the whole <paramref name="line"/> can be regarded as a comment
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line can be classified entirely as a comment (i.e. the first non-blank character is a semicolon)</returns>
	public static bool IsComment(this SourceLine line)
	{
		return line.Text.IsComment();
	}

	/// <summary>
	/// Determines whether the <paramref name="text"/> is a comment
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>True if the text represents the start of a comment</returns>
	public static bool IsComment(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       text.Trim().StartsWith(';');
	}
	
	/// <summary>
	/// Determines whether the <paramref name="line"/> contains a comment - either as the starting character, or as part of an inline comment
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line contains a semicolon character</returns>
	/// <remarks>Needs to be checked again - the semicolon may be contained within a string literal</remarks>
	public static bool HasComment(this SourceLine line)
	{
		return line.Text.HasComment();
	}

	/// <summary>
	/// Determines whether the <paramref name="text"/> supplied contains an inline comment character
	/// </summary>
	/// <param name="text">The source text to be checked</param>
	/// <returns>True if the semicolon character is found in <paramref name="text"/></returns>
	public static bool HasComment(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       text.Contains(';');
	}
	
	/// <summary>
	/// Determines whether the <paramref name="line"/> is blank - i.e. has no characters, is empty, or contains only whitespace
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line has no content or is wholly whitespace</returns>
	public static bool IsBlank(this SourceLine line)
	{
		return string.IsNullOrWhiteSpace(line.Text);
	}

	/// <summary>
	/// Determines whether the <paramref name="line"/> contains an assembler directive
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line contains one of the assembler directives such as <c>[DATA]</c>, or <c>[PROGRAM]</c></returns>
	public static bool HasDirective(this SourceLine line)
	{
		return DirectiveRegex().Match(line.Text.Trim()).Success;
	}

	/// <summary>
	/// Convert the directive appearing in the <see cref="SourceLine"/> as [text] into the corresponding <see cref="DirectiveType"/>
	/// </summary>
	/// <param name="line">The source to be checked for the directive</param>
	/// <returns>The type of the directive detected, or Unknown</returns>
	public static DirectiveType GetDirectiveType(this SourceLine line)
	{
		return line.Text.GetDirectiveType();
	}

	/// <summary>
	/// Convert the directive appearing in source as [text] into the corresponding <see cref="DirectiveType"/>
	/// </summary>
	/// <param name="text">The text to check for the directive</param>
	/// <returns>The type of the directive detected, or Unknown</returns>
	public static DirectiveType GetDirectiveType(this string text)
	{
		//	Blank text is automatically unknown... What were you thinking???
		if (string.IsNullOrWhiteSpace(text))
			return DirectiveType.Unknown;
		
		//	Check the trimmed text against known directive values
		var m = DirectiveRegex().Match(text.Trim());
		
		/*
		 * A bit of Enum magic to get a "common" value - allows for shortcuts
		 * like [PROG] and [CONST] to be used, yet still return the more common
		 * enum values of "Program" and "Constants":
		 *	1)	parse the text to equivalent DirectiveType enum value
		 *	2)	Mask the value with 0x0F to get the lower nibble value and convert back
		 */
		return Enum.TryParse(m.Groups[2].Value, true, out DirectiveType directiveType)
			? (DirectiveType)((int)directiveType & 0x0f)
			: DirectiveType.Unknown;
	}
	
	/// <summary>
	/// Determines whether the <paramref name="line"/> contains a label identifier as the first part of the input
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the first part of the line starts with the label identifier character</returns>
	public static bool HasLabel(this SourceLine line)
	{
		return line.Text.HasLabel();
	}

	/// <summary>
	/// Determines whether the <paramref name="text"/> contains a label identifier as the first part of the input
	/// </summary>
	/// <param name="text">The source text to be checked</param>
	/// <returns>True if the first part of the text starts with the label identifier character</returns>
	public static bool HasLabel(this string text)
	{
		return LabelRegex().Match(text.Trim()).Success;
	}
	
	/// <summary>
	/// Extracts a label from the <paramref name="line"/>, if one exists
	/// </summary>
	/// <param name="line">The line of source to check</param>
	/// <returns>The name of the label, if present, or a blank string</returns>
	public static string GetLabel(this SourceLine line)
	{
		return line.Text.GetLabel();
	}
	
	/// <summary>
	/// Extracts a label from the <paramref name="text"/>, if one exists
	/// </summary>
	/// <param name="text">The source text to check</param>
	/// <returns>The name of the label, if present, or a blank string</returns>
	public static string GetLabel(this string text)
	{
		//	Nothing there, no label!
		if (string.IsNullOrWhiteSpace(text))
			return string.Empty;
		
		//	Use the regex to grab any label from the text
		var m =  LabelRegex().Match(text.Trim());

		//	Any captured label from the regex should be in group[1], otherwise no label present
		return m.Success 
			? m.Groups[1].Value 
			: string.Empty;
	}
	
    [GeneratedRegex(@"^(\[(PROG(RAM)?|DATA|CONST(ANTS)?)\])", RegexOptions.IgnoreCase)]
    private static partial Regex DirectiveRegex();

    [GeneratedRegex(@"^:([A-Za-z_]\w*)(\s*)?(;)?")]
    private static partial Regex LabelRegex();
}