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
	private readonly IVirtualProcessorStrategy? _vCpu = null;
	private readonly IVirtualProcessorStrategyAsync? _vAsyncCpu = null;
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
	/// <param name="virtualDisplay">The class implementing a virtual display</param>
	/// <param name="fileSystemStrategy">The class implementing <see cref="IFileSystemStrategy"/> for the machine</param>
	/// <param name="vCpu">The class implementing the virtual CPU in a non-async runtime manner</param>
	/// <param name="logger">The logging class to use for output</param>
	public ByteVirtualMachine(bool isDebug, IMemoryAccess<byte> memoryAccess, IVirtualDisplay<byte> virtualDisplay,
		IFileSystemStrategy fileSystemStrategy, IVirtualProcessorStrategy vCpu, ILogger logger)
		: this(isDebug, memoryAccess, virtualDisplay, fileSystemStrategy, logger)
	{
		_vCpu = vCpu ?? throw new ArgumentNullException(nameof(vCpu));
	}

	public ByteVirtualMachine(bool isDebug, IMemoryAccess<byte> memoryAccess, IVirtualDisplay<byte> virtualDisplay,
		IFileSystemStrategy fileSystemStrategy, IVirtualProcessorStrategyAsync vAsyncCpu, ILogger logger)
		: this(isDebug, memoryAccess, virtualDisplay, fileSystemStrategy, logger)
	{
		_vAsyncCpu = vAsyncCpu ?? throw new ArgumentNullException(nameof(vAsyncCpu));
	}

	/// <summary>
	/// Internal constructor, taking common items for the VM 
	/// </summary>
	/// <param name="isDebug">Determines whether the machine is running in "debug" mode</param>
	/// <param name="memoryAccess">The class implementing memory access for the machine</param>
	/// <param name="virtualDisplay">The class implementing a virtual display</param>
	/// <param name="fileSystemStrategy">The class implementing <see cref="IFileSystemStrategy"/> for the machine</param>
	/// <param name="logger">The logging class to use for output</param>
	/// <exception cref="ArgumentNullException"></exception>
	private ByteVirtualMachine(bool isDebug, IMemoryAccess<byte> memoryAccess, IVirtualDisplay<byte> virtualDisplay,
		IFileSystemStrategy fileSystemStrategy, ILogger logger)
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
		//	Run chooses to run non-async variant first, but if not found, will attempt to run the async version in a non-async manner
		if (_vCpu is not null)
		{
			RunCheck();
			_vCpu.Run();
		}
		else
		{
			Task.Run(() => RunAsync(CancellationToken.None));
		}
	}

	/// <inheritdoc/>
	/// <remarks>
	/// If there is no async CPU present, an exception is thrown
	/// </remarks>
	public async Task RunAsync(CancellationToken token)
	{
		if (_vAsyncCpu is null)
			throw new NotImplementedException("No implementation of virtual CPU is present");

		RunCheck();
		await _vAsyncCpu.RunAsync(token);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Performs a check to see if the CPU is ready to run
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>
	/// The system should have called the <see cref="Boot()"/> method prior to calling the <see cref="Run"/> or
	/// <see cref="RunAsync"/> methods to ensure a program has been loaded into memory for the CPU. If a program is
	/// present, then the method outputs the "startup" message and returns
	/// </remarks>
	private void RunCheck()
	{
		if (!_bootstrapped)
			throw new InvalidOperationException("VirtualMachine is not bootstrapped");

		_logger.Log(LogLevel.Screen, "Starting landing computer running York's Obscenely Dumb Architecture (YODA) - Release Build 12x.11g-34 + Anti-gravity module");
		_logger.Log(LogLevel.Screen, $"Folder Path: {_fileSystemStrategy.Folder}\n");
		_logger.Log(LogLevel.Screen, "Connecting to Engine Control System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "Connecting to Landing Control System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "Connecting to Interplanetary Communication System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "All systems are GO!");
	}

	#endregion
}