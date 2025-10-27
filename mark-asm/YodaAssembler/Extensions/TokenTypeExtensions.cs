using System.Text.RegularExpressions;

namespace YodaAssembler.Extensions;

public static partial class TokenTypeExtensions
{
	static TokenTypeExtensions()
	{
		DirectivesMapLock = new object();
	}

	#region Directives Handling

	/// <summary>
	/// Defines enum names that are to be excluded from matches
	/// </summary>
	/// <remarks>Additions to the array should be in lowercase</remarks>
	private static readonly string[] Exclusions = ["none", "unknown"];

	/// <summary>
	/// Holds a map of already calculated Regex values for the given type
	/// </summary>
	private static Dictionary<Type, Regex> _directivesMap = new Dictionary<Type, Regex>();

	/// <summary>
	/// Lock object for protecting the directives map in multi-threaded execution
	/// </summary>
	private static readonly object DirectivesMapLock;

	#endregion

	#region Comment handling

	/// <summary>
	/// Determines whether the <paramref name="text"/> is a comment
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>True if the text represents the start of a comment</returns>
	public static bool IsComment(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       TokenRegex.IsComment().Match(text.Trim()).Success;
	}

	/// <summary>
	/// Determines whether the <paramref name="text"/> supplied contains an inline comment character
	/// </summary>
	/// <param name="text">The source text to be checked</param>
	/// <returns>True if the semicolon character is found in <paramref name="text"/></returns>
	public static bool HasComment(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       TokenRegex.HasComment().Match(text).Success;
	}

	/// <summary>
	/// Extracts the comment from the supplied <paramref name="text"/>
	/// </summary>
	/// <param name="text">The text to be checked and a comment extracted from</param>
	/// <returns>The text of the comment (if found), or <c>string.Empty</c></returns>
	public static string GetComment(this string text)
	{
		var m = TokenRegex.HasComment().Match(text.Trim());
		return m.Success
			? m.Groups[1].Value
			: string.Empty;
	}

	#endregion

	#region Directive handling

	/// <summary>
	/// Determines whether the text contains a piece of text that looks like a directive
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>True if a directive pattern is detected</returns>
	/// <remarks></remarks>
	public static bool IsDirective(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       TokenRegex.IsDirective().Match(text).Success;
	}

