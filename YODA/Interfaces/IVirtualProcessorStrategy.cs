namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessorStrategy
{
	/// <summary>
	/// Executes the program loaded into system memory
	/// </summary>
	/// <returns></returns>
	void Run();
}