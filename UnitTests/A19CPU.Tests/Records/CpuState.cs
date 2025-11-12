// ReSharper disable InconsistentNaming

using SimpleInstructionMachine.VirtualProcessors;

namespace A19CPU.Tests.Records;

public record CpuState
{
	#region registers

	/// <summary>
	/// The CPUs A register
	/// </summary>
	public byte A { get; init; }

	/// <summary>
	/// The CPUs B register
	/// </summary>
	public byte B { get; init; }

	/// <summary>
	/// The CPUs C register
	/// </summary>
	public byte C { get; init; }

	/// <summary>
	/// The CPUs instruction pointer value
	/// </summary>
	public int IP { get; init; }

	/// <summary>
	/// The CPUs stack pointer value
	/// </summary>
	public int SP { get; init; } = 0xf7;

	#endregion

	#region Flag Properties

	/// <summary>
	/// Shows state of the internal interrupts flag
	/// </summary>
	public bool InterruptsEnabled { get; init; }

	/// <summary>
	/// Shows state of the internal Debug flag
	/// </summary>
	public bool IsDebugging { get; init; }

	/// <summary>
	/// State of the Zero flag
	/// </summary>
	public bool Zero { get; init; }

	/// <summary>
	/// State of the NonZero flag
	/// </summary>
	public bool NonZero { get; init; }

	/// <summary>
	/// State of the Carry flag
	/// </summary>
	public bool Carry { get; init; }

	/// <summary>
	/// State of the NoCarry flag
	/// </summary>
	public bool NoCarry { get; init; }

	/// <summary>
	/// State of the ParityEven flag
	/// </summary>
	public bool ParityEven { get; init; }

	/// <summary>
	/// State of the ParityOdd flag
	/// </summary>
	public bool ParityOdd { get; init; }

	/// <summary>
	/// State of the Minus flag
	/// </summary>
	public bool Minus { get; init; }

	private A19.CpuFlags Flags => GetFlags();

	#endregion

	public CpuState()
	{
	}

	public CpuState(TestableA19Cpu cpu)
	{
		A = cpu.A;
		B = cpu.B;
		C = cpu.C;
		IP = cpu.IP;
		SP = cpu.SP;
		InterruptsEnabled = cpu.IF;
		IsDebugging = cpu.DF;
		Zero = cpu.Zero;
		NonZero = cpu.NonZero;
		Carry = cpu.Carry;
		NoCarry = cpu.NoCarry;
		ParityEven = cpu.ParityEven;
		ParityOdd = cpu.ParityOdd;
		Minus = cpu.Minus;
	}
	
	private A19.CpuFlags GetFlags()
	{
		A19.CpuFlags flags = A19.CpuFlags.None;
		if (Zero)
			flags |= A19.CpuFlags.Zero;
		if (NonZero)
			flags |= A19.CpuFlags.NonZero;
		if (Carry)
			flags |= A19.CpuFlags.Carry;
		if (NoCarry)
			flags |= A19.CpuFlags.NoCarry;
		if (ParityEven)
			flags |= A19.CpuFlags.ParityEven;
		if (ParityOdd)
			flags |= A19.CpuFlags.ParityOdd;
		if (Minus)
			flags |= A19.CpuFlags.Minus;
		return flags;
	}

	public override string ToString()
	{
		return $"IP={IP:X4}, SP={SP:X4}, A={A:X2}, B={B:X2}, C={C:X2}, IF=>{(InterruptsEnabled ? "Y" : "N")}, Debug=>{(IsDebugging ? "Y" : "N")}, Flags=>[{Flags}]";
	}
}

public record DebugCpuState
	: CpuState
{
	public DebugCpuState()
	{
		IsDebugging = true;
	}

	public override string ToString()
	{
		return base.ToString();
	}
}