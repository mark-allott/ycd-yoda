namespace SimpleInstructionMachine.Interfaces;

public interface IFileSystemStrategy
{
	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="filename"/>
	/// </summary>
	/// <param name="filename">The name to be given to the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	void SaveToFile(string filename, byte[] contents);
	
	/// <summary>
	/// Returns the contents of the file named <paramref name="filename"/> from the virtual file system
	/// </summary>
	/// <param name="filename">The name of the file to be read</param>
	/// <returns>The contents of the specified file</returns>
	byte[] LoadFromFile(string filename);
	
	/// <summary>
	/// Performs the writing of the specified <paramref name="contents"/> to the underlying virtual file system with the
	/// specified <paramref name="filename"/> with async support
	/// </summary>
	/// <param name="filename">The name to be given to the file</param>
	/// <param name="contents">The contents of the file to be written</param>
	/// <param name="token">The cancellation token</param>
	/// <returns></returns>
	Task SaveToFileAsync(string filename, byte[] contents, CancellationToken token);
	
	/// <summary>
	/// Returns the contents of the file named <paramref name="filename"/> from the virtual file system
	/// </summary>
	/// <param name="filename">The name of the file to be read</param>
	/// <param name="token">The cancellation token</param>
	/// <returns>The contents of the specified file</returns>
	Task<byte[]> LoadFromFileAsync(string filename, CancellationToken token);
}