using System.Collections.Generic;
using System.Text;

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
    }
}