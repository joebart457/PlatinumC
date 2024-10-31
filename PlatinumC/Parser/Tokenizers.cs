using PlatinumC.Parser.Constants;
using System.Runtime.InteropServices;
using TokenizerCore.Model;
using TokenizerCore.Models.Constants;
using TokenizerCore;
using PlatinumC.Shared;

namespace PlatinumC.Parser
{
    public static class Tokenizers
    {
        private static List<TokenizerRule> _defaultRules => new List<TokenizerRule>()
        {
                    new TokenizerRule(TokenTypes.Export, "export"),
                    new TokenizerRule(TokenTypes.Library, "library"),
                    new TokenizerRule(TokenTypes.LParen, "("),
                    new TokenizerRule(TokenTypes.RParen, ")"),
                    new TokenizerRule(TokenTypes.Colon, ":"),
                    new TokenizerRule(TokenTypes.Comma, ","),
                    new TokenizerRule(TokenTypes.Dot, "."),
                    new TokenizerRule(TokenTypes.LCurly, "{"),
                    new TokenizerRule(TokenTypes.RCurly, "}"),
                    new TokenizerRule(TokenTypes.SemiColon, ";"),
                    new TokenizerRule(TokenTypes.Import, "import"),
                    new TokenizerRule(TokenTypes.From, "from"),
                    new TokenizerRule(TokenTypes.If, "if"),
                    new TokenizerRule(TokenTypes.While, "while"),
                    new TokenizerRule(TokenTypes.Break, "break"),
                    new TokenizerRule(TokenTypes.Continue, "continue"),
                    new TokenizerRule(TokenTypes.Return, "return"),
                    new TokenizerRule(TokenTypes.Else, "else"),
                    new TokenizerRule(TokenTypes.And, "&&"),
                    new TokenizerRule(TokenTypes.Or, "||"),
                    new TokenizerRule(TokenTypes.Asterisk, "*"),
                    new TokenizerRule(TokenTypes.ForwardSlash, "/"),
                    new TokenizerRule(TokenTypes.Plus, "+"),
                    new TokenizerRule(TokenTypes.Minus, "-"),
                    new TokenizerRule(TokenTypes.Equal, "="),
                    new TokenizerRule(TokenTypes.Arrow, "->"),
                    new TokenizerRule(TokenTypes.Ampersand, "&"),
                    new TokenizerRule(TokenTypes.LessThan, "<"),
                    new TokenizerRule(TokenTypes.LessThanEqual, "<="),
                    new TokenizerRule(TokenTypes.GreaterThan, ">"),
                    new TokenizerRule(TokenTypes.GreaterThanEqual, ">="),
                    new TokenizerRule(TokenTypes.EqualEqual, "=="),
                    new TokenizerRule(TokenTypes.NotEqual, "!="),

                    new TokenizerRule(TokenTypes.Pipe, "|"),
                    new TokenizerRule(TokenTypes.UpCarat, "^"),
                    new TokenizerRule(TokenTypes.Not, "!"),
                    new TokenizerRule(TokenTypes.BitwiseNot, "~"),
                    new TokenizerRule(TokenTypes.Nullptr, "nullptr"),
                    new TokenizerRule(TokenTypes.Global, "global"),
                    new TokenizerRule(TokenTypes.As, "as"),
                    new TokenizerRule(TokenTypes.Icon, "icon"),

                    new TokenizerRule(TokenTypes.CallingConvention, CallingConvention.Cdecl.ToString(), ignoreCase: true),
                    new TokenizerRule(TokenTypes.CallingConvention, CallingConvention.StdCall.ToString(), ignoreCase: true),

                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.Int.ToString().ToLower()),
                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.Float.ToString().ToLower()),
                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.Byte.ToString().ToLower()),
                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.Void.ToString().ToLower()),
                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.String.ToString().ToLower()),
                    new TokenizerRule(TokenTypes.SupportedType, SupportedType.Ptr.ToString().ToLower()),

                    new TokenizerRule(BuiltinTokenTypes.EndOfLineComment, "//"),
                    new TokenizerRule(BuiltinTokenTypes.String, "\"", enclosingLeft: "\"", enclosingRight: "\""),
                    new TokenizerRule(BuiltinTokenTypes.String, "'", enclosingLeft: "'", enclosingRight: "'"),
                    new TokenizerRule(BuiltinTokenTypes.Word, "`", enclosingLeft: "`", enclosingRight: "`"),
        };
        public static TokenizerSettings DefaultSettings => new TokenizerSettings
        {
            AllowNegatives = true,
            NegativeChar = '-',
            NewlinesAsTokens = false,
        };
        public static Tokenizer Default => new Tokenizer(_defaultRules, DefaultSettings);
    }
}
