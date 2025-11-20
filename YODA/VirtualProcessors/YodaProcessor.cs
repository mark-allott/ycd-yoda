using System.Runtime.CompilerServices;
using System.Text;
using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.VirtualProcessors;

public class YodaProcessor
	: IVirtualProcessor, IVirtualProcessorStrategy, IVirtualProcessorStrategyAsync
{
	#region Fields

	private readonly bool _isDebug;
	private readonly ILogger _logger;
	private readonly IFileSystem<byte> _fileSystem;
	private readonly IMemoryAccess<byte> _memoryAccess;
	private bool _interruptsEnabled;

	#endregion

	#region IVirtualProcessor implementation

	/// <summary>
	/// Exposes the current address being executed
	/// </summary>
	public int InstructionPointer { get; private set; }

	/// <summary>
	/// Exposes the address of the base of the stack
	/// </summary>
	public int StackPointer { get; private set; }

	/// <summary>
	/// Exposes whether interrupts are enabled
	/// </summary>
	public bool InterruptFlag => _interruptsEnabled;

	/// <summary>
	/// Exposes the debug flag
	/// </summary>
	public bool Debugging => _isDebug;

	#endregion

	#region Constructors

	public YodaProcessor(bool isDebug, ILogger logger, IFileSystem<byte> fileSystem,
		IMemoryAccess<byte> memoryAccess)
	{
		_isDebug = isDebug;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		_memoryAccess = memoryAccess ?? throw new ArgumentNullException(nameof(memoryAccess));
	}

	#endregion

	#region IVirtualProcessorStrategy Members

	/// <inheritdoc/>
	public void Run()
	{
		Task.Run(() => RunAsync(CancellationToken.None));
	}

	#endregion

	#region IVirtualProcessorStrategyAsync Members

	/// <inheritdoc/>
	public async Task RunAsync(CancellationToken token)
	{
		//	All execution is wrapped in an exception handler so any errors can be reported in the console
		try
		{
			await Execute(token);
			_logger.Log(LogLevel.Screen, $"{Environment.NewLine}{Environment.NewLine}Program completed successfully");
		}
		catch (Exception e)
		{
			//	Build the "oops" message
			var sb = new StringBuilder("Your program has crashed! Things aren't looking too good for the space craft.")
				.AppendLine($"\n{e.Message}")
				.AppendLine($"Instruction Pointer: {InstructionPointer:x4}")
				.AppendLine($"Opcode: {_memoryAccess.Memory[InstructionPointer]:x2}\n");
			_logger.Log(LogLevel.Critical, sb.ToString());

			// Dump as bytes into the file system
			await _fileSystem.WriteBinaryCrashDumpAsync(_memoryAccess.Memory, token);
			// Dump as text into the file system
			await _fileSystem.WriteTextCrashDumpAsync(_memoryAccess.Memory, InstructionPointer, token);
			_logger.Log(LogLevel.Critical,
				$"A crash dump containing all the memory has been written to : '{_fileSystem.BinaryCrashDumpFileName}' and '{_fileSystem.TextCrashDumpFileName}'");
		}
	}

	#endregion

	#region Methods

	private async Task Execute(CancellationToken token)
	{
		InstructionPointer = KnownMemory.APP_DATA_BOTTOM;
		StackPointer = KnownMemory.STACK_BOTTOM;

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
						? _memoryAccess.Memory[KnownMemory.IVT_LEFT_ARROW]
						: _memoryAccess.Memory[KnownMemory.IVT_RIGHT_ARROW];
				}
			}

			var opCode = _memoryAccess.Memory[InstructionPointer];
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
					await SaveToFile(opCode, token);
					break;
				case Mask.LoadFromFile:
					await LoadFromFile(opCode, token);
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

			//	Halt processing if the cancellation token has requested cancellation
			halted = halted || token.IsCancellationRequested;
		}
	}

	private void DebugMessageWithCallerInfo(string message, [CallerMemberName] string callerMemberName = "")
	{
		if (!_isDebug)
			return;
		_logger.Log(LogLevel.Debug, $"{InstructionPointer:x4} {callerMemberName}:: {message}");
	}

	#endregion

	#region Instruction handlers

	private void PushToStack(byte value)
	{
		_memoryAccess.Memory[StackPointer--] = value;
	}

	private byte PopFromStack()
	{
		return StackPointer >= KnownMemory.STACK_BOTTOM
			? throw new Exception("Stack underflow")
			: _memoryAccess.Memory[StackPointer++];
	}

	/// <summary>
	/// SaveToFile FileNumber SourceLocation Length
	/// </summary>
	private async Task SaveToFile(int opCode, CancellationToken token = default)
	{
		var fileNumber = Read(InstructionPointer + 1, opCode, 2);
		var sourceLocation = Read(InstructionPointer + 2, opCode, 1);
		var length = Read(InstructionPointer + 3, opCode, 0);

		DebugMessageWithCallerInfo($"Writing {length} bytes starting at {sourceLocation:x4} to file {fileNumber}.");

		await _fileSystem.SaveToFileAsync(fileNumber,
			_memoryAccess.Memory[sourceLocation..(sourceLocation + length)], token);

		InstructionPointer += 4;
	}

	/// <summary>
	/// LoadFromFile FileNumber SourceLocation Length
	/// </summary>
	private async Task LoadFromFile(int opCode, CancellationToken token = default)
	{
		var fileNumber = Read(InstructionPointer + 1, opCode, 1);
		var targetLocation = Read(InstructionPointer + 2, opCode, 0);

		var fileContents = await _fileSystem.LoadFromFileAsync(fileNumber, token);
		if (fileContents.Length + targetLocation > _memoryAccess.MemorySize)
			throw new OutOfMemoryException("File too large");
		_memoryAccess.WriteToMemory(targetLocation, fileContents);

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
		_memoryAccess.WriteToMemory(location, value);
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
		_memoryAccess.WriteToMemory(location, (byte)(lhs + rhs));
		InstructionPointer += 4;
	}

	/// <summary>
	/// Inc [Location]
	/// </summary>
	private void Inc(int opCode)
	{
		var location = Read(InstructionPointer + 1, opCode, 0);
		var oldValue = _memoryAccess.Memory[location];
		var newValue = (byte)(1 + oldValue);
		DebugMessageWithCallerInfo($"Increasing value in {location:x4} from {oldValue} to {newValue}");
		_memoryAccess.WriteToMemory(location, newValue);
		InstructionPointer += 2;
	}

	/// <summary>
	/// Dec [Location]
	/// </summary>
	private void Dec(int opCode)
	{
		var location = Read(InstructionPointer + 1, opCode, 0);
		var oldValue = _memoryAccess.Memory[location];
		var newValue = (byte)(oldValue - 1);
		DebugMessageWithCallerInfo($"Decreasing value in {location:x4} from {oldValue} to {newValue}");
		_memoryAccess.WriteToMemory(location, newValue);
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
		var valueToCheck = _memoryAccess.Memory[addressToCheck];
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

		if (location >= _memoryAccess.MemorySize)
			throw new Exception("Illegal memory location " + location);

		if (isSet)
		{
			// Console.WriteLine($"{opCode:b8}, {mask}, {_memory[location]:x8}");  
			return _memoryAccess.ReadFromMemory(location);
		}

		var reference = _memoryAccess.ReadFromMemory(location);
		if (reference >= _memoryAccess.MemorySize)
			throw new Exception("Illegal de-referenced memory location " + location);
		return _memoryAccess.ReadFromMemory(reference);
	}

	#endregion
}