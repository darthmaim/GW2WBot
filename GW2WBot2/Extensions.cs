using System;
using System.Collections.Generic;
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

        private static Regex _trailingPunctuation = new Regex(@"[\W]*$");
        public static string RemoveTrailingPunctuation(this string original)
        {
            return _trailingPunctuation.Replace(original, "");
        }
    }
}