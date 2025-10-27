using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Processor;

public static class YodaCommandSet
{
	public const ParameterTypes ImmediateOrDirect = ParameterTypes.LiteralNumber | ParameterTypes.Symbol |
	                                                ParameterTypes.DirectNumber | ParameterTypes.DirectSymbol;

	public const ParameterTypes ImmediateDirectOrChar = ImmediateOrDirect | ParameterTypes.LiteralChar;

	public static readonly List<YodaCommand> Commands =
	[
		new(0x00, "halt"),
		new(0x01, "wait"),
		new(0x02, "return"),
		new(0x02, "ret"),
		new(0x03, "noop"),
		new(0x03, "nop"),
		new(0x04, "sif"),
		new(0x05, "cif"),
		new(0x10, "savetofile", 3, ImmediateOrDirect, ImmediateOrDirect, ImmediateOrDirect),
		new(0x10, "stf", 3, ImmediateOrDirect, ImmediateOrDirect, ImmediateOrDirect),
		new(0x20, "loadfromfile", 2, ImmediateOrDirect, ImmediateOrDirect),
		new(0x20, "lff", 2, ImmediateOrDirect, ImmediateOrDirect),
		new(0x30, "write", 2, ImmediateOrDirect, ImmediateDirectOrChar),
		new(0x40, "add", 3, ImmediateDirectOrChar, ImmediateDirectOrChar, ImmediateOrDirect),
		new(0x50, "sub", 3, ImmediateDirectOrChar, ImmediateDirectOrChar, ImmediateOrDirect),
		new(0x60, "inc", 1, ImmediateOrDirect),
		new(0x70, "dec", 1, ImmediateOrDirect),
		new(0x80, "jumpifzero", 2, ImmediateOrDirect, ImmediateOrDirect),
		new(0x80, "jz", 2, ImmediateOrDirect, ImmediateOrDirect),
		new(0x90, "jumpwithreturn", 1, ImmediateOrDirect),
		new(0x90, "jwr", 1, ImmediateOrDirect),
	];
}