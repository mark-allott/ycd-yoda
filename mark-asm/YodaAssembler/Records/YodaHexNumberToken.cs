using System.Globalization;

namespace YodaAssembler.Records;

public record YodaHexNumberToken
	: YodaNumericValueToken
{
	private YodaHexNumberToken(int lineNumber, int lineSequence, string text)
		: base(lineNumber, lineSequence, text)
	{
		NumericValue = int.Parse(text, NumberStyles.HexNumber);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(NumericValue, 255, nameof(text));
	}

	public static YodaHexNumberToken Create(int lineNumber, int lineSequence, string text)
	{
		//	Using NumberStyles.HexNumber for parsing does NOT permit use of the 0x prefix, so remove it before it is passed to the constructor
		return new YodaHexNumberToken(lineNumber, lineSequence,
			text.Replace("0x", "", StringComparison.InvariantCultureIgnoreCase));
	}

	public override string ToString()
	{
		return $"0x{NumericValue:x2}";
	}
}