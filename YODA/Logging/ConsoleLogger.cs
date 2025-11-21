using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Logging;

public class ConsoleLogger
	: ILogger
{
	private readonly Func<LogLevel, string, string> _formatter;

	#region Constructor

	public ConsoleLogger(bool isDebug, Func<LogLevel, string, string> formatter)
	{
		LogLevel = isDebug
			? LogLevel.Debug
			: LogLevel.Error;
		_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
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

		var consoleOutput = _formatter(level, message);
		if (level is LogLevel.Error or LogLevel.Critical)
			Console.Error.Write(consoleOutput);
		else
			Console.Write(consoleOutput);
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel level)
	{
		return level >= LogLevel;
	}

	#endregion
}