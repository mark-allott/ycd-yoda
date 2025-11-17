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
		_memory.Initialize();
		MemorySize = memorySize;
		_virtualDisplay = virtualDisplay ?? throw new ArgumentNullException(nameof(virtualDisplay));
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
			_virtualDisplay.Refresh(value, _memory[KnownMemory.LCD_0..KnownMemory.LCD_4]);
	}

	/// <inheritdoc/>
	public void WriteToMemory(int address, byte[] values)
	{
		ArgumentNullException.ThrowIfNull(values);
		ArgumentOutOfRangeException.ThrowIfZero(values.Length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(address + values.Length, MemorySize);
		CheckAddress(address);
		values.CopyTo(_memory, address);

		//	Check if the control flag for the virtual display is updated and call if required
		var offset = _virtualDisplay.ControlFlagAddress - address;
		if (offset > 0 && offset < values.Length)
			_virtualDisplay.Refresh(_memory[_virtualDisplay.ControlFlagAddress], _memory[KnownMemory.LCD_0..KnownMemory.LCD_4]);
	}

	/// <inheritdoc/>
	public byte[] Memory => _memory.AsReadOnly().ToArray();

	/// <inheritdoc/>
	public int MemorySize { get; private set; }

	/// <inheritdoc/>
	public byte this[int index] => ReadFromMemory(index);

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