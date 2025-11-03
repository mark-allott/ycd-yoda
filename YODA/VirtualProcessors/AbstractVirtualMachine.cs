using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.VirtualProcessors;

public abstract class AbstractVirtualMachine(bool debug)
	: IVirtualMachine
{
	#region IVirtualMachine Members

	public abstract Task Run(string folderPath);

	#endregion

	#region Fields

	protected bool Debug { get; private set; } = debug;
	protected string Folder = ".";

	#endregion

	#region Common Methods

	public void ConsoleMessage(string message)
	{
		Console.WriteLine(message);
	}

	public void DebugMessage(string message)
	{
		if (!Debug)
			return;
		ConsoleMessage(message);
	}

	public async Task ErrorMessage(string message)
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