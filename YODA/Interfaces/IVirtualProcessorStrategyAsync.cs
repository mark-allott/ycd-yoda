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
	/// Loads a pre-compiled binary program from the specified <paramref name="filename"/>
	/// </summary>
	/// <param name="filename">The name of the file to load into system memory</param>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task LoadProgram(string filename, CancellationToken token);

	/// <summary>
	/// Loads a pre-compiled binary program from the specified byte array
	/// </summary>
	/// <param name="program">The pre-compiled binary program</param>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task LoadProgram(byte[] program, CancellationToken token);

	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <param name="token">The cancellation token used to signal state to the processor</param>
	/// <returns></returns>
	Task Run(CancellationToken token);
}