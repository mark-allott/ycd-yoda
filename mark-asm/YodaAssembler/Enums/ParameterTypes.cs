namespace YodaAssembler.Enums;

[Flags]
public enum ParameterTypes
{
	None = 0,
	LiteralNumber = 1 << 0,
	LiteralString = 1 << 1,
	LiteralChar = 1 << 2,
	Symbol = 1 << 3,
	DirectNumber = 1 << 4,
	DirectSymbol = 1 << 5,
	IndirectNumber = 1 << 6,
	IndirectSymbol = 1 << 7,
}