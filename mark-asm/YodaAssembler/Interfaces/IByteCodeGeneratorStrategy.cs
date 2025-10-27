using YodaAssembler.Enums;
using YodaAssembler.Records;

namespace YodaAssembler.Interfaces;

public interface IByteCodeGeneratorStrategy
{
	/// <summary>
	/// Used to generate byte code specific to a particular virtual CPU
	/// </summary>
	/// <param name="directiveType">The type of directive the <paramref name="tokens"/> belong to</param>
	/// <param name="tokens">The <see cref="YodaToken"/>, or derivative, tokens to be converted to their bytecode representation</param>
	/// <returns>The array of bytes that represent the tokens</returns>
	/// <remarks>If the <paramref name="tokens"/> contain any symbols, then the byte returned for it shall be null</remarks>
	byte?[] Generate(DirectiveType directiveType, IEnumerable<YodaToken> tokens);
}