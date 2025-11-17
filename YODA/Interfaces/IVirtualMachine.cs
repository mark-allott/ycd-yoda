namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualMachine<in T>
	where T : struct
{
	/// <summary>
	/// Performs the required actions to bootstrap the machine from the FileSystem bootfile
	/// </summary>
	void Boot();

	/// <summary>
	/// Bootstraps the system memory from the supplied program 
	/// </summary>
	/// <param name="program"></param>
	void Boot(T[] program);

	/// <summary>
	/// Runs the program the machine has been bootstrapped with
	/// </summary>
	void Run();

	/// <summary>
	/// Runs the program the machine has been bootstrapped with async support
	/// </summary>
	Task RunAsync(CancellationToken token);
}