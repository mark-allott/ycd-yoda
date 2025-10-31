using YodaAssembler.Enums;
using YodaAssembler.Interfaces;

namespace YodaAssembler.Records;

public record YodaTokenByteCode
	: ITokenByteCode
{
	#region ITokenByteCode implementation

	/// <inheritdoc />
	public int LineNumber { get; private init; }

	/// <inheritdoc />
	public int MemoryLocation { get; private init; }

	/// <inheritdoc />
	public int NextLocation => MemoryLocation + Bytes.Length;

	/// <inheritdoc />
	public DirectiveType DirectiveType { get; private init; }

	/// <inheritdoc />
	public IReadOnlyList<IToken> Tokens => _tokens.AsReadOnly();

	/// <inheritdoc />
	public byte?[] Bytes { get; private init; }

	#endregion

	#region Fields

	private readonly List<IToken> _tokens;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for the bytecode data
	/// </summary>
	/// <param name="lineNumber">The line number of the source code</param>
	/// <param name="memoryLocation">The starting memory location for the bytes</param>
	/// <param name="directiveType">The type of directive handling this set of tokens</param>
	/// <param name="tokens">The tokens that make up this set of instructions on the line of source code</param>
	/// <param name="generatorStrategy">The strategy used to create the actual bytecode</param>
	/// <remarks>
	/// The only valid directives that should be used to write bytecode records are <see cref="DirectiveType.Program"/>
	/// and <see cref="DirectiveType.Data"/>. Any other directive type passing token data is invalid as they do not
	/// result in bytecode being generated
	/// </remarks>
	protected YodaTokenByteCode(int lineNumber, int memoryLocation, DirectiveType directiveType,
		IEnumerable<IToken> tokens, IByteCodeGeneratorStrategy generatorStrategy)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lineNumber, 0, nameof(lineNumber));
		ArgumentOutOfRangeException.ThrowIfLessThan(memoryLocation, 0, nameof(memoryLocation));
		ArgumentOutOfRangeException.ThrowIfGreaterThan(memoryLocation, 255, nameof(memoryLocation));
		if (directiveType is not (DirectiveType.Program or DirectiveType.Data))
			throw new ArgumentOutOfRangeException(nameof(directiveType), directiveType,
				$"Invalid directive for bytecode generation");
		ArgumentNullException.ThrowIfNull(tokens);
		ArgumentNullException.ThrowIfNull(generatorStrategy);

		LineNumber = lineNumber;
		MemoryLocation = memoryLocation;
		_tokens = tokens.ToList();
		DirectiveType = directiveType;
		Bytes = generatorStrategy.Generate(directiveType, _tokens);
	}

	public static YodaTokenByteCode Code(int lineNumber, int memoryLocation, IEnumerable<IToken> tokens,
		IByteCodeGeneratorStrategy generatorStrategy)
		=> new YodaTokenByteCode(lineNumber, memoryLocation, DirectiveType.Program, tokens, generatorStrategy);

	public static YodaTokenByteCode Data(int lineNumber, int memoryLocation, IEnumerable<IToken> tokens,
		IByteCodeGeneratorStrategy generatorStrategy)
		=> new YodaTokenByteCode(lineNumber, memoryLocation, DirectiveType.Data, tokens, generatorStrategy);

	#endregion
}