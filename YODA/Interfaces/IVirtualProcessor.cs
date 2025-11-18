namespace SimpleInstructionMachine.Interfaces;

public interface IVirtualProcessor
{
	/// <summary>
	/// Provides the current memory address being executed 
	/// </summary>
	int InstructionPointer { get; }

	/// <summary>
	/// Provides the address for the base of the stack
	/// </summary>
	int StackPointer { get; }

	/// <summary>
	/// Shows whether interrupts are enabled
	/// </summary>
	bool InterruptFlag { get; }

	/// <summary>
	/// Shows whether the processor is in "debug" mode
	/// </summary>
	bool Debugging { get; }
}