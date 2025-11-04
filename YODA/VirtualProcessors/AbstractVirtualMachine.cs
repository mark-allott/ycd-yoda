using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.VirtualProcessors;

public abstract class AbstractVirtualMachine(bool isDebug)
	: IVirtualMachine
{
	#region IVirtualMachine Members

	public abstract Task Run(string folderPath);

	#endregion

	#region Fields

	protected bool IsDebug { get; private set; } = isDebug;
	protected string Folder = ".";

	#endregion

	#region Common Methods

	protected void ConsoleMessage(string message)
	{
		Console.WriteLine(message);
	}

	protected void DebugMessage(string message)
	{
		if (!IsDebug)
			return;
		ConsoleMessage(message);
	}

	protected void ErrorMessage(string message)
	{
		Console.Error.WriteLineAsync(message);
	}

	protected async Task ErrorMessageAsync(string message)
	{
		await Console.Error.WriteLineAsync(message);
	}

	protected string FilenameFromFileNumber(byte fileNumber)
	{
		return fileNumber switch
		{
			< 8 => Path.Combine(Folder, $"{fileNumber}"),
			< 16 => Path.Combine(Folder, $"{fileNumber}.txt"),
			_ => throw new Exception(
				$"Unknown file {fileNumber}.  Binary files are between 0 and 7.   Text files are between 8 and 15")
		};
	}

	#endregion
}