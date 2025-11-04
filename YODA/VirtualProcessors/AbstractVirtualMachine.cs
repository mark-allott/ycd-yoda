using System.Runtime.CompilerServices;
using System.Text;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.VirtualProcessors;

public abstract class AbstractVirtualMachine
	: IVirtualMachine
{
	#region Constructors

	/// <summary>
	/// Initialises the class with specified debug setting and memory area
	/// </summary>
	/// <param name="isDebug">Indicates whether the system runs in "debug" mode</param>
	/// <param name="memorySize">The size of the memory area for the VM to occupy</param>
	protected AbstractVirtualMachine(bool isDebug, int memorySize = byte.MaxValue + 1)
	{
		IsDebug = isDebug;
		ByteCode = new byte[memorySize];
	}

	#endregion

	#region IVirtualMachine Members

	/// <inheritdoc />
	/// <exception cref="DirectoryNotFoundException"></exception>
	public async Task Run(string folderPath)
	{
		//	No point attempting to do anything if the folder doesn't even exist!!
		if (!Directory.Exists(folderPath))
			throw new DirectoryNotFoundException(folderPath);
		Folder = folderPath;

		//	The bootfile for the virtual machines is ALWAYS called boot
		var filename = Path.Combine(Folder, "boot");
		if(!File.Exists(filename))
			throw new FileNotFoundException(filename);
		
		//	Read and check the file for length
		var fileContents = await File.ReadAllBytesAsync(filename);
		if (fileContents.Length > ByteCode.Length)
			throw new ArgumentOutOfRangeException(nameof(folderPath), fileContents.Length, $"File exceeds maximum allowed file size {ByteCode.Length}");

		//	Calls the alternate run method with the byte array read from the file
		await Run(fileContents);
	}

	/// <inheritdoc />
	public async Task Run(byte[] bootData)
	{
		//	Do some sanity checks / initialisation
		ArgumentNullException.ThrowIfNull(bootData);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(bootData.Length, ByteCode.Length, nameof(bootData));
		ByteCode.Initialize();
		bootData.CopyTo(ByteCode, 0);

		//	All execution is wrapped in an exception handler so any errors can be reported in the console
		try
		{
			await Execute();
			ConsoleMessage($"{Environment.NewLine}{Environment.NewLine}Program completed successfully");
		}
		catch (Exception e)
		{
			//	Build the "oops" message
			var sb = new StringBuilder("Your program has crashed! Things aren't looking too good for the space craft.")
				.AppendLine($"\n{e.Message}")
				.AppendLine($"Instruction Pointer: {InstructionPointer:x4}")
				.AppendLine($"Opcode: {ByteCode[InstructionPointer]:x2}\n");
			await ErrorMessageAsync(sb.ToString());

			// Dump as bytes into the named folder
			await File.WriteAllBytesAsync(Path.Combine(Folder, "crash_dump"), ByteCode);

			// Dump as text into the named folder
			await using var textFile = File.CreateText(Path.Combine(Folder, "crash_dump.txt"));
			for (var i = 0; i < ByteCode.Length; i++)
				await textFile.WriteLineAsync($"{i:X2}   {ByteCode[i]}{(i == InstructionPointer ? "    <---- INSTRUCTION POINTER" : "")}");

			await textFile.FlushAsync();

			await ErrorMessageAsync("A crash dump containing all the memory has been written to : crash_dump and crash_dump.txt");
		}
	}

	/// <inheritdoc />
	public abstract Task Execute();

	#endregion

	#region Fields

	/// <summary>
	/// Holds the bytecode to be executed - size can be specified by 
	/// </summary>
	protected readonly byte[] ByteCode;

	/// <summary>
	/// Indicates whether the virtual machine is operating in "debug" mode and shows verbose messaging during execution
	/// </summary>
	protected bool IsDebug { get; private set; }
	
	/// <summary>
	/// The folder into which files are read from and written to
	/// </summary>
	protected string Folder = ".";

	/// <summary>
	/// Current location in the program being executed
	/// </summary>
	protected int InstructionPointer = KnownMemory.APP_DATA_BOTTOM;

	#endregion

	#region Common Methods

	/// <summary>
	/// Writes a message to the console
	/// </summary>
	/// <param name="message">The message to display</param>
	protected static void ConsoleMessage(string message)
	{
		Console.WriteLine(message);
	}

	/// <summary>
	/// Writes a <paramref name="message"/> to the console if <see cref="IsDebug"/> is set
	/// </summary>
	/// <param name="message">The message to display</param>
	protected void DebugMessage(string message)
	{
		if (!IsDebug)
			return;
		ConsoleMessage(message);
	}

	/// <summary>
	/// Writes a <paramref name="message"/> to the console, if <see cref="IsDebug"/> is set, prefixing the message with the current <see cref="InstructionPointer"/> and <paramref name="callerMemberName"/>
	/// </summary>
	/// <param name="message">The message to display in the console</param>
	/// <param name="callerMemberName">The name of the calling method</param>
	protected void DebugMessageWithCallerInfo(string message, [CallerMemberName] string callerMemberName = "")
	{
		if (!IsDebug)
			return;
		ConsoleMessage($"{InstructionPointer:x4} {callerMemberName}:: {message}");
	}
	
	/// <summary>
	/// Writes a <paramref name="message"/> to the error console
	/// </summary>
	/// <param name="message">The message to display in the error console</param>
	protected static void ErrorMessage(string message)
	{
		Console.Error.WriteLine(message);
	}

	/// <summary>
	/// Writes a <paramref name="message"/> to the error console asynchronously
	/// </summary>
	/// <param name="message">The message to display in the error console</param>
	protected async Task ErrorMessageAsync(string message)
	{
		await Console.Error.WriteLineAsync(message);
	}

	/// <summary>
	/// Returns the name of the file to use based on <paramref name="fileNumber"/>
	/// </summary>
	/// <param name="fileNumber">The number of the file</param>
	/// <returns>The text of the filename</returns>
	/// <exception cref="Exception"></exception>
	/// <remarks>
	/// Any <paramref name="fileNumber"/> in the range 0..7 shall return the fully qualified name of the binary file.
	/// Any in the range 8..15 shall return the name of the fully qualified name with the ".txt" file extension
	/// </remarks>
	protected string FilenameFromFileNumber(byte fileNumber)
	{
		return fileNumber switch
		{
			< 8 => Path.Combine(Folder, $"{fileNumber}"),
			< 16 => Path.Combine(Folder, $"{fileNumber}.txt"),
			_ => throw new Exception(
				$"Unknown file {fileNumber}. Binary files are between 0 and 7. Text files are between 8 and 15")
		};
	}

	/// <summary>
	/// Writes the <paramref name="value"/> into <paramref name="location"/>, checking to see if the screen is to be updated in the process
	/// </summary>
	/// <param name="location">The memory location to update</param>
	/// <param name="value">The new value for the location</param>
	/// <returns>The new value</returns>
	protected byte WriteToMemory(int location, int value)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThan(location, ByteCode.Length, nameof(location));

		var byteValue = (byte)value;
		if (location != KnownMemory.ControlFlags || 
		    (ByteCode[location] & 1) == 1 || (value & 1) == 0)
			return ByteCode[location] = byteValue;
		UpdateScreen();
		return ByteCode[location] = byteValue;
	}

	private const string LcdDisplayOuter = "---------------------";

	/// <summary>
	/// Displays the current contents of the LCD segment values in the console
	/// </summary>
	protected void UpdateScreen()
	{
		var sb = new StringBuilder()
			.AppendLine(LcdDisplayOuter)
			.Append($"| {ToChar(ByteCode[KnownMemory.LCD_0])} | {ToChar(ByteCode[KnownMemory.LCD_1])} ")
			.Append($"| {ToChar(ByteCode[KnownMemory.LCD_2])} | {ToChar(ByteCode[KnownMemory.LCD_3])} ")
			.AppendLine($"| {ToChar(ByteCode[KnownMemory.LCD_4])} |")
			.AppendLine(LcdDisplayOuter);
		ConsoleMessage(sb.ToString());
		return;

		char ToChar(byte value)
		{
			return value switch
			{
				>= 32 and <= 255 => (char)value,
				_ => '?'
			};
		}
	}
	
	#endregion
}