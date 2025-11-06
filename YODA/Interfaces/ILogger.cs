using SimpleInstructionMachine.Enums;

namespace SimpleInstructionMachine.Interfaces;

/// <summary>
/// Extremely simplified logger, borrows a lot from the ILogger in Microsoft.Extensions.Logging nuget packages
/// </summary>
public interface ILogger
{
	/// <summary>
	/// Writes a message to the log
	/// </summary>
	/// <param name="level">The level of log message</param>
	/// <param name="message">The message to be logged</param>
	void Log(LogLevel level, string message);
	
	/// <summary>
	/// Determines whether a specific level of logging is enabled
	/// </summary>
	/// <param name="level">The log level being tested</param>
	/// <returns>True if the level is at or above the minimum level; if lower, false is returned</returns>
	bool IsEnabled(LogLevel level);
}