namespace SimpleInstructionMachine.Interfaces;

public interface IFileNameStrategy
{
	/// <summary>
	/// Returns the string representation for the given file number
	/// </summary>
	/// <param name="fileNumber">The number of the file</param>
	/// <returns>The filename as a string, with optional file extension</returns>
	string GetFileName(byte fileNumber);

	/// <summary>
	/// Provides the name of the boot file to be loaded
	/// </summary>
	string BootFileName { get; }

	/// <summary>
	/// Provides the name of the binary crash dump file
	/// </summary>
	string BinaryCrashDumpFileName { get; }

	/// <summary>
	/// Provides the name of the text-based crash dump file
	/// </summary>
	string TextCrashDumpFileName { get; }
}