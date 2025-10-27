using YodaAssembler.Records;

namespace YodaAssembler.Interfaces;

public interface ICompiler
{
	/// <summary>
	/// Using the program source in <paramref name="lines"/>, generate the bytecode representation for it
	/// </summary>
	/// <param name="lines">The program source</param>
	void Compile(IEnumerable<string> lines);
	
	/// <summary>
	/// Using the source found in <paramref name="fileName"/>, generate the bytecode representation for it
	/// </summary>
	/// <param name="fileName">The name of the file containing the source for the program. May be a filename, a relative-path filename or absolute location on the filesystem</param>
	void Compile(string fileName);

	/// <summary>
	/// With the tokenised version of the source, represented as <paramref name="tokens"/>, convert to an intermediate stage that contains the tokens and bytecode representation for it.
	/// </summary>
	/// <param name="tokens">The tokenised version of the sourcecode</param>
	/// <returns>An intermediate stage of compiled tokens, with details held in the <see cref="YodaTokenByteCode"/></returns>
	/// <remarks>The intermediate stage is ready for assembly into the final bytecode, but is not yet fully sanitised by
	/// the compiler. Elements within the output may result in overwriting of other elements. The <see cref="Compile"/>
	/// methods will handle the checking of the elements to ensure that they are valid prior to writing the finalised
	/// version of the bytecode, which will be correctly aligned etc.</remarks>
	IEnumerable<YodaTokenByteCode> CompileToByteCode(IEnumerable<YodaToken> tokens);
}