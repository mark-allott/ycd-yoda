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
	/// Writes the memory <paramref name="contents"/> to the crash dump file in either binary of text format
	/// </summary>
	/// <param name="writeBinary">A flag indicating whether to write the binary or text representation of the crash dump file</param>
	/// <param name="contents">The memory contents to dump</param>
	/// <param name="instructionPointer">The current location of the instruction pointer</param>
	void WriteCrashDump(bool writeBinary, T[] contents, int instructionPointer);

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
	/// Writes the memory <paramref name="contents"/> to the crash dump file in either binary of text format with async support
	/// </summary>
	/// <param name="writeBinary">A flag indicating whether to write the binary or text representation of the crash dump file</param>
	/// <param name="contents">The memory contents to dump</param>
	/// <param name="instructionPointer">The current location of the instruction pointer</param>
	/// <param name="token">The cancellation token</param>
	Task WriteCrashDumpAsync(bool writeBinary, T[] contents, int instructionPointer, CancellationToken token);

	/// <summary>
	/// Provides the name of the file system's folder
	/// </summary>
	string Folder { get; }
}