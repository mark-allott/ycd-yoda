using System.Text;
using SimpleInstructionMachine.Enums;

namespace SimpleInstructionMachine.Logging;

public static class MessageFormatter
{
	/// <summary>
	/// Formats the incoming message, ensuring a prefix of the current <paramref name="level"/> is present on each line
	/// </summary>
	/// <param name="level">The <see cref="LogLevel"/> for the message</param>
	/// <param name="message">The message to log</param>
	/// <returns>The formatted message</returns>
	/// <remarks>
	/// If the message is multi-line, each line is prefixed with the level
	/// </remarks>
	public static string FormatMessage(LogLevel level, string message)
	{
		//	Work out the prefix for the message using the level of the message
		//	Result is teh LogLevel string, left-padded to achieve 8 characters in total
		var prefix = $"    {level}"[^8..];

		//	As the message may include multiple lines, split the message on newline boundaries and prefix each line
		var sb = new StringBuilder();
		foreach (var line in message.Split(Environment.NewLine))
			sb.AppendLine($"[{prefix}] {line}");
		return sb.ToString();
	}
}