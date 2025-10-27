using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Extensions;

public static partial class SourceLineExtensions
{
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
	/// Determines whether the whole <paramref name="line"/> can be regarded as a comment
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line can be classified entirely as a comment (i.e. the first non-blank character is a semicolon)</returns>
	public static bool IsComment(this SourceLine line)
	{
		return line.Text.IsComment();
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
	/// Extracts a comment from the <paramref name="line"/>
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>The comment associated with the line</returns>
	public static string GetComment(this SourceLine line)
	{
		return line.Text.GetComment();
	}
	
	/// <summary>
	/// Determines whether the <paramref name="line"/> contains an assembler directive
	/// </summary>
	/// <param name="line">The line of source to be checked</param>
	/// <returns>True if the line contains one of the assembler directives such as <c>[DATA]</c>, or <c>[PROGRAM]</c></returns>
	public static bool IsDirective(this SourceLine line)
	{
		return line.Text.IsDirective();
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
	/// Converts the <paramref name="directive"/> into its masked equivalent
	/// </summary>
	/// <param name="directive">The directive to be masked</param>
	/// <returns>The <see cref="DirectiveType"/>, masked to lowest nibble values</returns>
	private static DirectiveType MaskedDirectiveType(DirectiveType directive)
	{
		return (DirectiveType)((int)directive & 0x0f);
	}

	/// <summary>
	/// Convert the directive appearing in source as [text] into the corresponding <see cref="DirectiveType"/>
	/// </summary>
	/// <param name="text">The text to check for the directive</param>
	/// <returns>The type of the directive detected, or Unknown</returns>
	public static DirectiveType GetDirectiveType(this string text)
	{
		//	Get the directive, from one of the DirectiveType values, from text (if present)
		var result = text.GetDirective<DirectiveType>();
		//	Mask the returned value with 0x0f
		//	This has the effect of limiting the output directives to the "base" values, but still permitting use of the abbreviations or alternates
		return MaskedDirectiveType(result);
	}

	/// <summary>
	/// Wrapper around <see cref="TokenTypeExtensions.GetDirectiveDetail"/>, but ensures the directive is of type <see cref="DirectiveType"/> and masked to lowest nibble values
	/// </summary>
	/// <param name="line">The text to be parsed for a directive</param>
	/// <returns>The <see cref="DirectiveType"/>, plus and optional parameter and comment associated with it</returns>
	/// <remarks>If the parameter or comment is missing, then a <c>null</c> value shall be returned for them</remarks>
	public static (DirectiveType directive, string? parameter, string? comment) GetDirectiveDetail(this SourceLine line)
	{
		if (string.IsNullOrWhiteSpace(line.Text))
			return (DirectiveType.Unknown, null!, null!);
		var (directive, parameter, comment) = line.Text.GetDirectiveDetail<DirectiveType>();
		return (MaskedDirectiveType(directive), parameter, comment);
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
	/// Extracts a label from the <paramref name="line"/>, if one exists
	/// </summary>
	/// <param name="line">The line of source to check</param>
	/// <returns>The name of the label, if present, or a blank string</returns>
	public static string GetLabel(this SourceLine line)
	{
		return line.Text.GetLabelDetail().label!;
	}
}