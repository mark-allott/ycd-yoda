namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessorStrategyAsync
{
	/// <summary>
	/// Configures the virtual CPU prior to running
	/// </summary>
	/// <param name="folderPath"></param>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task Configure(string folderPath, CancellationToken token);

	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task Run(CancellationToken token);
}