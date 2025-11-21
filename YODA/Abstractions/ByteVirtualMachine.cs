using SimpleInstructionMachine.Enums;
using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Abstractions;

public class ByteVirtualMachine
	: IVirtualMachine<byte>
{
	#region Fields

	private readonly IMemoryAccess<byte> _memoryAccess;
	private readonly IFileSystem<byte> _fileSystem;
	private readonly IVirtualProcessorStrategy? _vCpu = null;
	private readonly IVirtualProcessorStrategyAsync? _vAsyncCpu = null;
	private readonly ILogger _logger;
	private bool _bootstrapped;

	#endregion

	#region Constructors

	/// <summary>
	/// Alternate constructor allowing specification of a virtual cpu to run in a non-async manner
	/// </summary>
	/// <param name="isDebug">Determines whether the machine is running in "debug" mode</param>
	/// <param name="memoryAccess">The class implementing memory access for the machine</param>
	/// <param name="fileSystem">The class implementing <see cref="IFileSystem{T}"/> for the machine</param>
	/// <param name="vCpu">The class implementing the virtual CPU in a non-async runtime manner</param>
	/// <param name="logger">The logging class to use for output</param>
	public ByteVirtualMachine(bool isDebug, IMemoryAccess<byte> memoryAccess, IFileSystem<byte> fileSystem,
		IVirtualProcessorStrategy vCpu, ILogger logger)
		: this(memoryAccess, fileSystem, logger)
	{
		_vCpu = vCpu ?? throw new ArgumentNullException(nameof(vCpu));
	}

	/// <summary>
	/// Alternate constructor allowing specification of a virtual cpu to run in an async manner
	/// </summary>
	/// <param name="isDebug">Determines whether the machine is running in "debug" mode</param>
	/// <param name="memoryAccess">The class implementing memory access for the machine</param>
	/// <param name="fileSystem">The class implementing <see cref="IFileSystem{T}"/> for the machine</param>
	/// <param name="vAsyncCpu">The class implementing the virtual CPU in an async runtime manner</param>
	/// <param name="logger">The logging class to use for output</param>
	/// <exception cref="ArgumentNullException"></exception>
	public ByteVirtualMachine(bool isDebug, IMemoryAccess<byte> memoryAccess, IFileSystem<byte> fileSystem,
		IVirtualProcessorStrategyAsync vAsyncCpu, ILogger logger)
		: this(memoryAccess, fileSystem, logger)
	{
		_vAsyncCpu = vAsyncCpu ?? throw new ArgumentNullException(nameof(vAsyncCpu));
	}

	/// <summary>
	/// Internal constructor, taking common items for the VM 
	/// </summary>
	/// <param name="memoryAccess">The class implementing memory access for the machine</param>
	/// <param name="fileSystem">The class implementing <see cref="IFileSystem{T}"/> for the machine</param>
	/// <param name="logger">The logging class to use for output</param>
	/// <exception cref="ArgumentNullException"></exception>
	private ByteVirtualMachine(IMemoryAccess<byte> memoryAccess, IFileSystem<byte> fileSystem, ILogger logger)
	{
		_memoryAccess = memoryAccess ?? throw new ArgumentNullException(nameof(memoryAccess));
		_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	#endregion

	#region IVirtualMachine Members

	/// <inheritdoc/>
	public void Boot()
	{
		//	Load the file from the filesystem and bootstrap using the bytes read from the file
		Boot(_fileSystem.LoadBootFile());
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
		_logger.Log(LogLevel.Screen, $"Folder Path: {_fileSystem.Folder}\n");
		_logger.Log(LogLevel.Screen, "Connecting to Engine Control System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "Connecting to Landing Control System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "Connecting to Interplanetary Communication System.... SUCCESS!");
		_logger.Log(LogLevel.Screen, "All systems are GO!");
	}

	#endregion
}