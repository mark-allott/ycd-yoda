using YodaAssembler.Enums;

namespace YodaAssembler.Records;

public record YodaCompositeToken
	: YodaToken
{
	protected YodaToken ChildToken { get; init; }

	private YodaCompositeToken(TokenType tokenType, int lineNumber, int lineSequence, string? text, YodaToken childToken, ParameterTypes parameterType) 
		: base(tokenType, lineNumber, lineSequence, text, parameterType)
	{
		ChildToken = childToken;
	}

	public static YodaCompositeToken CreateDirectNumber(int lineNumber, int lineSequence, string? text,
		YodaNumericValueToken childToken)
	{
		return new YodaCompositeToken(TokenType.DirectNumber, lineNumber, lineSequence, text, childToken, ParameterTypes.DirectNumber);
	}

	public static YodaCompositeToken CreateIndirectNumber(int lineNumber, int lineSequence, string? text,
		YodaNumericValueToken childToken)
	{
		var directNumber = CreateDirectNumber(lineNumber, lineSequence, text, childToken);
		return new YodaCompositeToken(TokenType.IndirectNumber, lineNumber, lineSequence, text, directNumber,
			ParameterTypes.IndirectNumber);
	}
	
	public override string ToString()
	{
		return $"[{ChildToken}]";
	}
}