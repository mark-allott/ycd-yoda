using System.ComponentModel;

namespace SimpleInstructionMachine.Enums;

public enum LogLevel
{
	[Description("Unkn")]
	Unknown,
	[Description("Dbug")]
	Debug,
	Info,
	[Description("Warn")]
	Warning,
	[Description("Err ")]
	Error,
	[Description("Crit")]
	Critical,
	[Description("Scrn")]
	Screen,
	None
}