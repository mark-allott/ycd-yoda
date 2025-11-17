using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Abstractions;

public class ByteMemoryAccess
	: IMemoryAccess<byte>
{
	#region Fields

	/// <summary>
	/// The memory storage area
	/// </summary>
	private readonly byte[] _memory;

	/// <summary>
	/// A link to a virtual display
	/// </summary>
	private readonly IVirtualDisplay<byte> _virtualDisplay;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for the memory access class - determines total size of 
	/// </summary>
	/// <param name="memorySize"></param>
	/// <param name="virtualDisplay"></param>
	public ByteMemoryAccess(int memorySize, IVirtualDisplay<byte> virtualDisplay)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(memorySize, 0);
		ArgumentNullException.ThrowIfNull(virtualDisplay);
		_memory = new byte[memorySize];
		MemorySize = memorySize;
		_virtualDisplay = virtualDisplay;
	}

	#endregion

	#region IMemoryAccess<byte> Members

	/// <inheritdoc/>
	public byte ReadFromMemory(int address)
	{
		CheckAddress(address);
		return _memory[address];
	}

	/// <inheritdoc/>
	public void WriteToMemory(int address, byte value)
	{
		CheckAddress(address);
		_memory[address] = value;

		//	Check if the control flag for the virtual display is updated and call if required 
		if (address == _virtualDisplay.ControlFlagAddress)
			_virtualDisplay.Refresh(value);
	}

	/// <inheritdoc/>
	public byte[] Memory => _memory.AsReadOnly().ToArray();

	/// <inheritdoc/>
	public int MemorySize { get; private set; }

	/// <inheritdoc/>
	public byte this[byte index] => ReadFromMemory(index);

	#endregion

	#region Methods

	/// <summary>
	/// Ensures the <paramref name="address"/> is in the accepted range for the machine
	/// </summary>
	/// <param name="address">The memory address to be validated</param>
	private void CheckAddress(int address)
	{
		//	No negative addresses allowed
		ArgumentOutOfRangeException.ThrowIfLessThan(address, 0);
		//	Address must be within range of memory available
		ArgumentOutOfRangeException.ThrowIfGreaterThan(address, MemorySize);
	}

	#endregion
}