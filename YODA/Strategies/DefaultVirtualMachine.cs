using System.Text;
using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;
using SimpleInstructionMachine.Logging;

namespace SimpleInstructionMachine.Strategies;

public class DefaultVirtualMachine
	: IVirtualMachine
{
	#region Fields

	private readonly byte[] _memory;
	private readonly IFileSystemStrategy _fileSystemStrategy;
	private readonly ILogger _logger;
	private bool _isDebug;

	#endregion

	#region Properties

	public int MemorySize { get; private set; }
	
	#endregion

	#region Constructors

	/// <summary>
	/// Default constructor gives a simple machine of 256 bytes of memory and a <see cref="DefaultFileSystemStrategy"/> implementation of the file system
	/// </summary>
	public DefaultVirtualMachine()
		: this(true, byte.MaxValue + 1, new DefaultFileSystemStrategy(), new ConsoleLogger(true))
	{
	}

	/// <summary>
	/// Alternate constructor allowing specific size of memory and the <see cref="IFileSystemStrategy"/> implementation to use for the underlying file system
	/// </summary>
	/// <param name="isDebug">Determines whether the machine is running in "debug" mode</param>
	/// <param name="memorySize">The size of the machine's memory area</param>
	/// <param name="fileSystemStrategy">The class implementing <see cref="IFileSystemStrategy"/> for the machine</param>
	/// <param name="logger">The logging class to use for output</param>
	public DefaultVirtualMachine(bool isDebug, int memorySize, IFileSystemStrategy fileSystemStrategy, ILogger logger)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(memorySize, 0);
		ArgumentNullException.ThrowIfNull(fileSystemStrategy);
		ArgumentNullException.ThrowIfNull(logger);

		//	Setup and initialise the machine memory
		_memory = new byte[memorySize];
		_memory.Initialize();
		_isDebug = isDebug;
		_fileSystemStrategy = fileSystemStrategy;
		_logger = logger;
		MemorySize = memorySize;
	}

	#endregion

	#region IVirtualMachine Members

	/// <inheritdoc/>
	public void Boot()
	{
		//	Load the file from the filesystem and bootstrap using the bytes read from the file
		Boot(_fileSystemStrategy.LoadBootFile());
	}

	/// <inheritdoc/>
	public void Boot(byte[] program)
	{
		//	Ensure it is of correct length
		ArgumentOutOfRangeException.ThrowIfZero(program.Length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(program.Length, _memory.Length);
		//	Copy to the system memory
		program.CopyTo(_memory, 0);
	}

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

		if (address == KnownMemory.ControlFlags)
			RefreshScreen(value);
	}

	public byte[] Memory => _memory.AsReadOnly().ToArray();

	private const string LcdDisplayOuter = "---------------------";

	/// <inheritdoc/>
	public void RefreshScreen(byte refreshFlag)
	{
		//	Mask the control flag and refresh flag values for bit 0
		var cf = _memory[KnownMemory.ControlFlags] & 1;
		var rf = refreshFlag & 1;
		
		//	If not refreshing or the toggling doesn't match, do not refresh
		if (rf == 0 || (cf ^ rf) != rf)
			return;
	
		var sb = new StringBuilder()
			.AppendLine(LcdDisplayOuter)
			.Append($"| {ToChar(_memory[KnownMemory.LCD_0])} | {ToChar(_memory[KnownMemory.LCD_1])} ")
			.Append($"| {ToChar(_memory[KnownMemory.LCD_2])} | {ToChar(_memory[KnownMemory.LCD_3])} ")
			.AppendLine($"| {ToChar(_memory[KnownMemory.LCD_4])} |")
			.AppendLine(LcdDisplayOuter);
		_logger.Log(LogLevel.Screen, sb.ToString());
		return;

		char ToChar(byte value)
		{
			return value switch
			{
				>= 32 => (char)value,
				_ => '?'
			};
		}
	}

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
		ArgumentOutOfRangeException.ThrowIfGreaterThan(address, _memory.Length);
	}

	#endregion
}