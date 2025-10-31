using YodaAssembler.Enums;

namespace YodaAssembler.Interfaces;

public interface ITokenByteCode
{
	/// <summary>
	/// Hold the line number for the source of the tokens
	/// </summary>
	int LineNumber { get; }

	/// <summary>
	/// The start location for the bytecode
	/// </summary>
	int MemoryLocation { get; }

	/// <summary>
	/// The next memory location for the bytecode to occupy 
	/// </summary>
	int NextLocation { get; }

	/// <summary>
	/// The type of directive causing the bytecode to be generated
	/// </summary>
	DirectiveType DirectiveType { get; }

	/// <summary>
	/// A read-only collection of the tokens that make up the line of source code
	/// </summary>
	IReadOnlyList<IToken> Tokens { get; }

	/// <summary>
	/// The bytecode representation for the tokens
	/// </summary>
	byte?[] Bytes { get; }
}