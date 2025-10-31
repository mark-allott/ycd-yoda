// ReSharper disable InconsistentNaming

using System.Text;

namespace SimpleInstructionMachine.VirtualProcessors;

public class VirtualMachine(bool debug)
	: AbstractVirtualMachine(debug)
{
	private readonly byte[] _memory = new byte[1 + byte.MaxValue];

	private int _instructionPointer = KnownMemory.APP_DATA_BOTTOM;
	private int _stackHeadPointer = KnownMemory.STACK_BOTTOM;
	private bool _interruptsEnabled;

	private string _folder = ".";

	public override async Task Run(string folderPath)
	{
		_folder = folderPath;
		await Boot();

		var halted = false;
		while (!halted)
		{
			// Check for interrupt
			if (_interruptsEnabled && Console.KeyAvailable)
			{
				var key = Console.ReadKey();
				if (key.Key == ConsoleKey.LeftArrow)
				{
					PushToStack((byte)_instructionPointer);
					_instructionPointer = _memory[KnownMemory.IVT_LEFT_ARROW];
				}
				else if (key.Key == ConsoleKey.RightArrow)
				{
					PushToStack((byte)_instructionPointer);
					_instructionPointer = _memory[KnownMemory.IVT_RIGHT_ARROW];
				}
			}

			var opCode = _memory[_instructionPointer];
			try
			{
				switch (opCode >> 4)
				{
					case Mask.Misc:
						switch (opCode)
						{
							case OpCode.Halt:
							{
								halted = true;
								continue;
							}
							case OpCode.Wait:
							{
								await Wait();
								continue;
							}
							case OpCode.Nop:
								Nop();
								continue;
							case OpCode.Sif:
								Sif();
								continue;
							case OpCode.Cif:
								Cif();
								continue;
							case OpCode.Ret:
							{
								Ret();
								continue;
							}
							default:
								throw new Exception("Unknown command " + opCode);
						}

					case Mask.SaveToFile:
						await SaveToFile(opCode);
						continue;
					case Mask.LoadFromFile:
						await LoadFromFile(opCode);
						continue;
					case Mask.Write:
						Write(opCode);
						continue;
					case Mask.Add:
						Add(opCode);
						continue;
					case Mask.Sub:
						throw new Exception("Due to lack of time this method has not been implemented");
					case Mask.Inc:
						Inc(opCode);
						continue;
					case Mask.Dec:
						Dec(opCode);
						continue;
					case Mask.JumpIfZero:
						JumpIfZero(opCode);
						continue;
					case Mask.JumpWithReturn:
						JumpWithReturn(opCode);
						continue;
					default:
						throw new Exception("Unknown command " + opCode);
				}
			}
			catch (Exception e)
			{
				var sb = new StringBuilder("Your program has crashed! Things aren't looking too good for the space craft.")
					.AppendLine($"\n{e.Message}")
					.AppendLine($"Instruction Pointer: {_instructionPointer:x4}")
					.AppendLine($"Opcode: {opCode:x2}\n");
				await ErrorMessage(sb.ToString());

				// Dump as bytes
				await File.WriteAllBytesAsync("crash_dump", _memory);

				// Dump as text
				await using var textFile = File.CreateText("crash_dump.txt");
				for (var i = 0; i < _memory.Length; i++)
					await textFile.WriteLineAsync($"{i:X2}   {_memory[i]}{(i == _instructionPointer ? "    <---- INSTRUCTION POINTER" : "")}");

				await textFile.FlushAsync();

				await ErrorMessage("A crash dump containing all the memory has been written to : crash_dump and crash_dump.txt");
				return;
			}
		}

		ConsoleMessage("\n\nProgram completed successfully");
	}

	private void PushToStack(byte value)
	{
		_memory[_stackHeadPointer--] = value;
	}

	private byte PopFromStack()
	{
		_stackHeadPointer++;
		if (_stackHeadPointer > KnownMemory.STACK_BOTTOM)
			throw new Exception("Stack underflow");

		return _memory[_stackHeadPointer];
	}

	private async Task Boot()
	{
		// Reset memory and pointers
		Array.Fill(_memory, (byte)0);
		_instructionPointer = KnownMemory.APP_DATA_BOTTOM;
		_interruptsEnabled = false;
		_stackHeadPointer = KnownMemory.STACK_BOTTOM;

		// Load the contents of the boot file into memory
		var filename = Path.Combine(_folder, "boot");
		if (File.Exists(filename))
		{
			var fileContents = await File.ReadAllBytesAsync(filename);
			if (fileContents.Length > _memory.Length)
				throw new Exception(
					$"The boot file is too large. It is {fileContents.Length} bytes long,  which exceeds the maximum allowed of {_memory.Length} bytes");

			fileContents.CopyTo(_memory, 0);

			ConsoleMessage($"\nMemory has been initialised using the boot file ({filename}).");
		}
		else
		{
			ConsoleMessage("\nNo boot file found.");
		}
	}

	private string FilenameFromFileNumber(byte fileNumber)
	{
		return fileNumber switch
		{
			< 8 => Path.Combine(_folder, $"{fileNumber}"),
			< 16 => Path.Combine(_folder, $"{fileNumber}.txt"),
			_ => throw new Exception(
				$"Unknown file {fileNumber}.  Binary files are between 0 and 7.   Text files are between 8 and 15")
		};
	}

	/// <summary>
	/// SaveToFile FileNumber SourceLocation Length
	/// </summary>
	private async Task SaveToFile(int opCode)
	{
		var fileNumber = Read(_instructionPointer + 1, opCode, 2);
		var sourceLocation = Read(_instructionPointer + 2, opCode, 1);
		var length = Read(_instructionPointer + 3, opCode, 0);

		DebugMessage($"{_instructionPointer:x4} SaveToFile:: Writing {length} bytes starting at {sourceLocation:x4} to file {fileNumber}.");

		await File.WriteAllBytesAsync(FilenameFromFileNumber(fileNumber),
			_memory[sourceLocation..(sourceLocation + length)]);

		_instructionPointer += 4;
	}

	/// <summary>
	/// LoadFromFile FileNumber SourceLocation Length
	/// </summary>
	private async Task LoadFromFile(int opCode)
	{
		var fileNumber = Read(_instructionPointer + 1, opCode, 1);
		var targetLocation = Read(_instructionPointer + 2, opCode, 0);

		var fileContents = await File.ReadAllBytesAsync(FilenameFromFileNumber(fileNumber));
		if (fileContents.Length + targetLocation > _memory.Length)
			throw new Exception("File too large");
		fileContents.CopyTo(_memory, targetLocation);

		DebugMessage($"{_instructionPointer:x4} LoadFromFile:: Reading from file {fileNumber} into {targetLocation:x4}.");

		_instructionPointer += 3;
	}

	/// <summary>
	/// Write [Location] Value
	/// </summary>
	private void Write(int opCode)
	{
		var location = Read(_instructionPointer + 1, opCode, 1);
		var value = Read(_instructionPointer + 2, opCode, 0);

		DebugMessage($"{_instructionPointer:x4} Write::  {value} into {location:X2}");

		UpdateScreenIfRequired(location, value);

		_memory[location] = value;
		_instructionPointer += 3;
	}

	private static readonly string LcdDisplayOuter = "---------------------";
	private void UpdateScreenIfRequired(byte location, byte value)
	{
		char ToChar(byte b)
		{
			if (b == 0x00)
				return ' ';
			else
				return (char)b;
		}

		//	If not screen location, do not do anything
		if (location != KnownMemory.ControlFlags)
			return;

		//	If already set, or the value is being reset, return
		if ((_memory[location] & 1) == 1 || (value & 1) == 0)
			return;
		
		//bit 0 has been set, refresh the LCD display
		var sb = new StringBuilder()
			.AppendLine(LcdDisplayOuter)
			.Append($"| {ToChar(_memory[KnownMemory.LCD_0])} | {ToChar(_memory[KnownMemory.LCD_1])} ")
			.Append($"| {ToChar(_memory[KnownMemory.LCD_2])} | {ToChar(_memory[KnownMemory.LCD_3])} ")
			.AppendLine($"| {ToChar(_memory[KnownMemory.LCD_4])} |")
			.AppendLine(LcdDisplayOuter);
		ConsoleMessage(sb.ToString());
	}

	/// <summary>
	/// Add LHS RHS Total
	/// </summary>
	private void Add(int opCode)
	{
		var lhs = Read(_instructionPointer + 1, opCode, 2);
		var rhs = Read(_instructionPointer + 2, opCode, 1);
		var location = Read(_instructionPointer + 3, opCode, 0);

		DebugMessage($"{_instructionPointer:x4} Add::  {lhs} + {rhs} = {lhs + rhs} ==> {location:x4}");
		_memory[location] = (byte)(lhs + rhs); // Can overflow
		_instructionPointer += 4;
	}

	/// <summary>
	/// Inc [Location]
	/// </summary>
	private void Inc(int opCode)
	{
		var location = Read(_instructionPointer + 1, opCode, 0);

		DebugMessage($"{_instructionPointer:x4} Inc::  Increasing value in {location:x4} from {_memory[location]} to {(_memory[location]) + 1}");

		_memory[location]++;
		_instructionPointer += 2;
	}

	/// <summary>
	/// Dec [Location]
	/// </summary>
	private void Dec(int opCode)
	{
		var location = Read(_instructionPointer + 1, opCode, 0);

		DebugMessage($"{_instructionPointer:x4} Dec::  Decreasing value in {location:x4} from {_memory[location]} to {(_memory[location]) - 1}");

		_memory[location]--;
		_instructionPointer += 2;
	}

	/// <summary>
	/// Nop
	/// </summary>
	private void Nop()
	{
		DebugMessage($"{_instructionPointer:x4} Nop::");
		_instructionPointer++;
	}

	/// <summary>
	/// Sif
	/// </summary>
	private void Sif()
	{
		DebugMessage($"{_instructionPointer:x4} Sif:: Was previously {_interruptsEnabled}");
		_interruptsEnabled = true;
		_instructionPointer++;
	}

	/// <summary>
	/// Cif
	/// </summary>
	private void Cif()
	{
		DebugMessage($"{_instructionPointer:x4} Cif:: Was previously {_interruptsEnabled}");
		_interruptsEnabled = false;
		_instructionPointer++;
	}

	private async Task Wait()
	{
		DebugMessage($"{_instructionPointer:x4} Wait::");
		await Task.Delay(100);
		_instructionPointer++;
	}


	private void Ret()
	{
		var gotoAddress = PopFromStack();

		DebugMessage($"{_instructionPointer:x4} Ret:: {_instructionPointer:x4} to {gotoAddress:x4}");
		_instructionPointer = gotoAddress;
	}

	/// <summary>
	/// JumpIfZero ValueToCheck Address
	/// </summary>
	private void JumpIfZero(int opCode)
	{
		var addressToCheck = Read(_instructionPointer + 1, opCode, 1);
		var locationToJumpTo = Read(_instructionPointer + 2, opCode, 0);
		var valueToCheck = _memory[addressToCheck];

		 DebugMessage($"{_instructionPointer:x4} JumpIfZero:: - jump to {locationToJumpTo:X2} if {valueToCheck} is 0");

		if (valueToCheck == 0)
			_instructionPointer = locationToJumpTo;
		else
			_instructionPointer += 3;
	}

	/// <summary>
	/// JumpWithReturn Address
	/// </summary>
	private void JumpWithReturn(int opCode)
	{
		var locationToJumpTo = Read(_instructionPointer + 1, opCode, 0);
		DebugMessage($"{_instructionPointer:x4} JumpWithReturn:: - jump to {locationToJumpTo:X2}");
		PushToStack((byte)(_instructionPointer + 2));
		_instructionPointer = locationToJumpTo;
	}

	private byte Read(int location, int opCode, int mask)
	{
		//isSet means immediate rather than memory
		var isSet = (((opCode & 0b0000_1111) >> mask) & 1) == 1;
		// Console.WriteLine($"{opCode:b8} {(opCode & 0b0000_1111):b8} mask {mask} {(((opCode & 0b0000_1111) >> mask) & 1 ):b8} {isSet} ");

		if (location >= _memory.Length)
			throw new Exception("Illegal memory location " + location);

		if (isSet)
		{
			// Console.WriteLine($"{opCode:b8}, {mask}, {_memory[location]:x8}");  
			return _memory[location];
		}

		var reference = _memory[location];
		if (reference >= _memory.Length)
			throw new Exception("Illegal de-referenced memory location " + location);

		// Console.WriteLine($"{opCode:b8}, {mask}, {reference:x8}, {_memory[reference]}");


		return _memory[reference];
	}
}