using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine;

public abstract class AbstractVirtualMachine(bool debug)
	: IVirtualMachine
{
	#region IVirtualMachine Members

	public abstract Task Run(string folderPath);

	#endregion

	#region Fields

	protected bool Debug { get; private set; } = debug;

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

	#endregion
}