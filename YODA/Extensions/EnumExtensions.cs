using System.ComponentModel;

namespace SimpleInstructionMachine.Extensions;

public static class EnumExtensions
{
	/// <summary>
	/// Extracts the <see cref="DescriptionAttribute"/> from the enum value, if present
	/// </summary>
	/// <param name="value">The enum value to extract</param>
	/// <returns>The value of the <see cref="DescriptionAttribute"/>, or the ToString equivalent</returns>
	public static string Description(Enum value)
	{
		//	Get the FieldInfo details for the enum value
		var fi = value.GetType().GetField($"{value}");

		//	Extract [Description("")] values. If found, return first description, or ToString of value
		return (fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) is not DescriptionAttribute[] descriptions || descriptions.Length == 0)
			? $"{value}"
			: descriptions[0].Description;
	}
}