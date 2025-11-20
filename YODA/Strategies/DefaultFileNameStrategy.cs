using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Strategies;

public class DefaultFileNameStrategy
	: IFileNameStrategy
{
	#region IFileNameStrategy Members

	/// <inheritdoc/>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public string GetFileName(byte fileNumber)
	{
		return fileNumber switch
		{
			< 8 => $"{fileNumber}",
			< 16 => $"{fileNumber}.txt",
			_ => throw new ArgumentOutOfRangeException(nameof(fileNumber), $"Unknown {nameof(fileNumber)}: '{fileNumber}'. Binary files are between 0 and 7. Text files are between 8 and 15")
		};
	}

	/// <inheritdoc/>
	public string BootFileName => "boot";

	/// <inheritdoc/>
	public string BinaryCrashDumpFileName => "crash_dump";

	/// <inheritdoc/>
	public string TextCrashDumpFileName => Path.ChangeExtension(BinaryCrashDumpFileName, "txt");

	#endregion
}