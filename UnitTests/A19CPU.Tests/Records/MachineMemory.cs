namespace A19CPU.Tests.Records;

public record MachineMemory
{
	public byte[] Program { get; init; } = new byte[byte.MaxValue + 1];
	public byte[] Expected { get; init; } = new byte[byte.MaxValue + 1];

	public MachineMemory()
	{
		Program.Initialize();
		Expected.Initialize();
	}

	public void CopyProgramToExpected()
	{
		Program.CopyTo(Expected, 0);
	}

	public override string ToString()
	{
		return $"{nameof(MachineMemory)}";
	}
}