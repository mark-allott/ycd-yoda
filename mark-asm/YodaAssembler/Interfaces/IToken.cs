using YodaAssembler.Enums;

namespace YodaAssembler.Interfaces;

public interface IToken
{
	/// <summary>
	///	The type of token this is
	/// </summary>
	TokenType TokenType { get; }
	
	/// <summary>
	/// Specifies the line number in the source code where this token is located
	/// </summary>
	int LineNumber { get; }
	
	/// <summary>
	/// Specifies the order in which the token appears on the line
	/// </summary>
	int LineSequence { get; }
	
	/// <summary>
	/// The text associated with the token
	/// </summary>
	string? Text { get; }
}