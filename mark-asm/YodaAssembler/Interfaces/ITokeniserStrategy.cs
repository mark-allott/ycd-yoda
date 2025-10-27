namespace YodaAssembler.Interfaces;

/// <summary>
/// Define a strategy descriptor for turning source code text into tokenised representations
/// </summary>
/// <typeparam name="T">The token used for output, which must implement the <see cref="IToken"/> interface</typeparam>
public interface ITokeniserStrategy<out T>
	where T : class, IToken
{
	/// <summary>
	/// Converts the supplied source <paramref name="text"/> into their equivalent tokens
	/// </summary>
	/// <param name="text">The source to be tokenised</param>
	/// <returns>The token equivalents of the source</returns>
	IEnumerable<T> Tokenise(IEnumerable<string> text);
}