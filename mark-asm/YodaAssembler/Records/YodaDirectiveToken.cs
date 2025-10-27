using YodaAssembler.Enums;
using YodaAssembler.Extensions;

namespace YodaAssembler.Records;

public record YodaDirectiveToken
	: YodaToken
{
	public DirectiveType DirectiveType { get; private init; }

	protected YodaDirectiveToken(int lineNumber, int lineSequence, string? text)
		: base(TokenType.Directive, lineNumber, lineSequence, text)
	{
		DirectiveType = text?.GetDirectiveType() ?? DirectiveType.Unknown;
	}

	public static YodaDirectiveToken Create(int lineNumber, int lineSequence, string? text)
		=> new(lineNumber, lineSequence, text);

	public override string ToString()
	{
		return $"[{Text}]";
	}
}