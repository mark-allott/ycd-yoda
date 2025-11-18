namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessorStrategyAsync
{
	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task RunAsync(CancellationToken token);
}