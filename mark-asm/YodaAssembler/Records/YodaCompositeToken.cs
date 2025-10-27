using YodaAssembler.Enums;

namespace YodaAssembler.Records;

public record YodaCompositeToken
	: YodaToken
{
	protected YodaToken ChildToken { get; init; }

	private YodaCompositeToken(TokenType tokenType, int lineNumber, int lineSequence, string? text, YodaToken childToken) 
		: base(tokenType, lineNumber, lineSequence, text)
	{
		ChildToken = childToken;
	}

	public static YodaCompositeToken CreateDirectNumber(int lineNumber, int lineSequence, string? text,
		YodaNumericValueToken childToken)
	{
		return new YodaCompositeToken(TokenType.DirectNumber, lineNumber, lineSequence, text, childToken);
	}

	public static YodaCompositeToken CreateIndirectNumber(int lineNumber, int lineSequence, string? text,
		YodaNumericValueToken childToken)
	{
		var directNumber = YodaCompositeToken.CreateDirectNumber(lineNumber, lineSequence, text, childToken);
		return new YodaCompositeToken(TokenType.IndirectNumber, lineNumber, lineSequence, text, directNumber);
	}
	
	public override string ToString()
	{
		return $"[{ChildToken}]";
	}
}