	/// <summary>
	/// Assembles a regex capable of matching enum names of the supplied type, using pre-defined exclusions
	/// </summary>
	/// <typeparam name="T">The type of the enum to be used for matches</typeparam>
	/// <returns>The assembled regex</returns>
	private static Regex GetValidDirectivesRegex<T>()
		where T : struct, Enum
	{
		//	If already known, return what we have
		Regex result = null!;
		if (_directivesMap.TryGetValue(typeof(T), out result!))
			return result;

		//	Extract all names from the enum, except ones that are like 'none' or 'unknown'
		var enumNames = Enum.GetNames<T>()
			.Select(x => x.ToLowerInvariant())
			.Where(q => !Exclusions.Contains(q))
			.Distinct()
			.ToArray();

		//	Names must all be 20 characters or fewer
		if (enumNames.Any(n => n.Length > 20))
			throw new ArgumentOutOfRangeException(nameof(T), $"An element of {nameof(T)} exceeds 20 characters.");

		//	Names MUST adhere to the required constraints
		if (!enumNames.All(n => TokenRegex.IsDirective().IsMatch($"[{n}]")))
			throw new ArgumentOutOfRangeException(nameof(T),
				$"An element of {nameof(T)} does not match naming constraints.");

		//	Assemble a regex:
		//		Use the enum values that weren't excluded
		//		Allow an optional supplemental parameter for the directive
		//		Allow an optional inline comment after all directive parts
		result = new Regex(@"^\s*\[(" + string.Join('|', enumNames) +
		                   @")\]\s*(\w+)?\s*(;\s*(.*)\s*)?$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

		//	Lock the sync object to make sure only one thread is writing to the dictionary at a time
		lock (DirectivesMapLock)
		{
			_directivesMap.TryAdd(typeof(T), result);
		}

		return result;
	}

	/// <summary>
	/// Determines whether the supplied <paramref name="text"/> contains a directive that matches the specified enum type
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <typeparam name="T">The enum type to specify directive names</typeparam>
	/// <returns>True if a match is located</returns>
	public static bool IsDirective<T>(this string text)
		where T : struct, Enum
	{
		return GetValidDirectivesRegex<T>().IsMatch(text);
	}

	/// <summary>
	/// Extracts the directive name from <paramref name="text"/>, matching against the enum type supplied
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <typeparam name="T">The enum type to specify directive names</typeparam>
	/// <returns>The name of the directive, if a valid match is found, otherwise an empty string</returns>
	public static string GetDirectiveName<T>(this string text)
		where T : struct, Enum
	{
		var m = GetValidDirectivesRegex<T>().Match(text);
		return m.Success
			? m.Groups[1].Value
			: string.Empty;
	}

	/// <summary>
	/// Extracts the directive name and parameter details from <paramref name="text"/>
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <typeparam name="T">The enum type to specify directive names</typeparam>
	/// <returns>The matching directive name and any parameter and comments supplied with it</returns>
	/// <remarks>Any missing elements shall be represented by <c>null</c></remarks>
	public static (string name, string? parameter, string? comment) GetDirectiveParts<T>(this string text)
		where T : struct, Enum
	{
		var m = GetValidDirectivesRegex<T>().Match(text);
		var name = m.Success
			? m.Groups[1].Value
			: null!;
		var parameter = m.Success && m.Groups[2].Success
			? m.Groups[2].Value
			: null;
		var comment = m.Success && m.Groups[4].Success
			? m.Groups[4].Value
			: null;
		return (name, parameter, comment);
	}

	/// <summary>
	/// Extracts the enum value of the directive in <paramref name="text"/>
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <typeparam name="T">The enum type to specify directive names</typeparam>
	/// <returns>The enum value associated with the directive, otherwise the default value for the enum</returns>
	public static T GetDirective<T>(this string text)
		where T : struct, Enum
	{
		var directiveName = GetDirectiveName<T>(text);
		return string.IsNullOrWhiteSpace(directiveName)
			? default
			: Enum.TryParse<T>(directiveName, true, out var result)
				? result
				: default;
	}

	/// <summary>
	/// Extracts the directive value parameter and/or comment details from <paramref name="text"/>
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <typeparam name="T">The enum type to specify directive names</typeparam>
	/// <returns>A <see cref="Tuple{T1,T2,T3}"/> of the enum value associated with the directive, any parameter and comment</returns>
	public static (T directive, string? parameter, string? comment) GetDirectiveDetail<T>(this string text)
		where T : struct, Enum
	{
		var (name, parameter, comment) = GetDirectiveParts<T>(text);
		return string.IsNullOrWhiteSpace(name)
			? (default, null, null)
			: Enum.TryParse<T>(name, true, out var result)
				? (result, parameter, comment)
				: (default, null, null);
	}

	#endregion

	#region Label Handling

	/// <summary>
	/// Determines whether the source supplied in <paramref name="text"/> contains a label
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>True if a label pattern is detected</returns>
	/// <remarks>Labels are defined as being:
	/// <ul>
	/// <li>may have optional whitespace preceding the colon</li>
	/// <li>prefixed with a colon</li>
	/// <li>starts with an alpha-character or underscore</li>
	/// <li>followed by up to 31 additional alphanumeric characters</li>
	/// <li>may have optional whitespace after the last character</li>
	/// <li>may also have an optional comment</li>
	/// </ul>
	/// </remarks>
	public static bool HasLabel(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       TokenRegex.HasLabel().Match(text).Success;
	}

	/// <summary>
	/// Extracts a label from the supplied source in <paramref name="text"/>, if present
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>The name of the label, if present, or <c>string.Empty</c></returns>
	public static (string? label, string? comment) GetLabelDetail(this string text)
	{
		var m = TokenRegex.HasLabel().Match(text);

		var label = m.Success
			? m.Groups[1].Value
			: null;
		var comment = m.Success && m.Groups[3].Success
			? m.Groups[3].Value
			: null;
		return (label, comment);
	}

	#endregion

	#region Generic Handling

	/// <summary>
	/// Determines whether the supplied <paramref name="text"/> contains a match for a "generic" word 
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>True if <paramref name="text"/> contains a "generic" word match, otherwise false</returns>
	public static bool HasGeneric(this string text)
	{
		return !string.IsNullOrWhiteSpace(text) &&
		       TokenRegex.GenericWord().Match(text).Success;
	}

	/// <summary>
	/// Determines whether the supplied <paramref name="text"/> contains a match for a "generic" word with optional parameter and comment collection 
	/// </summary>
	/// <param name="text">The text to be checked</param>
	/// <returns>A <see cref="Tuple{T1,T2,T3}"/> containing the generic word / command, any optional parameters associated with it and any optional comment</returns>
	public static (string word, string? parameters, string? comment) GetGenericDetail(this string text)
	{
		//	Check if there is a match found
		var m = TokenRegex.GenericWord().Match(text);

		var word = m.Success
			? m.Groups[1].Value
			: null!;
		var parameters = m.Success && m.Groups[2].Success && !string.IsNullOrWhiteSpace(m.Groups[2].Value)
			? m.Groups[2].Value.Trim()
			: null;
		var comment = m.Success && m.Groups[4].Success && !string.IsNullOrWhiteSpace(m.Groups[4].Value)
			? m.Groups[4].Value.Trim()
			: null;
		return (word, parameters, comment);
	}

	#endregion
}