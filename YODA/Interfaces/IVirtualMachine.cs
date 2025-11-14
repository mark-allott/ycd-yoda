namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualMachine
{
	/// <summary>
	/// Performs the required actions to bootstrap the machine
	/// </summary>
	void Boot();

	/// <summary>
	/// Accesses the machine memory and returns the value at the specified address
	/// </summary>
	/// <param name="address">The memory address to read</param>
	/// <returns>The value at the specified <paramref name="address"/></returns>
	byte ReadFromMemory(int address);
	
	/// <summary>
	/// Accesses the machine memory and stores the <paramref name="value"/> at the specified <paramref name="address"/>
	/// </summary>
	/// <param name="address">The address to store the value in</param>
	/// <param name="value">The value to be stored</param>
	void WriteToMemory(int address, byte value);

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