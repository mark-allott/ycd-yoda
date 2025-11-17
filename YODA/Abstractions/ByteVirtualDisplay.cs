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
	/// Holds the current state of the refresh flag
	/// </summary>
	private byte _currentState;

	#endregion

	#region Constructor

	/// <summary>
	/// Standard constructor, allows setting of the <paramref name="logger"/>, the <paramref name="controlFlagAddress"/>
	/// trigger location and an <paramref name="initialState"/> for the flag
	/// </summary>
	/// <param name="logger">A logger which will accept the screen output</param>
	/// <param name="controlFlagAddress">The memory address which would trigger a refresh</param>
	/// <param name="initialState">The initial state of the control flag</param>
	/// <exception cref="ArgumentNullException"></exception>
	public ByteVirtualDisplay(ILogger logger, int controlFlagAddress, byte initialState = 0)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		ControlFlagAddress = controlFlagAddress;
		_currentState = initialState;
	}

	#endregion

	#region IVirtualDisplay<byte> Members

	/// <inheritdoc/>
	public int ControlFlagAddress { get; }

	/// <inheritdoc/>
	public void Refresh(byte controlFlags, byte[] displayBuffer)
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
		var outer = new string('-', 1 + 4 * displayBuffer.Length);
		//	Top line
		var sb = new StringBuilder()
			.AppendLine(outer);
		//	Each segment
		foreach (var b in displayBuffer)
			sb.Append($"| {ToChar(b)} ");
		//	Finish display of segments and bottom line
		sb.AppendLine("|")
			.AppendLine(outer);

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