using YodaAssembler.Enums;

namespace YodaAssembler.Records;

public record YodaNumericValueToken
	: YodaToken
{
	public int NumericValue { get; protected init; }

	protected YodaNumericValueToken(int lineNumber, int lineSequence, string? text)
		: base(TokenType.LiteralNumber, lineNumber, lineSequence, text)
	{
	}
}