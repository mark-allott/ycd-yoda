using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Logging;

public class ConsoleLogger
	: ILogger
{
	#region Constructor

	public ConsoleLogger(bool isDebug)
	{
		LogLevel = isDebug
			? LogLevel.Debug
			: LogLevel.Error;
	}

	#endregion

	#region Properties

	private LogLevel LogLevel { get; }

	#endregion

	#region ILogger implementation

	/// <inheritdoc />
	public void Log(LogLevel level, string message)
	{
		if (!IsEnabled(level))
			return;

		var consoleOutput = MessageFormatter.FormatMessage(level, message);
		if (level is LogLevel.Error or LogLevel.Critical)
			Console.Error.WriteLine(consoleOutput);
		else
			Console.WriteLine(consoleOutput);
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel level)
	{
		return level >= LogLevel;
	}

	#endregion
}