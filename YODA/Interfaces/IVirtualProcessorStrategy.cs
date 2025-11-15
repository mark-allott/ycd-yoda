namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessorStrategy
{
	/// <summary>
	/// Configures the virtual CPU prior to running
	/// </summary>
	/// <param name="folderPath"></param>
	/// <returns></returns>
	void Configure(string folderPath);
	
	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <returns></returns>
	void Run();
}