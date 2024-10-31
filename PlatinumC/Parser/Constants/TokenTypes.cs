using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Parser.Constants
{
    public static class TokenTypes
    {
        public const string Export = "Export";
        public const string Library = "Library";

        public const string CallingConvention = "CallingConvention";
        public const string SupportedType = "SupportedType";

        public const string LParen = "LParen";
        public const string RParen = "RParen";
        public const string Colon = "Colon";
        public const string Comma = "Comma";
        public const string Dot = "Dot";
        public const string LCurly = "LCurly";
        public const string RCurly = "RCurly";

        public const string SemiColon = "SemiColon";
        public const string Import = "Import";
        public const string From = "From";
        public const string If = "If";
        public const string While = "While";
        public const string Break = "Break";
        public const string Continue = "Continue";
        public const string Return = "Return";
        public const string Else = "Else";
        public const string And = "And";
        public const string Or = "Or";
        public const string Asterisk = "Asterisk";
        public const string ForwardSlash = "ForwardSlash";
        public const string Plus = "Plus";
        public const string Minus = "Minus";
        public const string Equal = "Equal";

        public const string Arrow = "Arrow";
        public const string Ampersand = "Ampersand";

        public static string LessThan = "LessThan";
        public static string LessThanEqual = "LessThanEqual";
        public static string GreaterThan = "GreaterThan";
        public static string GreaterThanEqual = "GreaterThanEqual";
        public static string EqualEqual = "EqualEqual";
        public static string NotEqual = "NotEqual";

        public static string Pipe = "Pipe";
        public static string UpCarat = "UpCarat";
        public static string Not = "Not";
        public static string BitwiseNot = "BitwiseNot";
        public static string Nullptr = "Nullptr";
        public static string Global = "Global";
        public static string As = "As";

        public static string Icon = "Icon";
    }
}
