using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Utilities
{
    public static class TextBoxExtensions
	{
		private const uint ECM_FIRST = 0x1500;
		private const uint EM_SETCUEBANNER = ECM_FIRST + 1;

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

		public static void SetWatermark(this TextBox textBox, string watermarkText)
		{
			SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, watermarkText);
		}
	}

	public static class TypeExtensions
	{
		public static bool IsAtomic(this Type type)
		{
			return type.IsValueType || type == typeof(string);
		}
	}

	public static class ColorExtensions
	{
		public static Color Blend(this Color color1, Color color2, double percent)
		{
			if (percent <= 0)
				return color1;
			else if (percent >= 1)
				return color2;

			double p1 = 1 - percent;

			int r1 = (int)(color1.R * p1);
			int g1 = (int)(color1.G * p1);
			int b1 = (int)(color1.B * p1);
			int r2 = (int)(color2.R * percent);
			int g2 = (int)(color2.G * percent);
			int b2 = (int)(color2.B * percent);

			return Color.FromArgb(r1 + r2, g1 + g2, b1 + b2);
		}
	}
}

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
	{
		/// <summary>
		/// Remove the first entry that contains the given value from the Dictionary
		/// </summary>
		public static bool RemoveValue<TKey, TValue>(this Dictionary<TKey, TValue> source, TValue value)
		{
			if (source.FirstOrDefault(x => x.Value.Equals(value)) is KeyValuePair<TKey, TValue> deleteMe)
				return source.Remove(deleteMe.Key);
			return false;
		}
	}
}

namespace System
{
    public static class CharExtensions
	{
		public static bool IsVowel(this char c) => "aeiou".Contains(c);
	}

	public static class StringExtensions
	{
		/// <summary>
		/// Produces a printable sequence of byte codes from
		/// a potentialy unprintable sequence of bytes. The result
		/// looks something like this: &quot;C3 02 54 4E&quot;...
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToByteString(this string s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			StringBuilder sb = new StringBuilder();
			foreach (byte b in s)
				sb.Append($"{b:X2} ");
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}

		public static string Plurality(this string singular, double n) =>
			n == 1 ? singular : singular.Plural();

		public static string Plural(this string singular)
		{
			if (string.IsNullOrEmpty(singular)) return string.Empty;
			singular.TrimEnd();
			if (string.IsNullOrEmpty(singular)) return string.Empty;

			int slen = singular.Length;
			char ultimate = singular[slen - 1];
			if (slen == 1)
			{
				if (char.IsUpper(ultimate)) return singular + "s";
				return singular + "'s";
			}
			ultimate = char.ToLower(ultimate);
			char penultimate = char.ToLower(singular[slen - 2]);

			if (ultimate == 'y')
			{
				if (penultimate.IsVowel()) return singular + "s";
				return singular.Substring(0, slen - 1) + "ies";
			}
			if (ultimate == 'f')
				return singular.Substring(0, slen - 1) + "ves";
			if (penultimate == 'f' && ultimate == 'e')
				return singular.Substring(0, slen - 2) + "ves";
			if ((penultimate == 'c' && ultimate == 'h') ||
				(penultimate == 's' && ultimate == 'h') ||
				(penultimate == 's' && ultimate == 's') ||
				(ultimate == 'x') ||
				(ultimate == 'o' && !penultimate.IsVowel()))
				return singular + "es";
			return singular + "s";
		}

	}

	public static class ArrayExtensions
	{
		public static Array RemoveAt(this Array source, int index)
		{
			Array dest = Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);

			if (index > 0)
				Array.Copy(source, 0, dest, 0, index);

			if (index < source.Length - 1)
				Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

			return dest;
		}

		/// <summary>
		/// Convert the string into an ASCII8 byte array.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static byte[] ToASCII8ByteArray(this string s) =>
			EncodingType.ASCII8.GetBytes(s);

		/// <summary>
		/// Should be called ToString but extensions can't override base methods.
		/// </summary>
		/// <param name="byteArray"></param>
		/// <returns></returns>
		public static string ToStringToo(this byte[] byteArray) =>
			ToString(byteArray, 0, byteArray?.Length ?? 0);

		/// <summary>
		/// Converts a range of a byte array into a string.
		/// </summary>
		/// <param name="byteArray"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string ToString(this byte[] byteArray, int startIndex, int length)
		{
			if (byteArray == null || startIndex < 0 || length < 1 || startIndex >= byteArray.Length)
				return "";

			return EncodingType.ASCII8.GetString(byteArray, startIndex, length);
		}
	}
}

namespace System.Text
{
    public class EncodingType
	{
		/// <summary>
		/// ASCII uses only 7 bits; use this 8-bit "extended ASCII" encoding
		/// </summary>
		public static Encoding ASCII8 = Encoding.GetEncoding("iso-8859-1");
	}
}