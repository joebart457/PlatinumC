using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Extensions
{
    internal static class StringExtensions
    {
        public static string Indent(this string str, int indentLevel = 1)
        {
            return $"{new string('\t', indentLevel)}{str}";
        }
    }
}
