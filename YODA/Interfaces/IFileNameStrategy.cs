namespace SimpleInstructionMachine.Interfaces;

public interface IFileNameStrategy
{
	/// <summary>
	/// Returns the string representation for the given file number
	/// </summary>
	/// <param name="fileNumber">The number of the file</param>
	/// <returns>The filename as a string, with optional file extension</returns>
	string GetFileName(byte fileNumber);
}