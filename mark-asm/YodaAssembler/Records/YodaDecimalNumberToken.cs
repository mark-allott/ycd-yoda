using System.Globalization;

namespace YodaAssembler.Records;

public record YodaDecimalNumberToken
	: YodaNumericValueToken
{
	private YodaDecimalNumberToken(int lineNumber, int lineSequence, string text)
		: base(lineNumber, lineSequence, text)
	{
		NumericValue = int.Parse(text, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(NumericValue, 255, nameof(text));
	}

	public static YodaDecimalNumberToken Create(int lineNumber, int lineSequence, string text)
	{
		//	Assumes no dodgy values are passed in odd number bases
		return new YodaDecimalNumberToken(lineNumber, lineSequence, text);
	}

	public override string ToString()
	{
		return $"{NumericValue}";
	}
}