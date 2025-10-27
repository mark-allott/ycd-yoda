using System.Diagnostics.CodeAnalysis;

namespace YodaAssembler.Exceptions;

[ExcludeFromCodeCoverage]
public class YodaByteCodeException
	: Exception
{
	public int LineNumber { get; private set; }

	protected YodaByteCodeException(string? message)
		: base(message)
	{
	}

	protected YodaByteCodeException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}

	public YodaByteCodeException(int lineNumber, string? message)
		: this(MakeMessage(message, lineNumber))
	{
		LineNumber = lineNumber;
	}

	public YodaByteCodeException(int lineNumber, string? message, Exception? innerException)
		: this(MakeMessage(message, lineNumber), innerException)
	{
		LineNumber = lineNumber;
	}

	private static string? MakeMessage(string? message, int lineNumber = -1)
	{
		return lineNumber == -1
			? message
			: $"{message} on line {lineNumber}".Trim();
	}
}