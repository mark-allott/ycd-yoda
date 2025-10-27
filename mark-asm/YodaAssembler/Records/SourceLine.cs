using YodaAssembler.Enums;
using YodaAssembler.Extensions;

namespace YodaAssembler.Records;

public record SourceLine
{
	#region Explicit Fields

	/// <summary>
	/// The line number of the text within the larger body of text
	/// </summary>
	public int LineNumber { get; private init; }

	/// <summary>
	/// The text contained on the line
	/// </summary>
	public string Text { get; private init; }

	#endregion

	#region Constructor

	/// <summary>
	/// Constructor to validate / sanitise values 
	/// </summary>
	/// <param name="lineNumber">The line number of the source</param>
	/// <param name="text">The (optional) text for the line of source</param>
	/// <remarks>If the supplied <paramref name="text"/> is null or wholly whitespace, then it is replaced by <c>string.Empty</c>, so there is always a sensible value available</remarks>
	public SourceLine(int lineNumber, string? text)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lineNumber, 0, nameof(lineNumber));
		LineNumber = lineNumber;
		Text = string.IsNullOrWhiteSpace(text)
			? string.Empty
			: text;
	}

	#endregion

	#region Implicit Fields

	/// <summary>
	/// flag to indicate whether the line has been processed / handled etc. 
	/// </summary>
	public bool Handled { get; private set; } = false;

	/// <summary>
	/// indicates whether the line is entirely blank - i.e. no content or wholly made up of whitespace
	/// </summary>
	public bool IsBlank => this.IsBlank();

	/// <summary>
	/// indicates whether the line can be considered wholly as a comment
	/// </summary>
	public bool IsComment => this.IsComment();

	/// <summary>
	/// Indicates whether the line contains a label
	/// </summary>
	public bool IsLabel => this.HasLabel();

	/// <summary>
	/// Indicates whether the line contains a comment (detects both inline and full-line comments)
	/// </summary>
	public bool HasComment => this.HasComment();

	/// <summary>
	/// Determines whether the line is a directive
	/// </summary>
	public bool IsDirective => this.IsDirective();

	/// <summary>
	/// Extracts the <see cref="DirectiveType"/> from the line
	/// </summary>
	public DirectiveType Directive => Text.GetDirectiveType();

	#endregion

	/// <summary>
	/// Used to mark the line as having been completely processed
	/// </summary>
	public void SetHandled() => Handled = true;
}