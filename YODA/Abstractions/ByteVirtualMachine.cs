using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Abstractions;

public class ByteVirtualMachine
	: IVirtualMachine<byte>
{
	#region Fields

	private bool _isDebug;
	private readonly IMemoryAccess<byte> _memoryAccess;
	private readonly IVirtualDisplay<byte> _virtualDisplay;
	private readonly IFileSystemStrategy _fileSystemStrategy;
	private readonly ILogger _logger;
	private bool _bootstrapped;

	#endregion

	#region Properties

	//

	#endregion

	#region Constructors

	/// <summary>
	/// Alternate constructor allowing specific size of memory and the <see cref="IFileSystemStrategy"/> implementation to use for the underlying file system
	/// </summary>
	/// <param name="isDebug">Determines whether the machine is running in "debug" mode</param>
	/// <param name="memoryAccess">The class implementing memory access for the machine</param>
	/// <param name="virtualDisplay"></param>
	/// <param name="fileSystemStrategy">The class implementing <see cref="IFileSystemStrategy"/> for the machine</param>
	/// <param name="logger">The logging class to use for output</param>
	public ByteVirtualMachine(bool isDebug,
		IMemoryAccess<byte> memoryAccess,
		IVirtualDisplay<byte> virtualDisplay,
		IFileSystemStrategy fileSystemStrategy,
		ILogger logger)
	{
		_isDebug = isDebug;
		_memoryAccess = memoryAccess ?? throw new ArgumentNullException(nameof(memoryAccess));
		_virtualDisplay = virtualDisplay ?? throw new ArgumentNullException(nameof(virtualDisplay));
		_fileSystemStrategy = fileSystemStrategy ?? throw new ArgumentNullException(nameof(fileSystemStrategy));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region IVirtualMachine Members

	/// <inheritdoc/>
	public void Boot()
	{
		//	Load the file from the filesystem and bootstrap using the bytes read from the file
		Boot(_fileSystemStrategy.LoadBootFile());
	}

	/// <inheritdoc/>
	public void Boot(byte[] program)
	{
		//	Ensure it is of correct length
		ArgumentOutOfRangeException.ThrowIfZero(program.Length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(program.Length, _memoryAccess.MemorySize);
		//	Copy to the system memory
		_memoryAccess.WriteToMemory(0, program);
		_bootstrapped = true;
	}

	/// <inheritdoc/>
	public void Run()
	{
		Task.Run(() => RunAsync(CancellationToken.None));
	}

	/// <inheritdoc/>
	public Task RunAsync(CancellationToken token)
	{
		if (!_bootstrapped)
			throw new InvalidOperationException("VirtualMachine is not bootstrapped");

		_logger.Log(LogLevel.Screen, "Starting landing computer running York's Obscenely Dumb Architecture (YODA) - Release Build 12x.11g-34 + Anti-gravity module");
		_logger.Log(LogLevel.Screen, $"Folder Path: {_fileSystemStrategy.Folder}\n" );
		_logger.Log(LogLevel.Screen, $"Connecting to Engine Control System.... SUCCESS!" );
		_logger.Log(LogLevel.Screen, $"Connecting to Landing Control System.... SUCCESS!" );
		_logger.Log(LogLevel.Screen, $"Connecting to Interplanetary Communication System.... SUCCESS!" );
		_logger.Log(LogLevel.Screen, $"All systems are GO!" );
		
		throw new NotImplementedException();
	}

	#endregion
}