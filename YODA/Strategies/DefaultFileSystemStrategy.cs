using SimpleInstructionMachine.Interfaces;

namespace SimpleInstructionMachine.Strategies;

public class DefaultFileSystemStrategy
	: IFileSystemStrategy
{
	#region Fields

	private const string BootfileName = "boot";

	#endregion
	#region Properties

	/// <summary>
	/// The fully-qualified path of the virtual file system
	/// </summary>
	public string Folder { get; private set; }

	/// <summary>
	/// The naming strategy to be used for filenames
	/// </summary>
	public IFileNameStrategy FileNameStrategy { get; }

	#endregion

	#region Constructors

	/// <summary>
	/// Default constructor - uses current working directory for virtual file system
	/// </summary>
	public DefaultFileSystemStrategy()
		: this(".", new DefaultFileNameStrategy())
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="folderName"></param>
	/// <param name="fileNameStrategy"></param>
	/// <exception cref="DirectoryNotFoundException"></exception>
	public DefaultFileSystemStrategy(string folderName, IFileNameStrategy fileNameStrategy)
	{
		if (!Directory.Exists(folderName))
			throw new DirectoryNotFoundException(folderName);

		//	Locate the fully-qualified name of the folder from folderName: 
		Folder = Path.GetFullPath(folderName);
		//	Set the naming strategy
		FileNameStrategy = fileNameStrategy;
	}

	#endregion

	#region IFileSystemStrategy Members

	/// <inheritdoc/>
	public void SaveToFile(int fileNumber, byte[] contents)
	{
        CheckFileNumber(fileNumber);
        CheckContents(contents);
        Task.Run(() => SaveToFileAsync(fileNumber, contents, CancellationToken.None));
	}

	/// <inheritdoc/>
	public byte[] LoadFromFile(int fileNumber)
	{
        CheckFileNumber(fileNumber);
        return Task.Run(() => LoadFromFileAsync(fileNumber, CancellationToken.None)).Result;
	}

	/// <inheritdoc/>
	public byte[] LoadBootFile()
	{
		return Task.Run(() => LoadBootFileAsync(CancellationToken.None)).Result;
	}
	
	/// <inheritdoc/>
	public async Task SaveToFileAsync(int fileNumber, byte[] contents, CancellationToken token)
	{
        CheckFileNumber(fileNumber);
        CheckContents(contents);
        
        //	If the cancellation token is already set, return now
        if (token.IsCancellationRequested)
	        return;

        var fileName = Path.Combine(Folder, GetFileName(fileNumber));
		await File.WriteAllBytesAsync(fileName, contents, token);
	}

	/// <inheritdoc/>
	public async Task<byte[]> LoadFromFileAsync(int fileNumber, CancellationToken token)
	{
        CheckFileNumber(fileNumber);
        //	If the cancellation token is already set, return now
        if (token.IsCancellationRequested)
	        return [];
        return await InternalLoadFromFileAsync(GetFileName(fileNumber), token);
	}

	public async Task<byte[]> LoadBootFileAsync(CancellationToken token)
	{
		return await InternalLoadFromFileAsync(BootfileName, token);
	}
	
	#endregion

	#region Methods

	/// <summary>
	/// Ensures the <paramref name="fileNumber"/> is in the valid range
	/// </summary>
	/// <param name="fileNumber">The file number for a load / save operation</param>
	private static void CheckFileNumber(int fileNumber)
	{
		//	fileNumber >=0 && <= 15
		ArgumentOutOfRangeException.ThrowIfLessThan(fileNumber, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(fileNumber, 15);
	}

	/// <summary>
	/// Ensures there are some contents to save to the file system
	/// </summary>
	/// <param name="contents">The contents of a file to be written</param>
	private static void CheckContents(byte[] contents)
	{
		//	Contents must be present
		ArgumentNullException.ThrowIfNull(contents);
		ArgumentOutOfRangeException.ThrowIfLessThan(contents.Length, 1);
	}

	/// <summary>
	/// Gets the name of the file from the <see cref="FileNameStrategy"/>
	/// </summary>
	/// <param name="fileNumber">The number of the file</param>
	/// <returns>The file name</returns>
	private string GetFileName(int fileNumber)
	{
		return FileNameStrategy.GetFileName((byte)fileNumber);
	}

	private async Task<byte[]> InternalLoadFromFileAsync(string filename, CancellationToken token)
	{
		//	If the cancellation token is already set, return now
		if (token.IsCancellationRequested)
			return [];
		return await File.ReadAllBytesAsync(Path.Combine(Folder, filename), token);
	}
	#endregion
}