using YodaAssembler.Enums;

namespace YodaAssembler.Records;

public record YodaCommandToken
	: YodaToken
{
	#region Properties

	public YodaCommand YodaCommand { get; private init; }

	#endregion

	#region Constructors

	protected YodaCommandToken(int lineNumber, int lineSequence, string? text, YodaCommand command)
		: base(TokenType.Command, lineNumber, lineSequence, text)
	{
		ArgumentNullException.ThrowIfNull(command, nameof(command));
		YodaCommand = command;
	}

	public static YodaCommandToken Create(int lineNumber, int lineSequence, string? text, YodaCommand command) =>
		new YodaCommandToken(lineNumber, lineSequence, text, command);

	#endregion
}