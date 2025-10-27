using YodaAssembler.Processor;

namespace YodaAssembler.Interfaces;

public interface IParser<out T>
{
	/// <summary>
	/// Parses the supplied <paramref name="lines"/> into tokenised values
	/// </summary>
	/// <param name="lines">The sourcecode to be parsed</param>
	IEnumerable<T> Parse(IEnumerable<string> lines);
	
	/// <summary>
	/// Parses the file located at <paramref name="filePath"/> into tokenised values
	/// </summary>
	/// <param name="filePath"></param>
	IEnumerable<T> ParseFile(string filePath);
}