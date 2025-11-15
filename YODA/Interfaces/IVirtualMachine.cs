namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualMachine
	: IMemoryAccess<byte>
{
	/// <summary>
	/// Performs the required actions to bootstrap the machine
	/// </summary>
	void Boot();

	/// <summary>
	/// Provides access to all the machine memory as an array
	/// </summary>
	/// <remarks>Implementors should consider making this a read-only property, with writing performed only by <see cref="WriteToMemory"/></remarks>
	byte[] MachineMemory { get; }

	/// <summary>
	/// Performs a "refresh" of the machine's screen, with output directed to the appropriate logging device
	/// </summary>
	/// <param name="refreshFlag">The value written to the control flag memory location</param>
	/// <remarks>
	/// The <paramref name="refreshFlag"/> value should be tested against the current value of the control flag memory
	/// location. Both values should be masked for bit 0, checked for a change and if the result is "set", then the
	/// screen should be refreshed.
	/// </remarks>
	void RefreshScreen(byte refreshFlag);
}