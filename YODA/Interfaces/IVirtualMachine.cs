namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualMachine
{
	Task Run(string folderPath);
}