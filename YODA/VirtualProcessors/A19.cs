using System.ComponentModel;

namespace SimpleInstructionMachine.VirtualProcessors;

public class A19(bool isDebug)
	: AbstractVirtualMachine(isDebug)
{
	[Flags]
	public enum CpuFlags
	{
		None = 0,
		Zero = 1 << 0,
		NonZero = 1 << 1,
		Carry = 1 << 2,
		NoCarry = 1 << 3,
		ParityEven = 1 << 4,
		ParityOdd = 1 << 5,
		Minus = 1 << 6,
	}

	private enum Register
	{
		A = 0,
		B = 1,
		C = 2,
		N = 3,
		[Description("[B]")] DirectB = 4,
		[Description("[C]")] DirectC = 5,
		[Description("[N]")] DirectN = 6,
	}

	#region Fields

	protected int StackPointer = KnownMemory.STACK_BOTTOM;
	protected bool InterruptsEnabled;

	#endregion

	#region Properties

	public byte A { get; private set; }
	public byte B { get; private set; }
	public byte C { get; private set; }

	public CpuFlags Flags { get; private set; }

	public bool Zero => Flags.HasFlag(CpuFlags.Zero);
	public bool NonZero => Flags.HasFlag(CpuFlags.NonZero);
	public bool Carry => Flags.HasFlag(CpuFlags.Carry);
	public bool NoCarry => Flags.HasFlag(CpuFlags.NoCarry);
	public bool ParityEven => Flags.HasFlag(CpuFlags.ParityEven);
	public bool ParityOdd => Flags.HasFlag(CpuFlags.ParityOdd);
	public bool Minus => Flags.HasFlag(CpuFlags.Minus);

	public byte OpCode => ByteCode[InstructionPointer];
	public byte Data1 => ByteCode[InstructionPointer + 1];
	public byte Data2 => ByteCode[InstructionPointer + 2];

	#endregion

	#region AbstractVirtualMachine override

	/// <inheritdoc />
	/// <exception cref="InvalidOperationException"></exception>
	public override async Task Execute()
	{
		InterruptsEnabled = false;
		StackPointer = KnownMemory.STACK_BOTTOM;
		InstructionPointer = KnownMemory.APP_DATA_BOTTOM;
		DebugMessageWithCallerInfo($"Starting program execution");

		//	Loop until a "halt" instruction is seen
		while (true)
		{
			//	Check for any keypresses
			if (Console.KeyAvailable)
			{
				//	Grab the key pressed
				var keyPressed = Console.ReadKey().Key;

				//	Process the keypress if interrupts are enabled
				if (InterruptsEnabled)
					InstructionPointer = keyPressed switch
					{
						ConsoleKey.LeftArrow => ByteCode[KnownMemory.IVT_LEFT_ARROW],
						ConsoleKey.RightArrow => ByteCode[KnownMemory.IVT_RIGHT_ARROW],
						_ => InstructionPointer
					};
			}

			//	If halt is seen, stop processing now and return
			if (OpCode == 0)
				return;

			//	Do lookups on the opcodes and call the appropriate methods, setting the new instruction location on return
			InstructionPointer = OpCode switch
			{
				1 => DisableInterrupt(),
				2 => Nop(),
				8 => await Suspend(),
				9 => EnableInterrupt(),
				>= 0x11 and < 0x17 => await LoadFromFile(),
				>= 0x19 and < 0x1E => await SaveToFile(),

				>= 0x20 and < 0x27 => Load(),
				>= 0x28 and < 0x2E => Load(),
				>= 0x30 and < 0x37 => Load(),
				>= 0x38 and < 0x3E => Load(),

				>= 0x40 and < 0x47 => Add(),
				>= 0x48 and < 0x4E => Add(),
				>= 0x50 and < 0x57 => Add(),
				>= 0x58 and < 0x5E => Add(),

				>= 0x60 and < 0x67 => Subtract(),
				>= 0x68 and < 0x6E => Subtract(),
				>= 0x70 and < 0x77 => Subtract(),
				>= 0x78 and < 0x7E => Subtract(),

				>= 0x80 and < 0x87 => And(),
				>= 0x88 and < 0x8E => Or(),

				>= 0x90 and < 0x97 => Xor(),
				>= 0x98 and < 0x9E => Compare(),

				>= 0xA0 and < 0xA7 => Inc(),
				>= 0xA8 and < 0xAE => Dec(),

				>= 0xB0 and < 0xB7 => Push(),

				>= 0xB8 and < 0xBB => Pop(),
				>= 0xBC and < 0xBE => Pop(),

				//	Operations depend on the state of the Zero flag
				0xC0 => Call(CpuFlags.Zero),
				0xC1 => Jump(CpuFlags.Zero),
				0xC2 => JumpRelative(CpuFlags.Zero),
				0xC3 => Return(CpuFlags.Zero),

				//	Operations depend on the state of the NonZero flag
				0xC4 => Call(CpuFlags.NonZero),
				0xC5 => Jump(CpuFlags.NonZero),
				0xC6 => JumpRelative(CpuFlags.NonZero),
				0xC7 => Return(CpuFlags.NonZero),

				//	Operations depend on the state of the Carry flag
				0xC8 => Call(CpuFlags.Carry),
				0xC9 => Jump(CpuFlags.Carry),
				0xCA => JumpRelative(CpuFlags.Carry),
				0xCB => Return(CpuFlags.Carry),

				//	Operations depend on the state of the NoCarry flag
				0xCC => Call(CpuFlags.NoCarry),
				0xCD => Jump(CpuFlags.NoCarry),
				0xCE => JumpRelative(CpuFlags.NoCarry),
				0xCF => Return(CpuFlags.NoCarry),

				//	Operations depend on the state of the ParityEven flag
				0xD0 => Call(CpuFlags.ParityEven),
				0xD1 => Jump(CpuFlags.ParityEven),
				0xD2 => JumpRelative(CpuFlags.ParityEven),
				0xD3 => Return(CpuFlags.ParityEven),

				//	Operations depend on the state of the ParityOdd flag
				0xD4 => Call(CpuFlags.ParityOdd),
				0xD5 => Jump(CpuFlags.ParityOdd),
				0xD6 => JumpRelative(CpuFlags.ParityOdd),
				0xD7 => Return(CpuFlags.ParityOdd),

				//	Operations depend on the state of the Minus flag
				0xD8 => Call(CpuFlags.Minus),
				0xD9 => Jump(CpuFlags.Minus),
				0xDA => JumpRelative(CpuFlags.Minus),
				0xDB => Return(CpuFlags.Minus),

				//	Direct operations - no flags required
				0xDC => Call(CpuFlags.None),
				0xDD => Jump(CpuFlags.None),
				0xDE => JumpRelative(CpuFlags.None),
				0xDF => Return(CpuFlags.None),

				//	Explicitly set flags
				0xE0 => SetFlag(CpuFlags.Zero),
				0xE1 => SetFlag(CpuFlags.Carry),
				0xE2 => SetFlag(CpuFlags.ParityEven),
				0xE3 => SetFlag(CpuFlags.Minus),

				//	Explicitly clear flags
				0xE4 => ClearFlag(CpuFlags.Zero),
				0xE5 => ClearFlag(CpuFlags.Carry),
				0xE6 => ClearFlag(CpuFlags.ParityEven),
				0xE7 => ClearFlag(CpuFlags.Minus),

				//	Multi-step operations
				0xF0 => LoadInc(),
				0xF1 => LoadIncRepeat(),
				0xF2 => CompareInc(),
				0xF3 => CompareIncRepeat(),
				0xF4 => LoadDec(),
				0xF5 => LoadDecRepeat(),
				0xF6 => CompareDec(),
				0xF7 => CompareDecRepeat(),

				_ => throw new InvalidOperationException($"Invalid operation: {OpCode}")
			};
			//	Always force the IP back into byte range if it moved out
			InstructionPointer = (byte)InstructionPointer;
		}
	}

	#endregion

	#region Utility methods

	/// <summary>
	/// Identifies the register for the operand element for an OpCode
	/// </summary>
	/// <param name="value">The specified value for the opcode</param>
	/// <returns>The register to target</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private Register GetOperandRegister(int value)
	{
		var operand = (value & 0x18) >> 3;
		return (operand) switch
		{
			0 => Register.A,
			1 => Register.B,
			2 => Register.C,
			3 => Register.DirectN,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	/// <summary>
	/// Identifies the register for the parameter element in an opcode
	/// </summary>
	/// <param name="value">The value of the opcode</param>
	/// <returns>The register being used</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private Register GetParameterRegister(int value)
	{
		return (value & 0x07) switch
		{
			>= 0 and < 7 => (Register)(value & 0x07),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	/// <summary>
	/// Gets the value of the specified <paramref name="register"/>. If using an Indirect Register, then the value of the byte at the indirect location is returned 
	/// </summary>
	/// <param name="register">The register to dereference</param>
	/// <returns>The value for the register, or indirect value</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private byte GetRegisterValue(Register register)
	{
		return register switch
		{
			Register.A => A,
			Register.B => B,
			Register.C => C,
			Register.N => Data1,
			Register.DirectB => ByteCode[B],
			Register.DirectC => ByteCode[C],
			Register.DirectN => ByteCode[Data1],
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	/// <summary>
	/// Calculates the new instruction pointer value based on the type of register used
	/// </summary>
	/// <param name="registerUsed">The register used in the operation</param>
	/// <returns>The new value for <see cref="AbstractVirtualMachine.InstructionPointer"/></returns>
	private int GetNewInstructionPointer(Register registerUsed)
	{
		return InstructionPointer + registerUsed is Register.N or Register.IndirectN
			? 2
			: 1;
	}

	/// <summary>
	/// Sets the value for the register, or Indirect location
	/// </summary>
	/// <param name="register">The register to be updated</param>
	/// <param name="value">The new value for the register</param>
	/// <returns>The new value</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private byte SetRegister(Register register, byte value)
	{
		return register switch
		{
			Register.A => A = value,
			Register.B => B = value,
			Register.C => C = value,
			Register.DirectB => WriteToMemory(B, value),
			Register.DirectC => WriteToMemory(C, value),
			Register.DirectN => WriteToMemory(Data1, value),
			_ => throw new ArgumentOutOfRangeException(nameof(register), register, $"Invalid register: {register}")
		};
	}

	/// <summary>
	/// Updates the state of the <see cref="Flags"/>, based on the operation(s) taking place
	/// </summary>
	/// <param name="target">The target register being changed</param>
	/// <param name="result">The new value for the register (allows for signed result to set positive/negative/carry flags)</param>
	/// <returns>The byte value assigned to the register</returns>
	private byte SetFlagsFromValue(Register target, int result)
	{
		//	if the result is over or under range, set the appropriate carry/minus flags
		if (result > 255)
			SetFlag(CpuFlags.Carry);
		else if (result < 0)
			SetFlag(CpuFlags.Minus);

		//	Mask to 8-bit value
		var value = (byte)(result & 0xFF);
		//	Only set/reset other flags if the target is register A
		if (target != Register.A)
			return value;

		//	If zero, set the Zero/NonZero flags accordingly
		SetFlag(value == 0 ? CpuFlags.Zero : CpuFlags.NonZero);
		//	Detect parity of bits
		var parityEven = $"{value:b8}".ToCharArray().Count(c => c == '1') % 2 == 0;
		SetFlag(parityEven ? CpuFlags.ParityEven : CpuFlags.ParityOdd);
		return value;
	}

	/// <summary>
	/// Compound operation to update the register value and flags
	/// </summary>
	/// <param name="target">The register being updated</param>
	/// <param name="result">The new value for the register</param>
	/// <returns>The byte value of <paramref name="result"/></returns>
	private byte SetValueAndFlags(Register target, int result)
	{
		return SetRegister(target, SetFlagsFromValue(target, result));
	}

	#endregion

	#region OpCode implementations

	/// <summary>
	/// Flags interrupts as disabled
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int DisableInterrupt()
	{
		_interruptsEnabled = false;
		return InstructionPointer + 1;
	}

	/// <summary>
	/// No Operation
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Nop()
	{
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Forces the virtual CPU to sleep for 100mSec
	/// </summary>
	/// <returns>The next instruction location</returns>
	private async Task<int> Suspend()
	{
		await Task.Delay(100);
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Flags interrupts as enabled
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int EnableInterrupt()
	{
		_interruptsEnabled = true;
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Loads memory with data from the file specified in register <see cref="A"/>
	/// </summary>
	/// <returns>The next instruction location</returns>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="FileLoadException"></exception>
	private async Task<int> LoadFromFile()
	{
		var fileName = FilenameFromFileNumber(A);
		if (!File.Exists(fileName))
			throw new FileNotFoundException($"File {fileName} not found");

		var param2 = GetParameterRegister(OpCode);
		var location = GetRegisterValue(param2);

		//	Grab the contents of the file so size etc. can be determined
		var fileContents = await File.ReadAllBytesAsync(fileName);

		//	The maximum location depends on the destination location:
		//		If writing to the LCD area, end of memory
		//		Elsewhere, bottom of stack - otherwise a stack overwrite occurs
		var maxLocation = location >= KnownMemory.LCD_0
			? ByteCode.Length
			: StackPointer - 1;
		if (location + fileContents.Length > maxLocation)
			throw new FileLoadException("File too large");

		//	When updating, the "file" might overwrite the screen area, so store the old flag value before it may be changed
		var oldControlFlagValue = ByteCode[KnownMemory.ControlFlags];
		DebugMessageWithCallerInfo($"Loading file {A} into location {param2} [{location:0x2}]");
		fileContents.CopyTo(ByteCode, location);

		//	Grab the new flag value, compare with the old and if set, force a screen update
		var newControlFlagValue = ByteCode[KnownMemory.ControlFlags];
		if (oldControlFlagValue != newControlFlagValue && (newControlFlagValue & 1) == 1)
			UpdateScreen();
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Saves the bytes specified into the file number in register <see cref="A"/>
	/// </summary>
	/// <returns>The next instruction location</returns>
	private async Task<int> SaveToFile()
	{
		var fileName = FilenameFromFileNumber(A);
		var param2 = GetParameterRegister(OpCode);
		var location = GetRegisterValue(param2);
		var nextIp = GetNewInstructionPointer(param2);
		var length = ByteCode[nextIp + 1];

		await File.WriteAllBytesAsync(fileName, ByteCode[location..(location + length)]);
		return 1 + nextIp;
	}

	/// <summary>
	/// Loads the appropriate register or indirect memory location with the value specified
	/// </summary>
	/// <returns>The next instruction location</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private int Load()
	{
		var param1 = GetOperandRegister(OpCode);
		var param2 = GetParameterRegister(OpCode);
		var value = GetRegisterValue(param2);
		DebugMessageWithCallerInfo($"{param1}, {param2} => {param1}, {value:x2}");

		switch (param1)
		{
			case Register.A:
				A = value;
				break;
			case Register.B:
				B = value;
				break;
			case Register.C:
				C = value;
				break;
			case Register.IndirectN:
				WriteToMemory(value, value);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Adds the specified registers together
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Add()
	{
		//	Wants A,B,C or (n)
		var param1 = GetOperandRegister((OpCode - 0x20) >> 4);
		//	Wants A,B,C,N,(B),(C) or (N)
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(param1);
		var rhs = GetRegisterValue(param2);
		//	Result allows for overflow (flags can be set for this)
		var value = SetValueAndFlags(param1, lhs + rhs);

		DebugMessageWithCallerInfo($"{param1}, {param2} => {param1} = {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Subtracts the specified registers from each other
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Subtract()
	{
		var param1 = GetOperandRegister((OpCode - 0x40) >> 4);
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(param1);
		var rhs = GetRegisterValue(param2);
		//	Result allows for overflow (flags can be set for this)
		var value = SetValueAndFlags(param1, lhs - rhs);
		DebugMessageWithCallerInfo($"{param1}, {param2} => {param1}, {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Performs a bitwise AND operation between <see cref="A"/> and another register
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int And()
	{
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(Register.A);
		var rhs = GetRegisterValue(param2);

		var result = lhs & rhs;
		var value = SetValueAndFlags(Register.A, result);
		DebugMessageWithCallerInfo($"{param2} => {Register.A} = {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Performs a bitwise OR operation between <see cref="A"/> and another register
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Or()
	{
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(Register.A);
		var rhs = GetRegisterValue(param2);
		var value = SetValueAndFlags(Register.A, lhs | rhs);
		DebugMessageWithCallerInfo($"{param2} => {Register.A} = {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Performs a bitwise XOR operation between <see cref="A"/> and another register
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Xor()
	{
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(Register.A);
		var rhs = GetRegisterValue(param2);
		var value = SetValueAndFlags(Register.A, lhs ^ rhs);
		DebugMessageWithCallerInfo($"{param2} => {Register.A} = {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Performs a compare operation between <see cref="A"/> and another register
	/// </summary>
	/// <returns>The next instruction location</returns>
	/// <remarks>
	/// The operation essentially subtracts the second register from the value in <see cref="A"/>, but leaves the values intact, but does update the <see cref="Flags"/> accordingly
	/// </remarks>
	private int Compare()
	{
		var param2 = GetParameterRegister(OpCode);
		//	get left and right side values
		var lhs = GetRegisterValue(Register.A);
		var rhs = GetRegisterValue(param2);
		var value = SetFlagsFromValue(Register.A, lhs - rhs);
		DebugMessageWithCallerInfo($"{param2} => {Register.A} = {value:0x2}");
		return GetNewInstructionPointer(param2);
	}

	/// <summary>
	/// Increments the value of the register or indirect location
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Inc()
	{
		var incRegister = GetParameterRegister(OpCode);
		var registerValue = GetRegisterValue(incRegister);
		var value = SetValueAndFlags(incRegister, registerValue + 1);
		DebugMessageWithCallerInfo($"{incRegister} => {incRegister} = {value:0x2}");
		return GetNewInstructionPointer(incRegister);
	}

	/// <summary>
	/// Decrements the value of the register or indirect location
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Dec()
	{
		var decRegister = GetParameterRegister(OpCode);
		var registerValue = GetRegisterValue(decRegister);
		var value = SetValueAndFlags(decRegister, registerValue - 1);
		DebugMessageWithCallerInfo($"{decRegister} => {decRegister} = {value:0x2}");
		return GetNewInstructionPointer(decRegister);
	}

	/// <summary>
	/// Pushes a value from a register or indirection location onto the stack
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int Push()
	{
		var register = GetParameterRegister(OpCode);
		var registerValue = GetRegisterValue(register);
		ByteCode[StackPointer--] = registerValue;
		DebugMessageWithCallerInfo($"{register} => {registerValue} [SP:{StackPointer:x4}]");
		return GetNewInstructionPointer(register);
	}

	/// <summary>
	/// Pops a value from the stack into the required register 
	/// </summary>
	/// <returns>The next instruction location</returns>
	/// <exception cref="StackOverflowException"></exception>
	private int Pop()
	{
		if (StackPointer == KnownMemory.STACK_BOTTOM)
			throw new StackOverflowException("Stack is empty");

		var register = GetParameterRegister(OpCode);
		var registerValue = ByteCode[StackPointer++];
		SetValueAndFlags(register, registerValue);
		DebugMessageWithCallerInfo($"{register} => {registerValue} [SP:{StackPointer:x4}]");
		return GetNewInstructionPointer(register);
	}

	/// <summary>
	/// Performs a call to another memory location, pushing the return address onto the stack 
	/// </summary>
	/// <param name="controlFlag">The <see cref="Flags"/> state required for the call to execute</param>
	/// <returns>The next instruction location</returns>
	/// <remarks>
	/// This is a common routine used for all call operations. If the value of <paramref name="controlFlag"/> is
	/// <see cref="CpuFlags.None"/>, the call always happens; otherwise the state of the specified flag is checked
	/// before performing the call, or moving to the next instruction
	/// </remarks>
	private int Call(CpuFlags controlFlag)
	{
		//	Next instruction to execute, irrespective of flags
		var nextIp = (byte)(InstructionPointer + 2);

		//	If required flag is set or an unconditional call
		if (controlFlag == CpuFlags.None || Flags.HasFlag(controlFlag))
		{
			//	push return address to stack
			ByteCode[StackPointer--] = nextIp;
			nextIp = Data1;
		}

		DebugMessageWithCallerInfo($"{(controlFlag != CpuFlags.None ? $"{controlFlag}" : "")} => {(controlFlag != CpuFlags.None ? $"{Flags.HasFlag(controlFlag)}" : "")} [IP:{InstructionPointer:x4}] [SP:{StackPointer:x4}]");
		return nextIp;
	}

	/// <summary>
	///	Jumps to a specific location in the code, provided the appropriate flags are set 
	/// </summary>
	/// <param name="controlFlag">The <see cref="Flags"/> state required for the jump to execute</param>
	/// <returns>The next instruction location</returns>
	/// <remarks>
	/// This is a common routine used for all jump operations. If the value of <paramref name="controlFlag"/> is
	/// <see cref="CpuFlags.None"/>, the jump always happens; otherwise the state of the specified flag is checked
	/// before performing the jump, or moving to the next instruction
	/// </remarks>
	private int Jump(CpuFlags controlFlag)
	{
		//	If a direct jump, or flags match, jump to desired address; otherwise next instruction along
		return (controlFlag == CpuFlags.None || Flags.HasFlag(controlFlag))
			? Data1
			: InstructionPointer + 2;
	}

	/// <summary>
	///	Performs a relative jump in the execution, from the current <see cref="AbstractVirtualMachine.InstructionPointer"/> 
	/// </summary>
	/// <param name="controlFlag">The <see cref="Flags"/> state required for the jump to execute</param>
	/// <returns>The next instruction location</returns>
	/// <remarks>
	/// This is a common routine used for all jump operations. If the value of <paramref name="controlFlag"/> is
	/// <see cref="CpuFlags.None"/>, the jump always happens; otherwise the state of the specified flag is checked
	/// before performing the jump, or moving to the next instruction
	/// </remarks>
	private int JumpRelative(CpuFlags controlFlag)
	{
		//	If a jump, or flags match, jump to desired address; otherwise next instruction along
		return (controlFlag == CpuFlags.None || Flags.HasFlag(controlFlag))
			? InstructionPointer + Data1
			: InstructionPointer + 2;
	}

	/// <summary>
	/// Returns from a call by popping the return address from the stack and setting the next instruction
	/// </summary>
	/// <param name="controlFlag">The <see cref="Flags"/> state required for the jump to execute</param>
	/// <returns>The next instruction location</returns>
	/// <exception cref="StackOverflowException"></exception>
	private int Return(CpuFlags controlFlag)
	{
		//	If required flag is not set and not a flagless return, move 1 instruction along
		if (controlFlag != CpuFlags.None && !Flags.HasFlag(controlFlag))
			return InstructionPointer + 1;

		if (StackPointer == KnownMemory.STACK_BOTTOM)
			throw new StackOverflowException("Stack is empty");

		return ByteCode[StackPointer++];
	}

	/// <summary>
	/// Sets the specified flag state for the processor
	/// </summary>
	/// <param name="controlFlag"></param>
	/// <returns>The next instruction location</returns>
	/// <exception cref="ArgumentException"></exception>
	private int SetFlag(CpuFlags controlFlag)
	{
		if ((controlFlag.HasFlag(CpuFlags.Zero) && controlFlag.HasFlag(CpuFlags.NonZero)) ||
		    (controlFlag.HasFlag(CpuFlags.Carry) && controlFlag.HasFlag(CpuFlags.NoCarry)) ||
		    (controlFlag.HasFlag(CpuFlags.ParityEven) && controlFlag.HasFlag(CpuFlags.ParityOdd)))
			throw new ArgumentException($"Invalid combination of flags: {controlFlag}", nameof(controlFlag));

		if (controlFlag.HasFlag(CpuFlags.Zero))
			Flags = (Flags | CpuFlags.Zero) & ~CpuFlags.NonZero;
		else if (controlFlag.HasFlag(CpuFlags.NonZero))
			Flags = (Flags | CpuFlags.NonZero) & ~CpuFlags.Zero;

		if (controlFlag.HasFlag(CpuFlags.Carry))
			Flags = (Flags | CpuFlags.Carry) & ~CpuFlags.NoCarry;
		else if (controlFlag.HasFlag(CpuFlags.NoCarry))
			Flags = (Flags | CpuFlags.NoCarry) & ~CpuFlags.Carry;

		if (controlFlag.HasFlag(CpuFlags.ParityEven))
			Flags = (Flags | CpuFlags.ParityEven) & ~CpuFlags.ParityOdd;
		else if (controlFlag.HasFlag(CpuFlags.ParityOdd))
			Flags = (Flags | CpuFlags.ParityOdd) & ~CpuFlags.ParityEven;

		Flags = controlFlag.HasFlag(CpuFlags.Minus)
			? Flags | CpuFlags.Minus
			: Flags & ~CpuFlags.Minus;

		DebugMessageWithCallerInfo($"{controlFlag} => [{Flags}]");
		return InstructionPointer + 1;
	}

	/// <summary>
	///	Clears the required flag on the processor 
	/// </summary>
	/// <param name="controlFlag"></param>
	/// <returns>The next instruction location</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private int ClearFlag(CpuFlags controlFlag)
	{
		//	Exclude trying to reset pairs of flags
		if ((controlFlag.HasFlag(CpuFlags.Zero) && controlFlag.HasFlag(CpuFlags.NonZero)) ||
		    (controlFlag.HasFlag(CpuFlags.Carry) && controlFlag.HasFlag(CpuFlags.NoCarry)) ||
		    (controlFlag.HasFlag(CpuFlags.ParityEven) && controlFlag.HasFlag(CpuFlags.ParityOdd)))
			throw new ArgumentException($"Invalid combination of flags: {controlFlag}", nameof(controlFlag));

		//	Reset flags in pairs
		Flags = controlFlag switch
		{
			CpuFlags.Zero => Flags & ~(CpuFlags.Zero | CpuFlags.NonZero),
			CpuFlags.Carry => Flags & ~(CpuFlags.Carry | CpuFlags.NoCarry),
			CpuFlags.ParityEven => Flags & ~(CpuFlags.ParityEven | CpuFlags.ParityOdd),
			CpuFlags.Minus => Flags & ~CpuFlags.Minus,
			_ => throw new ArgumentOutOfRangeException(nameof(controlFlag), controlFlag, null)
		};
		DebugMessageWithCallerInfo($"{controlFlag} => [{Flags}]");
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Loads the memory location at <see cref="B"/> with the value of <see cref="A"/>, then increments the value of <see cref="B"/>
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int LoadInc()
	{
		WriteToMemory(B++, GetRegisterValue(Register.A));
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Loads the memory location at <see cref="B"/> with the value of <see cref="A"/>, increments the value of <see cref="B"/>
	/// and repeats until <see cref="C"/> is zero
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int LoadIncRepeat()
	{
		do
		{
			ByteCode[B++] = GetRegisterValue(Register.A);
			C--;
		} while (C > 0);

		return InstructionPointer + 1;
	}

	/// <summary>
	/// Performs a comparison between the <see cref="A"/> register and the memory at the location pointed to by register
	/// <see cref="B"/>, updates the <see cref="Flags"/> state based on the comparison, then increments the value of
	/// register <see cref="B"/> 
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int CompareInc()
	{
		var rhs = GetRegisterValue(Register.DirectB);
		var result = A - rhs;
		SetFlagsFromValue(Register.A, result);
		B++;
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Performs a comparison between the <see cref="A"/> register and the memory at the location pointed to by register
	/// <see cref="B"/>, updates the <see cref="Flags"/> state based on the comparison, then increments the value of
	/// register <see cref="B"/> and repeats until register <see cref="C"/> is zero, or the comparison results in a
	/// <see cref="Zero"/> result 
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int CompareIncRepeat()
	{
		do
		{
			var rhs = GetRegisterValue(Register.DirectB);
			var result = A - rhs;
			SetFlagsFromValue(Register.A, result);
			B++;
			C--;
		} while (C > 0 || NonZero);

		return InstructionPointer + 1;
	}

	/// <summary>
	/// Loads the memory location at <see cref="B"/> with the value of <see cref="A"/>, then decrements the value of
	/// register <see cref="B"/>
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int LoadDec()
	{
		ByteCode[B--] = GetRegisterValue(Register.A);
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Loads the memory location at <see cref="B"/> with the value of <see cref="A"/>, decrements the value of register
	/// <see cref="B"/> and repeats until <see cref="C"/> is zero
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int LoadDecRepeat()
	{
		do
		{
			ByteCode[B--] = GetRegisterValue(Register.A);
			C--;
		} while (C != 0);

		return InstructionPointer + 1;
	}

	/// <summary>
	/// Performs a comparison between the <see cref="A"/> register and the memory at the location pointed to by register
	/// <see cref="B"/>, updates the <see cref="Flags"/> state based on the comparison, then decrements the value of
	/// register <see cref="B"/> 
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int CompareDec()
	{
		var rhs = GetRegisterValue(Register.DirectB);
		var result = A - rhs;
		SetFlagsFromValue(Register.A, result);
		B--;
		return InstructionPointer + 1;
	}

	/// <summary>
	/// Performs a comparison between the <see cref="A"/> register and the memory at the location pointed to by register
	/// <see cref="B"/>, updates the <see cref="Flags"/> state based on the comparison, then decrements the value of
	/// register <see cref="B"/> and repeats until register <see cref="C"/> is zero, or the comparison results in a
	/// <see cref="Zero"/> result 
	/// </summary>
	/// <returns>The next instruction location</returns>
	private int CompareDecRepeat()
	{
		do
		{
			var rhs = GetRegisterValue(Register.DirectB);
			var result = A - rhs;
			SetFlagsFromValue(Register.A, result);
			B--;
			C--;
		} while (C > 0 || NonZero);

		return InstructionPointer + 1;
	}

	#endregion
}