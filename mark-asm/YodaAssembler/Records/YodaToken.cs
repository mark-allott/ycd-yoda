using System.Text.RegularExpressions;
using YodaAssembler.Enums;
using YodaAssembler.Interfaces;

namespace YodaAssembler.Records;

public partial record YodaToken
	: IToken
{
	#region IToken implementation

	public TokenType TokenType { get; private init; }
	public int LineNumber { get; private init; }
	public int LineSequence { get; private init; }
	public string? Text { get; private init; }

	#endregion

	#region Private classes etc.

	/// <summary>
	/// Extracts a comment from the supplied text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@";(.*)$", RegexOptions.Compiled)]
	private static partial Regex CommentRegex();

	/// <summary>
	/// Extracts a directive from the supplied text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"\[(\w)\]", RegexOptions.Compiled)]
	private static partial Regex DirectiveRegex();

	/// <summary>
	/// Extracts a label from the supplied text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@":([A-Za-z_]\w{1,31})\s*$", RegexOptions.Compiled)]
	private static partial Regex LabelRegex();

	/// <summary>
	/// Extracts a generic single-word selection from the supplied text
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"(\w+)", RegexOptions.Compiled)]
	private static partial Regex GenericWordRegex();

	#endregion

	#region Constructors

	/// <summary>
	/// Protected constructor so control of the token types returned can be maintained according to expected results - e.g. a Blank token should have no text associated with it, etc.
	/// </summary>
	/// <param name="tokenType">The type of token to be created</param>
	/// <param name="lineNumber">The line number of the source on which the token is located</param>
	/// <param name="lineSequence">The sequence within the line number of the source this token is located</param>
	/// <param name="text">The text (if any) associated with this token</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	protected YodaToken(TokenType tokenType, int lineNumber, int lineSequence, string? text)
	{
		if (tokenType.Equals(TokenType.Unknown))
			throw new ArgumentOutOfRangeException(nameof(tokenType), TokenType.Unknown,
				"Unknown is an invalid token type");
		ArgumentOutOfRangeException.ThrowIfLessThan(0, lineNumber, nameof(lineNumber));
		ArgumentOutOfRangeException.ThrowIfLessThan(0, lineSequence, nameof(lineSequence));

		TokenType = tokenType;
		LineNumber = lineNumber;
		LineSequence = lineSequence;
		Text = text?.Trim();
	}

	/// <summary>
	/// Static constructor to create a "Blank" token at the specified position in the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <returns>The token equivalent of a blank entry</returns>
	public static YodaToken Blank(int lineNumber, int lineSequence) =>
		new YodaToken(TokenType.Blank, lineNumber, lineSequence, null);

	/// <summary>
	/// Common static constructor to create the required <paramref name="tokenType"/>, applying regex rules to extract the correct details
	/// </summary>
	/// <param name="tokenType">The type of token to create</param>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text for the token</param>
	/// <returns>The requested token type, if validation passed</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="ArgumentException"></exception>
	private static YodaToken Create(TokenType tokenType, int lineNumber, int lineSequence, string text)
	{
		var regex = tokenType switch
		{
			TokenType.Comment => CommentRegex(),
			TokenType.Directive => DirectiveRegex(),
			TokenType.Label => LabelRegex(),
			TokenType.Command => GenericWordRegex(),
			TokenType.Operand => GenericWordRegex(),
			TokenType.Literal => GenericWordRegex(),
			TokenType.Symbol => GenericWordRegex(),
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Unhandled value")
		};
		var m = regex.Match(text);
		return m.Success
			? new YodaToken(tokenType, lineNumber, lineSequence, m.Groups[1].Value)
			: throw new ArgumentException($"Invalid {tokenType}", nameof(text));
	}

	/// <summary>
	/// Static constructor to yield a "comment" token at the specified position
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the comment (MUST contain the semi-colon character, followed by the comment text)</param>
	/// <returns>The tokenised version of the comment</returns>
	public static YodaToken Comment(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Comment, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to yield a "directive" token at the specified location
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the directive (MUST be contained within square brackets - e.g. [help])</param>
	/// <returns>The tokenised version of the Directive</returns>
	public static YodaToken Directive(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Directive, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to yield a "label" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the label (MUST be prefixed with a colon, start with alpha or underscore and contain other alphanumeric characters, up to a maximum of 32 characters in total)</param>
	/// <returns>The tokenised version of the Label</returns>
	public static YodaToken Label(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Label, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "command" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the command</param>
	/// <returns>The tokenised version of the Command</returns>
	public static YodaToken Command(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Command, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create an "operand" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the operand</param>
	/// <returns>The tokenised version of the Operand</returns>
	public static YodaToken Operand(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Operand, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "literal" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the literal value</param>
	/// <returns>The tokenised version of the Literal</returns>
	public static YodaToken Literal(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Literal, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "symbol" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the symbol value</param>
	/// <returns>The tokenised version of the Symbol</returns>
	public static YodaToken Symbol(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Symbol, lineNumber, lineSequence, text);

	#endregion

	#region Overrides

	/// <summary>
	/// Override the ToString to emit the sanitised version of the token
	/// </summary>
	/// <returns>The textual representation of the token</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public override string ToString()
	{
		return (TokenType switch
		{
			TokenType.Blank => string.Empty,
			TokenType.Comment => $"; {Text}",
			TokenType.Directive => $"[{Text}]",
			TokenType.Label => $":{Text}",
			TokenType.Command => Text,
			TokenType.Operand => Text,
			TokenType.Literal => Text,
			TokenType.Symbol => Text,
			_ => throw new ArgumentOutOfRangeException(nameof(TokenType), this.TokenType, "Unhandled value")
		})!;
	}

	#endregion
}