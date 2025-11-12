using A19CPU.Tests.Records;
using FluentAssertions;

namespace A19CPU.Tests;

public partial class CommandTests
{
	/// <summary>
	/// Supply a debuggable CPU for testing
	/// </summary>
	private TestableA19Cpu TestCpu => new TestableA19Cpu(true);

	#region Tests for simple commands

	[Theory]
	[ClassData(typeof(SimpleCommandTestDataProvider))]
	public void VerifySimpleCommand(byte[] programCode, CpuState expectedState)
	{
		Task.Run(async () =>
		{
			var sut = TestCpu;
			sut.Should().NotBeNull();

			await sut.Run(programCode);
			var actualState = new CpuState(sut);
			actualState.Should().BeEquivalentTo(expectedState);
		});
	}

	#endregion

	private async Task RunTest(MachineMemory memory, CpuState expectedState)
	{
		var sut = TestCpu;
		sut.Should().NotBeNull();

		await sut.Run(memory.Program);
		sut.Bytes.Should().ContainInConsecutiveOrder(memory.Expected);
		var actualState = new CpuState(sut);
		actualState.Should().BeEquivalentTo(expectedState);
	}

	[Theory]
	[ClassData(typeof(LoadCommandTestDataProvider))]
	public void VerifyLoadBehaviour(MachineMemory memory, CpuState expectedState)
	{
		Task.Run(async () => await RunTest(memory, expectedState));
	}

	[Theory]
	[ClassData(typeof(AddCommandTestDataProvider))]
	public void VerifyAddBehaviour(MachineMemory memory, CpuState expectedState)
	{
		Task.Run(async () => await RunTest(memory, expectedState));
	}

	[Theory]
	[ClassData(typeof(SubCommandTestDataProvider))]
	public void VerifySubBehaviour(MachineMemory memory, CpuState expectedState)
	{
		Task.Run(async () => await RunTest(memory, expectedState));
	}
	
	[Theory]
	[ClassData(typeof(PushCommandTestDataProvider))]
	public void VerifyPushBehaviour(MachineMemory bytes, CpuState expectedState)
	{
		Task.Run(async () => await RunTest(bytes, expectedState));
	}
}