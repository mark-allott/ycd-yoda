using YodaAssembler.Records;

namespace YodaAssembler.Processor;

public static class YodaCommandSet
{
	public static readonly List<YodaCommand> Commands =
	[
		new YodaCommand(0x00, "halt"),
		new YodaCommand(0x01, "wait"),
		new YodaCommand(0x02, "return"),
		new YodaCommand(0x02, "ret"),
		new YodaCommand(0x03, "noop"),
		new YodaCommand(0x03, "nop"),
		new YodaCommand(0x04, "sif"),
		new YodaCommand(0x05, "cif"),
		new YodaCommand(0x10, "savetofile", 3),
		new YodaCommand(0x10, "stf", 3),
		new YodaCommand(0x20, "loadfromfile", 2),
		new YodaCommand(0x20, "lff", 2),
		new YodaCommand(0x30, "write", 2),
		new YodaCommand(0x40, "add", 3),
		new YodaCommand(0x50, "sub", 3),
		new YodaCommand(0x60, "inc", 1),
		new YodaCommand(0x70, "dec", 1),
		new YodaCommand(0x80, "jumpifzero", 2),
		new YodaCommand(0x80, "jz", 2),
		new YodaCommand(0x90, "jumpwithreturn", 1),
		new YodaCommand(0x90, "jwr", 1),
	];
}