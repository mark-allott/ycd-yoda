using System.Globalization;

namespace YodaAssembler.Records;

public record YodaBinaryNumberToken
	: YodaNumericValueToken
{
	private YodaBinaryNumberToken(int lineNumber, int lineSequence, string text)
		: base(lineNumber, lineSequence, text)
	{
		//	Using NumberStyles.BinaryNumber for parsing does NOT permit use of the 0b prefix, so remove it
		NumericValue = int.Parse(text, NumberStyles.BinaryNumber);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(NumericValue, 255, nameof(text));
	}

	public static YodaBinaryNumberToken Create(int lineNumber, int lineSequence, string text)
	{
		return new YodaBinaryNumberToken(lineNumber, lineSequence,
			//	Using NumberStyles.BinaryNumber for parsing does NOT permit use of the 0b prefix, so remove it before it is passed to the constructor
			text.Replace("0b", "", StringComparison.InvariantCultureIgnoreCase) 
			//	Using NumberStyles.BinaryNumber for parsing does NOT permit use of the underscore either
				.Replace("_", ""));
	}

	public override string ToString()
	{
		return $"0b{NumericValue:b8}";
	}
}