namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualMachine
{
	/// <summary>
	/// Loads a bootfile from <paramref name="folderPath"/> and calls <see cref="Execute"/> to run it
	/// </summary>
	/// <param name="folderPath">The name of the folder to load the bootfile from</param>
	/// <returns></returns>
	Task Run(string folderPath);
	
	/// <summary>
	/// Loads the bootfile with the contents of <paramref name="bootData"/>, then calls <see cref="Execute"/>
	/// </summary>
	/// <param name="bootData">The boot file to be executed</param>
	/// <returns></returns>
	Task Run(byte[] bootData);
	
	/// <summary>
	/// Performs execution of the bytecode loaded into the system memory
	/// </summary>
	/// <returns></returns>
	/// <remarks>Execution starts at location 0 in bytecode memory and continues until an error occurs, or the program terminates normally</remarks>
	Task Execute();
}