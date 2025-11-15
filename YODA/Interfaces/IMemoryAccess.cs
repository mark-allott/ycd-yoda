namespace SimpleInstructionMachine.Interfaces;

public interface IMemoryAccess<T>
	where T : struct
{
	/// <summary>
	/// Accesses the machine memory and returns the value at the specified address
	/// </summary>
	/// <param name="address">The memory address to read</param>
	/// <returns>The value at the specified <paramref name="address"/></returns>
	T ReadFromMemory(int address);
	
	/// <summary>
	/// Accesses the machine memory and stores the <paramref name="value"/> at the specified <paramref name="address"/>
	/// </summary>
	/// <param name="address">The address to store the value in</param>
	/// <param name="value">The value to be stored</param>
	void WriteToMemory(int address, T value);
}