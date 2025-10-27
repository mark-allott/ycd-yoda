using System.Text;
using YodaAssembler.Enums;

namespace YodaAssembler.Exceptions;

public class TokeniserException
	: Exception
{
	public int LineNumber { get; }
	public string? SourceText { get; }

	protected TokeniserException(string? message)
		: base(message)
	{
	}

	protected TokeniserException(string? message, Exception? inner)
		: base(message, inner)
	{
	}

	public TokeniserException(int lineNumber, string? sourceText)
		: base(MakeMessage(null, lineNumber, sourceText))
	{
		LineNumber = lineNumber;
		SourceText = sourceText;
	}

	public TokeniserException(int lineNumber, string? sourceText, string? message)
		: base(MakeMessage(message, lineNumber, sourceText))
	{
		LineNumber = lineNumber;
		SourceText = sourceText;
	}

	public TokeniserException(int lineNumber, string? sourceText, string? message, Exception? innerException)
		: base(MakeMessage(message, lineNumber, sourceText), innerException)
	{
		LineNumber = lineNumber;
		SourceText = sourceText;
	}

	/// <summary>
	/// Converts the optional parts of <paramref name="message"/>, <paramref name="lineNumber"/> and <paramref name="sourceText"/> into a combined message
	/// </summary>
	/// <param name="message">An optional message to be used in the base value for the entire message</param>
	/// <param name="lineNumber">An optional line number - if present will append "... on line xxx" to the message</param>
	/// <param name="sourceText">An optional value for the source text - if present shall append "... with source text 'text'" to the message</param>
	/// <returns></returns>
	private static string? MakeMessage(string? message, int lineNumber = -1, string? sourceText = null)
	{
		if (lineNumber == -1 && sourceText is null)
			return message;
		var sb = new StringBuilder(message);
		if (lineNumber != -1)
			sb.Append($" on line {lineNumber}");
		if (!string.IsNullOrWhiteSpace(sourceText))
			sb.Append($" with source text: '{sourceText}'");

		return sb.ToString().Trim();
	}
}

public class DirectiveException
	: TokeniserException
{
	public DirectiveType Directive { get; }

	public DirectiveException(DirectiveType directive, string? message)
		: base(message)
	{
		Directive = directive;
	}

	public DirectiveException(DirectiveType directive, string? message, Exception? innerException)
		: base(message, innerException)
	{
		Directive = directive;
	}

	public DirectiveException(DirectiveType directive, int lineNumber, string? sourceText, string? message)
		: base(lineNumber, sourceText, message)
	{
		Directive = directive;
	}

	public DirectiveException(DirectiveType directive, int lineNumber, string? sourceText, string? message,
		Exception? innerException)
		: base(lineNumber, sourceText, message, innerException)
	{
		Directive = directive;
	}
}