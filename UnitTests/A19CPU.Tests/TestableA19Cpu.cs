using SimpleInstructionMachine.VirtualProcessors;

namespace A19CPU.Tests;

/// <summary>
/// Create a subclass of the <see cref="A19"/> CPU that permits read-only access to the instruction pointer, stack
/// pointer, interrupt flag and debugging flag to allow unit tests to check the values 
/// </summary>
/// <param name="isDebug"></param>
public class TestableA19Cpu(bool isDebug)
	: A19(isDebug)
{
	public int IP => InstructionPointer;
	public int SP => StackPointer;
	public bool IF => base.InterruptsEnabled;
	public bool DF => IsDebug;
	public byte[] Bytes => ByteCode;
}