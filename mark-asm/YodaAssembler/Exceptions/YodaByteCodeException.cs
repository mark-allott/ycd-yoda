using System.Text;

namespace YodaAssembler.Exceptions;

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
		if (lineNumber == -1)
			return message;

		var sb = new StringBuilder(message);
		sb.Append($" on line {lineNumber}");
		return sb.ToString().Trim();
	}
}