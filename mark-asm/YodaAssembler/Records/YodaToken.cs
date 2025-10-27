using System.Text.RegularExpressions;
using YodaAssembler.Enums;
using YodaAssembler.Interfaces;

namespace YodaAssembler.Records;

public record YodaToken
	: IToken
{
	#region IToken implementation

	public TokenType TokenType { get; private init; }
	public int LineNumber { get; private init; }
	public int LineSequence { get; private init; }
	public string? Text { get; private init; }

	public ParameterTypes ParameterType { get; private init; }

	#endregion

	#region Constructors

	/// <summary>
	/// Protected constructor so control of the token types returned can be maintained according to expected results - e.g. a Blank token should have no text associated with it, etc.
	/// </summary>
	/// <param name="tokenType">The type of token to be created</param>
	/// <param name="lineNumber">The line number of the source on which the token is located</param>
	/// <param name="lineSequence">The sequence within the line number of the source this token is located</param>
	/// <param name="text">The text (if any) associated with this token</param>
	/// <param name="parameterType">The type of parameter this token is</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	protected YodaToken(TokenType tokenType, int lineNumber, int lineSequence, string? text, ParameterTypes parameterType = ParameterTypes.None)
	{
		if (tokenType.Equals(TokenType.Unknown))
			throw new ArgumentOutOfRangeException(nameof(tokenType), TokenType.Unknown,
				"Unknown is an invalid token type");
		ArgumentOutOfRangeException.ThrowIfLessThan(lineNumber, 0, nameof(lineNumber));
		ArgumentOutOfRangeException.ThrowIfLessThan(lineSequence, 0, nameof(lineSequence));

		TokenType = tokenType;
		LineNumber = lineNumber;
		LineSequence = lineSequence;
		Text = text?.Trim();
		ParameterType = parameterType;
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
	/// <param name="parameterType">The type of parameter this is</param>
	/// <returns>The requested token type, if validation passed</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="ArgumentException"></exception>
	private static YodaToken Create(TokenType tokenType, int lineNumber, int lineSequence, string text, ParameterTypes parameterType =  ParameterTypes.None)
	{
		ArgumentNullException.ThrowIfNull(text, nameof(text));

		var regex = tokenType switch
		{
			TokenType.Comment => TokenRegex.IsComment(),
			TokenType.Directive => TokenRegex.IsDirective(),
			TokenType.Label => TokenRegex.HasLabel(),
			TokenType.Command or TokenType.Operand => TokenRegex.GenericWord(),
			TokenType.LiteralString => TokenRegex.LiteralString(),
			TokenType.LiteralChar => TokenRegex.LiteralChar(),
			TokenType.LiteralNumber => TokenRegex.LiteralNumber(),
			TokenType.Symbol => TokenRegex.GenericWord(),
			TokenType.DirectNumber => TokenRegex.DirectNumber(),
			TokenType.DirectSymbol => TokenRegex.DirectSymbol(),
			TokenType.IndirectNumber => TokenRegex.IndirectNumber(),
			TokenType.IndirectSymbol => TokenRegex.IndirectSymbol(),
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Unhandled value")
		};
		var m = regex.Match(text);
		if (!m.Success)
			throw new ArgumentException($"Invalid {tokenType}", nameof(text));

		switch (tokenType)
		{
			case TokenType.LiteralNumber:
				return CreateNumericValueToken(m, lineNumber, lineSequence, tokenType);
			
			case TokenType.DirectNumber:
			case TokenType.IndirectNumber:
				var numericToken = CreateNumericValueToken(m, lineNumber, lineSequence, tokenType);
				return tokenType == TokenType.DirectNumber
					? YodaCompositeToken.CreateDirectNumber(lineNumber, lineSequence, text, numericToken)
					: YodaCompositeToken.CreateIndirectNumber(lineNumber, lineSequence, text, numericToken);

			default:
				return new YodaToken(tokenType, lineNumber, lineSequence, m.Groups[1].Value, parameterType);
		}
	}

	/// <summary>
	/// Returns a <see cref="YodaNumericValueToken"/> record based on the regex matches contained in <paramref name="m"/>
	/// </summary>
	/// <param name="m">The regex matches detected</param>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="tokenType">The type of token to create</param>
	/// <returns>A <see cref="YodaNumericValueToken"/> record of the appropriate type (hex, binary or decimal)</returns>
	/// <exception cref="ArgumentException"></exception>
	private static YodaNumericValueToken CreateNumericValueToken(Match m, int lineNumber, int lineSequence, TokenType tokenType)
	{
		if (m.Groups[2].Success) //	Should be a hex number
			return YodaHexNumberToken.Create(lineNumber, lineSequence, m.Groups[1].Value);
		if (m.Groups[3].Success) //	Should be a binary number in form 0b0000_1111
			return YodaBinaryNumberToken.Create(lineNumber, lineSequence, m.Groups[1].Value);
		if (m.Groups[5].Success) //	Should be a binary number in form 0b00001111
			return YodaBinaryNumberToken.Create(lineNumber, lineSequence, m.Groups[1].Value);
		if (m.Groups[6].Success) //	Should be a decimal number
			return YodaDecimalNumberToken.Create(lineNumber, lineSequence, m.Groups[1].Value);
		throw new ArgumentException($"Invalid match detected for {tokenType}");
	}
	
	/// <summary>
	/// Static constructor to yield a "comment" token at the specified position
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the comment (MUST contain the semicolon character, followed by the comment text)</param>
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
	/// Static constructor to create a "literal" token from the source to represent a string
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the literal value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.LiteralString"/></returns>
	public static YodaToken LiteralString(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.LiteralString, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "literal" token from the source to represent a character
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the literal value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.LiteralChar"/></returns>
	public static YodaToken LiteralChar(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.LiteralChar, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "literal" token from the source to represent a number value
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the literal value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.LiteralNumber"/></returns>
	public static YodaToken LiteralNumber(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.LiteralNumber, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "symbol" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the symbol value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.Symbol"/></returns>
	public static YodaToken Symbol(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.Symbol, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "direct number" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the number for the direct value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.DirectNumber"/></returns>
	public static YodaToken DirectNumber(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.DirectNumber, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create a "direct symbol" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the number for the direct symbol</param>
	/// <returns>The tokenised version of the <see cref="TokenType.DirectSymbol"/></returns>
	public static YodaToken DirectSymbol(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.DirectSymbol, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create an "indirect number" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the number for the indirect value</param>
	/// <returns>The tokenised version of the <see cref="TokenType.IndirectNumber"/></returns>
	public static YodaToken IndirectNumber(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.IndirectNumber, lineNumber, lineSequence, text);

	/// <summary>
	/// Static constructor to create an "indirect symbol" token from the source
	/// </summary>
	/// <param name="lineNumber">The line on which the source appears</param>
	/// <param name="lineSequence">The position within the line</param>
	/// <param name="text">The text representing the number for the indirect symbol</param>
	/// <returns>The tokenised version of the <see cref="TokenType.IndirectSymbol"/></returns>
	public static YodaToken IndirectSymbol(int lineNumber, int lineSequence, string text) =>
		Create(TokenType.IndirectSymbol, lineNumber, lineSequence, text);

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
			TokenType.Directive or
				TokenType.DirectNumber or
				TokenType.DirectSymbol => $"[{Text}]",
			TokenType.Label => $":{Text}",
			TokenType.Command or
				TokenType.Operand or
				TokenType.LiteralNumber or
				TokenType.Symbol => Text,
			TokenType.LiteralString => $"\"{Text}\"",
			TokenType.LiteralChar => $"'{Text}'",
			TokenType.IndirectNumber or
				TokenType.IndirectSymbol => $"[[{Text}]]",
			_ => throw new ArgumentOutOfRangeException(nameof(TokenType), this.TokenType, "Unhandled value")
		})!;
	}

	#endregion
}