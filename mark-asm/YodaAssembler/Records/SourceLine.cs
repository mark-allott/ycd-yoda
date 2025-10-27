using YodaAssembler.Enums;
using YodaAssembler.Extensions;

namespace YodaAssembler.Records;

public record SourceLine
{
	#region Explicit Fields

	/// <summary>
	/// The line number of the text within the larger body of text
	/// </summary>
	public required int LineNumber { get; init; }

	/// <summary>
	/// The text contained on the line
	/// </summary>
	public required string Text { get; init; }

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
	public bool IsLabel => this.HasLabel();
	public bool HasComment => this.HasComment();
	public bool HasDirective => this.HasDirective();
	public DirectiveType Directive => Text.GetDirectiveType();

	#endregion


	public void SetHandled() => Handled = true;
}