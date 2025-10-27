using System.Text.RegularExpressions;
using YodaAssembler.Enums;
using YodaAssembler.Exceptions;
using YodaAssembler.Extensions;
using YodaAssembler.Interfaces;
using YodaAssembler.Records;

namespace YodaAssembler.Strategies;

public partial class YodaTokeniserStrategy
	: ITokeniserStrategy<YodaToken>
{
	#region Private Members

	[GeneratedRegex(@""".*""|'\\?.'|\[\[\w+\]\]|\[\w+\]|\w+", RegexOptions.Compiled)]
	private static partial Regex ParameterSplitterRegex();

	[GeneratedRegex(@"^\s*([A-Za-z_]\w{0,31})\s*=\s*(\S*)(\s*;\s*(.*))?$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	private static partial Regex ConstantSplitterRegex();

	private readonly YodaCommand[] _commands;

	/// <summary>
	/// Data can be represented by numeric, literal string, literal character or symbol types
	/// </summary>
	private static readonly YodaCommand DataCommand = new YodaCommand(0xff, "[DATA]", -1,
		[ ParameterTypes.LiteralNumber | ParameterTypes.LiteralString | ParameterTypes.LiteralChar | ParameterTypes.Symbol]);

	/// <summary>
	/// Constants are of form <c>x = y</c>, where x is a symbol and y can be numeric, literal character or another symbol.
	/// The statement may optionally be terminated with an inline comment
	/// </summary>
	/// <remarks>Examples of valid constant declarations are:
	/// <code>
	/// [CONST]
	/// decimalNumber = 0
	/// hexNumber     = 0xcd
	/// binaryNumber  = 0b0101_1010
	/// binaryNumber  = 0b01011010
	/// binaryNumber  = 0b10101
	/// characterA    = 'A' ; with a comment
	/// </code></remarks>
	private static readonly YodaCommand ConstCommand = new YodaCommand(0xff, "[CONST]", 1,
		[ ParameterTypes.LiteralNumber | ParameterTypes.LiteralChar | ParameterTypes.Symbol]);

	#endregion

	#region Constructors

	public YodaTokeniserStrategy(IEnumerable<YodaCommand> commands)
	{
		ArgumentNullException.ThrowIfNull(commands);
		_commands = commands.ToArray();
	}

	#endregion

	#region ITokeniserStrategy<YodaToken> Members

	public IEnumerable<YodaToken> Tokenise(IEnumerable<string> text)
	{
		return Tokenise(text.Select((l, i) => new SourceLine(i + 1, l)));
	}

	/// <summary>
	/// Takes the <paramref name="source"/> of the program and outputs a tokenised version of it for later transformation into bytecode
	/// </summary>
	/// <param name="source">The source of the program</param>
	/// <returns>The tokenised representation of <paramref name="source"/></returns>
	/// <exception cref="DirectiveException"></exception>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <remarks>
	/// <para>When processing the source, the code itself MUST adhere to certain rules:</para>
	/// <ul>
	/// <li>All code, symbols, constants etc. MUST be contained in their appropriate directive block</li>
	/// <li>The <c>[PROG]</c> directive may have an optional memory location associated with it and optional inline comment</li>
	/// <li>The <c>[DATA]</c> directive may have an optional memory location and inline comment</li>
	/// <li>The <c>[CONSTANTS]</c> directive may ONLY have an inline comment</li>
	/// <li>Blank lines and comment blocks <i>may</i> exist outside defined directives - e.g. the start of a program may consist of comments and blank lines before encountering a directive</li>
	/// <li>Labels must be able to be allocated a memory location, therefore are only permitted within the <c>[CODE]</c> and <c>[DATA]</c> directive blocks</li>
	/// <li>Constants / symbols defined within the <c>[CONSTANTS]</c> block should be of the form <c>x = y</c></li>
	/// </ul>
	/// </remarks>
	public IEnumerable<YodaToken> Tokenise(IEnumerable<SourceLine> source)
	{
		var tokens = new List<YodaToken>();
		var lines = source.ToList();

		//	Extract all lines that contain directives
		var directiveLines = lines.Where(q => q.IsDirective)
			.OrderByDescending(o => o.Directive)
			.ToList();

		//	There MUST be a [Program] directive (or equivalent) present in the source
		if (directiveLines.All(a => a.Directive != DirectiveType.Program))
			throw new DirectiveException(DirectiveType.Program, $"No definition for the [{DirectiveType.Program}]");

		var currentDirective = DirectiveType.Unknown;

		//	Process each line sequentially
		foreach (var line in lines)
		{
			if (line.Handled)
				continue;

			var lineSequence = -1;

			//	Check for full line tokens first
			if (line.IsBlank)
			{
				tokens.Add(YodaToken.Blank(line.LineNumber, 0));
			}
			else if (line.IsComment)
			{
				tokens.Add(YodaToken.Comment(line.LineNumber, 0, line.Text));
			}
			//	Check that labels are positioned correctly
			else if (line.IsLabel)
			{
				//	Labels are only valid within program/code and data definition blocks
				if (currentDirective is not (DirectiveType.Program or DirectiveType.Data))
					throw new TokeniserException(line, "Invalid label definition");

				//	Grab the label details and tokenise
				var (label, comment) = line.Text.GetLabelDetail();
				tokens.Add(YodaToken.Label(line.LineNumber, ++lineSequence, $":{label}"));
				if (comment is not null)
					tokens.Add(YodaToken.Comment(line.LineNumber, ++lineSequence, $";{comment}"));
			}
			//	Check for a directive or directive change
			else if (line.IsDirective)
			{
				tokens.AddRange(HandleDirectiveChange(line, out currentDirective));
			}
			else
				switch (currentDirective)
				{
					//	continue tokenising program/code
					case DirectiveType.Program:
						tokens.AddRange(TokeniseCommand(line));
						break;
					//	tokenise data elements - must be literal values
					case DirectiveType.Data:
						tokens.AddRange(TokeniseParameters(line.Text, DataCommand, line, -1));
						break;
					case DirectiveType.Constants:
						tokens.AddRange(TokeniseConstant(line));
						break;
					//	If processing somehow manages to get here, then stop immediately as there is something seriously screwy happening
					default:
						throw new DirectiveException(currentDirective, line.LineNumber, line.Text,
							$"Unexpected directive [{currentDirective}]");
				}

			line.SetHandled();
		}

		//	Double-check: if all lines are handled, return the tokens
		if (lines.All(l => l.Handled))
			return tokens;

		//	There's something that was unhandled, so report it
		var unhandledLine = lines.First(l => !l.Handled);
		throw new TokeniserException(unhandledLine, "Line was unprocessed");
	}

	#endregion

	#region Private methods

	/// <summary>
	/// Handle changes to the current directive - i.e. from Unknown to Code, etc.
	/// </summary>
	/// <param name="line">The line of code in the source</param>
	/// <param name="currentDirective"></param>
	/// <returns></returns>
	/// <exception cref="DirectiveException"></exception>
	/// <exception cref="TokeniserException"></exception>
	private List<YodaToken> HandleDirectiveChange(SourceLine line, out DirectiveType currentDirective)
	{
		var newTokens = new List<YodaToken>();
		(currentDirective, var parameter, var comment) = line.GetDirectiveDetail();

		//	Unexpected directives stop processing immediately
		if (currentDirective is not (DirectiveType.Program or DirectiveType.Data or DirectiveType.Constants))
			throw new DirectiveException(currentDirective, line.LineNumber, line.Text,
				$"Unexpected directive [{currentDirective}]");

		//	Using the parameter returned from GetDirectiveDetail, attempt to split into multiple parameters
		var parameters = ParameterSplitterRegex().Matches(parameter ?? string.Empty);

		//	If declaring Constants, no parameters are permitted
		if (currentDirective is DirectiveType.Constants && parameters.Count > 0)
			throw new DirectiveException(currentDirective, line.LineNumber, null,
				$"[{currentDirective}] does not permit a parameter");

		//	If more than one parameter is found, then throw an exception
		if (parameters.Count > 1)
			throw new DirectiveException(currentDirective,
				$"Multiple parameters for the [{currentDirective}] directive");

		var sequence = -1;

		//	Add the directive token
		newTokens.Add(YodaToken.Directive(line.LineNumber, ++sequence, $"[{currentDirective}]"));

		//	Add an optional parameter - must be either a number or symbol
		if (parameters.Count > 0)
		{
			var p0 = parameters[0].Value;
			if (TokenRegex.LiteralNumber().IsMatch(p0))
				newTokens.Add(YodaToken.LiteralNumber(line.LineNumber, ++sequence, p0));
			else if (TokenRegex.GenericWord().IsMatch(p0))
				newTokens.Add(YodaToken.Symbol(line.LineNumber, ++sequence, p0));
			else
				throw new TokeniserException(line.LineNumber, p0, $"Unknown parameter '{p0}' for [{currentDirective}]");
		}

		//	Add an optional comment
		if (comment is not null)
			//	GetDirectiveDetail removes the semicolon, so it needs to be added back in
			newTokens.Add(YodaToken.Comment(line.LineNumber, ++sequence, $";{comment}"));

		//	return the newly created tokens for the directive change
		return newTokens;
	}

	/// <summary>
	/// Tokenises the value of <paramref name="text"/> into parameter tokens of the appropriate types
	/// </summary>
	/// <param name="text">The text representing all parameters</param>
	/// <param name="command">The virtual CPU command being handled</param>
	/// <param name="line">The current line of source being parsed/tokenised</param>
	/// <param name="lineSequence">The seed value for the line sequence, the first token will the one more than this value</param>
	/// <returns>A list of <see cref="YodaToken"/> records representing the parameters for the command</returns>
	/// <exception cref="TokeniserException"></exception>
	private List<YodaToken> TokeniseParameters(string text, YodaCommand command, SourceLine line, int lineSequence = 0)
	{
		//	If the command expects no parameters, but there are parameters supplied, throw an exception
		if (command.ParameterCount == 0 && !string.IsNullOrWhiteSpace(text))
			throw new TokeniserException(line, $"{command.Mnemonic} expects no parameters, but '{text}' is present");

		//	Split parameters into individual elements
		var paramMatches = ParameterSplitterRegex().Matches(text);

		//	Quick check for parameter count mismatches (-1 indicates a variable number of optional parameters)
		if (command.ParameterCount >= 0 &&
		    paramMatches.Count != command.ParameterCount)
			throw new TokeniserException(line,
				$"{command.Mnemonic} expects {command.ParameterCount} parameters but found {paramMatches.Count}");

		//	Process parameters to determine if they are of the correct types
		var paramTokens = new List<YodaToken>();
		foreach (Match paramMatch in paramMatches)
		{
			var paramText = paramMatch.Value;
			Func<int, int, string, YodaToken> fn;
			if (TokenRegex.LiteralString().IsMatch(paramText))
				fn = YodaToken.LiteralString;
			else if (TokenRegex.LiteralChar().IsMatch(paramText))
				fn = YodaToken.LiteralChar;
			else if (TokenRegex.LiteralNumber().IsMatch(paramText))
				fn = YodaToken.LiteralNumber;
			else if (TokenRegex.GenericWord().IsMatch(paramText))
				fn = YodaToken.Symbol;
			else if (TokenRegex.IndirectNumber().IsMatch(paramText))
				fn = YodaToken.IndirectNumber;
			else if (TokenRegex.IndirectSymbol().IsMatch(paramText))
				fn = YodaToken.IndirectSymbol;
			else if (TokenRegex.DirectNumber().IsMatch(paramText))
				fn = YodaToken.DirectNumber;
			else if (TokenRegex.DirectSymbol().IsMatch(paramText))
				fn = YodaToken.DirectSymbol;
			else
				throw new TokeniserException(line, $"Invalid parameter '{paramText}'");
			paramTokens.Add(fn(line.LineNumber, ++lineSequence, paramText));
		}

		//	No tokens to check, so return quickly
		if (command.ParameterCount == 0 || paramTokens.Count == 0)
			return paramTokens;

		//	Verify the tokenised parameters match the expected types in the correct locations
		for (int i = 0; i < paramTokens.Count; i++)
		{
			var index = command.ParameterCount == -1
				? 0
				: i;
			if (!command.ParameterTypes[index].HasFlag(paramTokens[i].ParameterType))
				throw new TokeniserException(line,
					$"Parameter {i + 1} has an invalid parameter type '{paramTokens[i].ParameterType}'");
		}

		return paramTokens;
	}

	/// <summary>
	/// Takes the source in <paramref name="line"/> and tokenises, where possible, into the command, parameters and comments
	/// </summary>
	/// <param name="line">The source to be tokenised</param>
	/// <returns>A list of tokens representing the code</returns>
	/// <exception cref="TokeniserException"></exception>
	private List<YodaToken> TokeniseCommand(SourceLine line)
	{
		//	Extract the elements from the source text:
		//		First "word" should be a command
		//		Everything else before a semicolon should be a parameter, or parameters
		//		Everything after a semicolon should be a comment 
		var (word, parameters, comment) = line.Text.GetGenericDetail();

		//	Attempt to find the text for the command in the supplied command set 
		var command = _commands.FirstOrDefault(c => c.IsMatch(word));

		//	Unknown command throws an exception now
		if (command is null)
			throw new TokeniserException(line, $"'{word}' is not a valid command");

		//	Tokenise any parameters we have for the command - checking they are required / of correct type etc.
		var paramTokens = TokeniseParameters(parameters ?? string.Empty, command, line);

		//	Good so far, create a temp container for the various new tokens, adding the command
		var cmdTokens = new List<YodaToken>([YodaToken.Command(line.LineNumber, 0, word)]);
		cmdTokens.AddRange(paramTokens);
		//	If a comment is present, add it before things are wrapped up
		if (comment is not null)
			cmdTokens.Add(YodaToken.Comment(line.LineNumber, paramTokens.Count + 1, $";comment"));

		return cmdTokens;
	}

	private List<YodaToken> TokeniseConstant(SourceLine line)
	{
		//	Try to match parts in the line text with an expected constant declaration regex
		var m = ConstantSplitterRegex().Match(line.Text);

		if (!m.Success)
			throw new TokeniserException(line, "Invalid constant declaration");
		
		//	Split the matches into their various parts for easier tracking
		var symbol = m.Groups[1].Value;
		var valueString = m.Groups[2].Value;
		var comment = m.Groups[3].Success 
			? m.Groups[3].Value 
			: string.Empty;

		//	Attempt to break the value into a parameter token, checking it will conform to permitted types
		var valueTokens = TokeniseParameters(valueString, ConstCommand, line);

		//	Seems to be good so far, add the initial token for the symbol
		var constTokens = new List<YodaToken>([YodaToken.Symbol(line.LineNumber, 0, symbol)]);
		//	Add the value
		constTokens.AddRange(valueTokens);
		//	Add any optional comment
		if(!string.IsNullOrWhiteSpace(comment))
			constTokens.Add(YodaToken.Comment(line.LineNumber, 2, $";{comment}"));
		return constTokens;
	}

	#endregion
}