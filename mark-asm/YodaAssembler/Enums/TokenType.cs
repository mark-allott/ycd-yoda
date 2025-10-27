namespace YodaAssembler.Enums;

public enum TokenType
{
	Unknown = 0,
	Blank,
	Comment,
	Directive,
	Label,
	Command,
	Operand,
	LiteralString,
	LiteralChar,
	LiteralNumber,
	Symbol,
	DirectNumber,
	DirectSymbol,
	IndirectNumber,
	IndirectSymbol,
}