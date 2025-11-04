// ReSharper disable InconsistentNaming

namespace SimpleInstructionMachine.VirtualProcessors;

public class VirtualMachine(bool isDebug)
	: AbstractVirtualMachine(isDebug)
{
	private int _stackHeadPointer = KnownMemory.STACK_BOTTOM;
	private bool _interruptsEnabled;

	public override async Task Execute()
	{
		InstructionPointer = KnownMemory.APP_DATA_BOTTOM;
		
		var halted = false;
		while (!halted)
		{
			// Check for interrupt
			if (_interruptsEnabled && Console.KeyAvailable)
			{
				var key = Console.ReadKey();
				if (key.Key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
				{
					PushToStack((byte)InstructionPointer);
					InstructionPointer = key.Key is ConsoleKey.LeftArrow
						? ByteCode[KnownMemory.IVT_LEFT_ARROW]
						: ByteCode[KnownMemory.IVT_RIGHT_ARROW];
				}
			}

			var opCode = ByteCode[InstructionPointer];
			switch (opCode >> 4)
			{
				case Mask.Misc:
					switch (opCode)
					{
						case OpCode.Halt:
							halted = true;
							break;
						case OpCode.Wait:
							await Wait();
							break;
						case OpCode.Nop:
							Nop();
							break;
						case OpCode.Sif:
							Sif();
							break;
						case OpCode.Cif:
							Cif();
							break;
						case OpCode.Ret:
							Ret();
							break;
						default:
							throw new Exception($"Unknown command: 0x{opCode:x2}");
					}
					break;

				case Mask.SaveToFile:
					await SaveToFile(opCode);
					break;
				case Mask.LoadFromFile:
					await LoadFromFile(opCode);
					break;
				case Mask.Write:
					Write(opCode);
					break;
				case Mask.Add:
					Add(opCode);
					break;
				case Mask.Sub:
					throw new Exception("Due to lack of time this method has not been implemented");
				case Mask.Inc:
					Inc(opCode);
					break;
				case Mask.Dec:
					Dec(opCode);
					break;
				case Mask.JumpIfZero:
					JumpIfZero(opCode);
					break;
				case Mask.JumpWithReturn:
					JumpWithReturn(opCode);
					break;
				default:
					throw new Exception($"Unknown command: 0x{opCode:x2}");
			}
		}
	}

	private void PushToStack(byte value)
	{
		ByteCode[_stackHeadPointer--] = value;
	}

	private byte PopFromStack()
	{
		_stackHeadPointer++;
		if (_stackHeadPointer > KnownMemory.STACK_BOTTOM)
			throw new Exception("Stack underflow");

		return ByteCode[_stackHeadPointer];
	}

	/// <summary>
	/// SaveToFile FileNumber SourceLocation Length
	/// </summary>
	private async Task SaveToFile(int opCode)
	{
		var fileNumber = Read(InstructionPointer + 1, opCode, 2);
		var sourceLocation = Read(InstructionPointer + 2, opCode, 1);
		var length = Read(InstructionPointer + 3, opCode, 0);

		DebugMessageWithCallerInfo($"Writing {length} bytes starting at {sourceLocation:x4} to file {fileNumber}.");

		await File.WriteAllBytesAsync(FilenameFromFileNumber(fileNumber),
			ByteCode[sourceLocation..(sourceLocation + length)]);

		InstructionPointer += 4;
	}

	/// <summary>
	/// LoadFromFile FileNumber SourceLocation Length
	/// </summary>
	private async Task LoadFromFile(int opCode)
	{
		var fileNumber = Read(InstructionPointer + 1, opCode, 1);
		var targetLocation = Read(InstructionPointer + 2, opCode, 0);

		var fileContents = await File.ReadAllBytesAsync(FilenameFromFileNumber(fileNumber));
		if (fileContents.Length + targetLocation > ByteCode.Length)
			throw new Exception("File too large");
		fileContents.CopyTo(ByteCode, targetLocation);

		DebugMessageWithCallerInfo($"Reading from file {fileNumber} into {targetLocation:x4}.");
		InstructionPointer += 3;
	}

	/// <summary>
	/// Write [Location] Value
	/// </summary>
	private void Write(int opCode)
	{
		var location = Read(InstructionPointer + 1, opCode, 1);
		var value = Read(InstructionPointer + 2, opCode, 0);

		DebugMessageWithCallerInfo($"{value} into {location:X2}");
		WriteToMemory(location, value);
		InstructionPointer += 3;
	}

	/// <summary>
	/// Add LHS RHS Total
	/// </summary>
	private void Add(int opCode)
	{
		var lhs = Read(InstructionPointer + 1, opCode, 2);
		var rhs = Read(InstructionPointer + 2, opCode, 1);
		var location = Read(InstructionPointer + 3, opCode, 0);

		DebugMessageWithCallerInfo($"{lhs} + {rhs} = {lhs + rhs} ==> {location:x4}");
		WriteToMemory(location, lhs + rhs); // Can overflow
		InstructionPointer += 4;
	}

	/// <summary>
	/// Inc [Location]
	/// </summary>
	private void Inc(int opCode)
	{
		var location = Read(InstructionPointer + 1, opCode, 0);
		var newValue = (byte)(1 + ByteCode[location]);
		DebugMessageWithCallerInfo($"Increasing value in {location:x4} from {ByteCode[location]} to {newValue}");
		WriteToMemory(location, newValue);
		InstructionPointer += 2;
	}

	/// <summary>
	/// Dec [Location]
	/// </summary>
	private void Dec(int opCode)
	{
		var location = Read(InstructionPointer + 1, opCode, 0);
		var newValue = (byte)(ByteCode[location] - 1);
		DebugMessageWithCallerInfo($"Decreasing value in {location:x4} from {ByteCode[location]} to {newValue}");
		WriteToMemory(location, newValue);
		InstructionPointer += 2;
	}

	/// <summary>
	/// Nop
	/// </summary>
	private void Nop()
	{
		DebugMessageWithCallerInfo("");
		InstructionPointer++;
	}

	/// <summary>
	/// Sif
	/// </summary>
	private void Sif()
	{
		DebugMessageWithCallerInfo($"Was previously {_interruptsEnabled}");
		_interruptsEnabled = true;
		InstructionPointer++;
	}

	/// <summary>
	/// Cif
	/// </summary>
	private void Cif()
	{
		DebugMessageWithCallerInfo($"Was previously {_interruptsEnabled}");
		_interruptsEnabled = false;
		InstructionPointer++;
	}

	private async Task Wait()
	{
		DebugMessageWithCallerInfo("");
		DebugMessage($"{InstructionPointer:x4} Wait::");
		await Task.Delay(100);
		InstructionPointer++;
	}

	private void Ret()
	{
		var gotoAddress = PopFromStack();
		DebugMessageWithCallerInfo($"{InstructionPointer:x4} to {gotoAddress:x4}");
		InstructionPointer = gotoAddress;
	}

	/// <summary>
	/// JumpIfZero ValueToCheck Address
	/// </summary>
	private void JumpIfZero(int opCode)
	{
		var addressToCheck = Read(InstructionPointer + 1, opCode, 1);
		var locationToJumpTo = Read(InstructionPointer + 2, opCode, 0);
		var valueToCheck = ByteCode[addressToCheck];
		DebugMessageWithCallerInfo($"jump to {locationToJumpTo:X2} if {valueToCheck} is 0 [{valueToCheck == 0}]");
		InstructionPointer = (valueToCheck == 0)
			? locationToJumpTo
			: InstructionPointer += 3;
	}

	/// <summary>
	/// JumpWithReturn Address
	/// </summary>
	private void JumpWithReturn(int opCode)
	{
		var locationToJumpTo = Read(InstructionPointer + 1, opCode, 0);
		DebugMessageWithCallerInfo($"jump to {locationToJumpTo:X2}");
		PushToStack((byte)(InstructionPointer + 2));
		InstructionPointer = locationToJumpTo;
	}

	private byte Read(int location, int opCode, int mask)
	{
		//isSet means immediate rather than memory
		var isSet = (((opCode & 0b0000_1111) >> mask) & 1) == 1;
		// Console.WriteLine($"{opCode:b8} {(opCode & 0b0000_1111):b8} mask {mask} {(((opCode & 0b0000_1111) >> mask) & 1 ):b8} {isSet} ");

		if (location >= ByteCode.Length)
			throw new Exception("Illegal memory location " + location);

		if (isSet)
		{
			// Console.WriteLine($"{opCode:b8}, {mask}, {_memory[location]:x8}");  
			return ByteCode[location];
		}

		var reference = ByteCode[location];
		if (reference >= ByteCode.Length)
			throw new Exception("Illegal de-referenced memory location " + location);
		return ByteCode[reference];
	}
}