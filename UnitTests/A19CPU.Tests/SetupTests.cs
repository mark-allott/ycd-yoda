using A19CPU.Tests.Records;
using FluentAssertions;

namespace A19CPU.Tests;

public class SetupTests
{
	[Fact]
	public void ValidateCpuStateOnConstruction()
	{
		var sut = new TestableA19Cpu(true);
		sut.Should().NotBeNull();

		var expectedState = new CpuState() { IsDebugging = true };
		var actualState = new CpuState(sut);
		actualState.Should().BeEquivalentTo(expectedState);
	}

	[Fact]
	public async Task RunWithNoCodeThrowsException()
	{
		var sut = new TestableA19Cpu(false);
		sut.Should().NotBeNull();

		byte[] bytecode = null!;
		await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.Run(bytecode));
	}

	[Fact]
	public async Task RunWithOversizedCodeThrowsException()
	{
		var sut = new TestableA19Cpu(false);
		sut.Should().NotBeNull();

		//	Make the block 1 byte too big
		var bytecode = new byte[byte.MaxValue + 2];
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.Run(bytecode));
	}

	[Theory]
	[InlineData(1)]
	[InlineData(32)]
	[InlineData(128)]
	[InlineData(byte.MaxValue + 1)]
	public async Task RunWithBlankByteCodeExitsCorrectly(int byteCodeSize)
	{
		var sut = new TestableA19Cpu(false);
		sut.Should().NotBeNull();

		//	Assign bytecode block of correct max size and initialise (sets all values to "halt" command equivalent)
		var bytecode = new byte[byteCodeSize];
		bytecode.Initialize();

		var expectedState = new CpuState();
		
		//	Run the "code"
		await sut.Run(bytecode);
		var actualState = new CpuState(sut);
		actualState.Should().BeEquivalentTo(expectedState);
	}
}