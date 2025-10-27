using YodaAssembler.Enums;

namespace YodaAssembler.Records;

public record YodaCommand
{
	public byte OpCode { get; private init; }
	public string Mnemonic { get; private init; }
	public int ParameterCount { get; private init; }

	public ParameterTypes[] ParameterTypes { get; private init; }

	public YodaCommand(byte opCode, string mnemonic, int parameterCount = 0, params ParameterTypes[] parameterTypes)
	{
		//	Commands MUST have a name supplied
		ArgumentException.ThrowIfNullOrWhiteSpace(mnemonic);
		//	OpCount MUST be between 0 and 3, if not set to the "optional" value of -1
		if (parameterCount == -1)
		{
			//	If optional parameter numbers are being used, only one ParameterType definition is allowed
			ArgumentOutOfRangeException.ThrowIfNotEqual(parameterTypes.Length, 1, nameof(parameterTypes));
		}
		else
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(parameterCount, 0, nameof(parameterCount));
			ArgumentOutOfRangeException.ThrowIfGreaterThan(parameterCount, 3, nameof(parameterCount));
			ArgumentOutOfRangeException.ThrowIfNotEqual(parameterTypes.Length, parameterCount, nameof(parameterTypes));
		}

		OpCode = opCode;
		Mnemonic = mnemonic.Trim();
		ParameterCount = parameterCount;
		ParameterTypes = parameterTypes;
	}

	public bool IsMatch(string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       Mnemonic.Equals(text.Trim(), StringComparison.InvariantCultureIgnoreCase);
	}
}