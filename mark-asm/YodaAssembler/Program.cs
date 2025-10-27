using System.Reflection;

namespace YodaAssembler;

internal static class Program
{
    private static string _filePath = null!;

    internal static void Main(string[] args)
    {
        //	Get individual path parts to the assembly and where the assembly is in the list
        var pathParts = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "")
            .Split(Path.DirectorySeparatorChar)
            .ToList();
        var assemblyIndex = pathParts.IndexOf(nameof(YodaAssembler));
        //	Reassemble the paths up to the point of the assembly name
        var basePath = string.Join(Path.DirectorySeparatorChar, pathParts[..assemblyIndex]);
        //	Append Files to the path - this is the location of the boot file area
        _filePath = Path.Combine(basePath, "Files");

        //	Make sure the Files folder exists
        if (!Directory.Exists(_filePath))
            Directory.CreateDirectory(_filePath);
    }
}
