namespace SimpleInstructionMachine.Interfaces;

public interface IFileSystem<T>
	where T : struct
{
	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="fileNumber"/>
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	void SaveToFile(int fileNumber, T[] contents);

	/// <summary>
	/// Returns the contents of the file named <paramref name="fileNumber"/> from the virtual file system
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <returns>The contents of the specified file</returns>
	T[] LoadFromFile(int fileNumber);

	/// <summary>
	/// Returns the contents of the bootfile from the virtual file system
	/// </summary>
	/// <returns>The contents of the specified file</returns>
	T[] LoadBootFile();

	/// <summary>
	/// Writes the memory <paramref name="contents"/> to the crash dump file in binary format
	/// </summary>
	/// <param name="contents">The memory contents to dump</param>
	void WriteBinaryCrashDump(T[] contents);

	/// <summary>
	/// Writes the memory <paramref name="contents"/> to the crash dump file in text format
	/// </summary>
	/// <param name="contents">The memory contents to dump</param>
	/// <param name="instructionPointer">The current location of the instruction pointer</param>
	void WriteTextCrashDump(T[] contents, int instructionPointer);

	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="fileNumber"/> with async support
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	/// <param name="token">The cancellation token</param>
	/// <returns></returns>
	Task SaveToFileAsync(int fileNumber, T[] contents, CancellationToken token);

	/// <summary>
	/// Returns the contents of the file named <paramref name="fileNumber"/> from the virtual file system
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>The contents of the specified file</returns>
	Task<T[]> LoadFromFileAsync(int fileNumber, CancellationToken token);

	/// <summary>
	/// Returns the contents of the bootfile from the virtual file system with async support
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <returns>The contents of the specified file</returns>
	Task<T[]> LoadBootFileAsync(CancellationToken token);

	/// <summary>
	/// Writes the memory <paramref name="contents"/> to the crash dump file in binary format with async support
	/// </summary>
	/// <param name="contents">The memory contents to dump</param>
	/// <param name="token">The cancellation token</param>
	Task WriteBinaryCrashDumpAsync(T[] contents, CancellationToken token);

	/// <summary>
	/// Writes the memory <paramref name="contents"/> to the crash dump file in text format with async support
	/// </summary>
	/// <param name="contents">The memory contents to dump</param>
	/// <param name="instructionPointer">The current location of the instruction pointer</param>
	/// <param name="token">The cancellation token</param>
	Task WriteTextCrashDumpAsync(T[] contents, int instructionPointer, CancellationToken token);

	/// <summary>
	/// Provides the name of the file system's folder
	/// </summary>
	string Folder { get; }

	/// <summary>
	/// Provides the name of the boot file
	/// </summary>
	string BootFileName { get; }

	/// <summary>
	/// Provides the name of the binary crash dump file
	/// </summary>
	string BinaryCrashDumpFileName { get; }

	/// <summary>
	/// Provides the name of the text crash dump file
	/// </summary>
	string TextCrashDumpFileName { get; }
}