namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessorStrategy
{
	/// <summary>
	/// Configures the virtual CPU prior to running
	/// </summary>
	/// <param name="folderPath"></param>
	/// <returns></returns>
	Task Configure(string folderPath);
	
	/// <summary>
	/// Loads a pre-compiled binary program from the specified <paramref name="filename"/>
	/// </summary>
	/// <param name="filename">The name of the file to load into system memory</param>
	/// <returns></returns>
	Task LoadProgram(string filename);
	
	/// <summary>
	/// Loads a pre-compiled binary program from the specified byte array
	/// </summary>
	/// <param name="program">The pre-compiled binary program</param>
	/// <returns></returns>
	Task LoadProgram(byte[] program);
	
	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <returns></returns>
	Task Run();
}