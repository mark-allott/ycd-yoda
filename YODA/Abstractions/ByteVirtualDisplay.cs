using System.Text;
using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Abstractions;

public class ByteVirtualDisplay
	: IVirtualDisplay<byte>
{
	#region Fields

	/// <summary>
	/// Display is really directed to a logger
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// The class responsible for handling memory access for the machine
	/// </summary>
	private readonly IMemoryAccess<byte> _memory;

	/// <summary>
	/// Holds the current state of the refresh flag
	/// </summary>
	private byte _currentState;

	/// <summary>
	/// Used to "wrap" the LCD output for the logger
	/// </summary>
	private const string LcdDisplayOuter = "---------------------";

	#endregion

	#region Constructor

	/// <summary>
	/// Simple constructor, requires only the <paramref name="logger"/> implementation
	/// </summary>
	/// <param name="logger">A logger which will accept the screen output</param>
	/// <param name="memoryAccess">A class handling the memory state for the machine</param>
	public ByteVirtualDisplay(ILogger logger, IMemoryAccess<byte> memoryAccess)
		: this(logger, memoryAccess, KnownMemory.ControlFlags, 0)
	{
	}

	/// <summary>
	/// Standard constructor, allows setting of the <paramref name="logger"/>, the <paramref name="controlFlagAddress"/>
	/// trigger location and an <paramref name="initialState"/> for the flag
	/// </summary>
	/// <param name="logger">A logger which will accept the screen output</param>
	/// <param name="memoryAccess">A class handling the memory state for the machine</param>
	/// <param name="controlFlagAddress">The memory address which would trigger a refresh</param>
	/// <param name="initialState">The initial state of the control flag</param>
	/// <exception cref="ArgumentNullException"></exception>
	public ByteVirtualDisplay(ILogger logger, IMemoryAccess<byte> memoryAccess, byte controlFlagAddress,
		byte initialState = 0)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_memory = memoryAccess ?? throw new ArgumentNullException(nameof(memoryAccess));
		ControlFlagAddress = controlFlagAddress;
		_currentState = initialState;
	}

	#endregion

	#region IVirtualDisplay<byte> Members

	public int ControlFlagAddress { get; }

	public void Refresh(byte controlFlags)
	{
		//	Mask the control flag and refresh flag values for bit 0
		var cf = (byte)(_currentState & 1);
		var rf = (byte)(controlFlags & 1);

		//	If the state changes, update the current state
		if (cf != rf)
			_currentState = rf;

		//	If not refreshing or the toggling doesn't match, do not refresh
		if (rf == 0 || (cf ^ rf) != rf)
			return;

		//	Refreshing, so build the display output
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
}