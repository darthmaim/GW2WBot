using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GW2WBot2
{
    public static class Extensions
    {
         public static IEnumerable<string> GetLines(this string s)
         {
             var sb = new StringBuilder();
             char lastChar = '\0';
             foreach (char c in s)
             {
                 if (c == '\r' || c == '\n' && lastChar != '\r')
                 {
                     yield return sb.ToString();
                     sb.Clear();
                 }
                 else if (c != '\n')
                 {
                     sb.Append(c);
                 }
                 lastChar = c;
             }
             yield return sb.ToString();
         }

         /// <summary>
         /// Finds the part in the original string, which is removed in the modified string
         /// </summary>
         public static string FindRemovedPart(this string original, string modified)
         {
             if (original.Length < modified.Length) throw new ArgumentException();
             if (original.Length == modified.Length) return "";
             if (modified.Length == 0) return original;

             //find start of removed part

             var start = -1;

             for (int i = 0; i < original.Length && i <= modified.Length; i++)
             {
                 if (modified.Length == i || original[i] != modified[i])
                 {
                     start = i;
                     break;
                 }
             }

             if (start == -1) return original;

             var length = original.Length - start;

             for (int i = 1; i <= original.Length - start && i <= modified.Length; i++, length--)
             {
                 if (original[original.Length - i] != modified[modified.Length - i])
                 {
                     break;
                 }
             }

             return original.Substring(start, length);
         }

        private static readonly Regex TrailingPunctuation = new Regex(@"[\W]*$");
        public static string RemoveTrailingPunctuation(this string original)
        {
            return TrailingPunctuation.Replace(original, "");
        }

        /// <returns>Returns true if the key exists and the value is the same, returns false if the key doesnt exists or the value is different</returns>
        public static bool HasValueIgnoreCase(this IDictionary<string, string> dictionary, string key, string value)
        {
            return HasValue(dictionary, key, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <returns>Returns true if the key exists and the value is the same, returns false if the key doesnt exists or the value is different</returns>
        public static bool HasValue(this IDictionary<string, string> dictionary, string key, string value,
                                    StringComparison comparisonType = StringComparison.Ordinal)
        {
            return dictionary.ContainsKey(key) && dictionary[key].Equals(value, comparisonType);
        }

        public static bool HasValueIgnoreCase(this IDictionary<string, string> dictionary, string key, params string[] value)
        {
            return HasValue(dictionary, StringComparison.OrdinalIgnoreCase, key, value);
        }

        public static bool HasValue(this IDictionary<string, string> dictionary, string key, params string[] value)
        {
            return HasValue(dictionary, StringComparison.Ordinal, key, value);
        }

        public static bool HasValue(this IDictionary<string, string> dictionary, StringComparison comparisonType, string key, params string[] value)
        {
            return dictionary.ContainsKey(key) && value.Any(s => s.Equals(dictionary[key], comparisonType));
        }

        /// <returns>Returns true if the key exists and the value is different, returns false if the key doesnt exists or the value is the same</returns>
        public static bool HasNotValueIgnoreCase(this IDictionary<string, string> dictionary, string key, string value)
        {
            return HasNotValue(dictionary, key, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <returns>Returns true if the key exists and the value is different, returns false if the key doesnt exists or the value is the same</returns>
        public static bool HasNotValue(this IDictionary<string, string> dictionary, string key, string value,
                                    StringComparison comparisonType = StringComparison.Ordinal)
        {
            return dictionary.ContainsKey(key) && !dictionary[key].Equals(value, comparisonType);
        }
    }
}