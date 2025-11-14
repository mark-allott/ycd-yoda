namespace SimpleInstructionMachine.Interfaces;

public interface IFileSystemStrategy
{
	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="fileNumber"/>
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	void SaveToFile(int fileNumber, byte[] contents);

	/// <summary>
	/// Returns the contents of the file named <paramref name="fileNumber"/> from the virtual file system
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <returns>The contents of the specified file</returns>
	byte[] LoadFromFile(int fileNumber);

	/// <summary>
	/// Returns the contents of the bootfile from the virtual file system
	/// </summary>
	/// <returns>The contents of the specified file</returns>
	byte[] LoadBootFile();

	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="fileNumber"/> with async support
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	/// <param name="token">The cancellation token</param>
	/// <returns></returns>
	Task SaveToFileAsync(int fileNumber, byte[] contents, CancellationToken token);

	/// <summary>
	/// Returns the contents of the file named <paramref name="fileNumber"/> from the virtual file system
	/// </summary>
	/// <param name="fileNumber">The number for the file</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>The contents of the specified file</returns>
	Task<byte[]> LoadFromFileAsync(int fileNumber, CancellationToken token);

	/// <summary>
	/// Returns the contents of the bootfile from the virtual file system with async support
	/// </summary>
	/// <param name="token">The cancellation token</param>
	/// <returns>The contents of the specified file</returns>
	Task<byte[]> LoadBootFileAsync(CancellationToken token);
}