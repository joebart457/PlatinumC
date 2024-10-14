using ParserLite;
using ParserLite.Exceptions;
using PlatinumC.Parser.Constants;
using PlatinumC.Shared;
using System.Runtime.InteropServices;
using TokenizerCore.Interfaces;
using TokenizerCore.Models.Constants;
using static PlatinumC.Shared.FunctionDeclaration;

namespace PlatinumC.Parser
{
    public class ProgramParser: TokenParser
    {
        public ParsingResult ParseFile(string path, out List<ParsingException> errors)
        {
            return ParseText(File.ReadAllText(path), out errors);
        }

        public ParsingResult ParseText(string text, out List<ParsingException> errors)
        {
            errors = new List<ParsingException>();
            var declarations = new List<Declaration>();
            var tokens = Tokenizers.Default.Tokenize(text);
            Initialize(tokens.Where(t => t.Type != BuiltinTokenTypes.EndOfFile).ToList());
            while (!AtEnd())
            {
                try
                {
                    declarations.Add(ParseDeclaration());
                }
                catch (ParsingException pe)
                {
                    errors.Add(pe);
                    SeekToNextParsableUnit();
                }
            }
            return new ParsingResult(declarations);
        }

        private void SeekToNextParsableUnit()
        {
            while (!AtEnd())
            {
                Advance();
                if (AdvanceIfMatch(TokenTypes.SemiColon)) break;
            }
        }
        public Declaration ParseDeclaration()
        {
            if (AdvanceIfMatch(TokenTypes.Library)) return ParseImportLibraryDeclaration();
            if (AdvanceIfMatch(TokenTypes.Import)) return ParseImportedFunctionDeclaration();
            return ParseFunctionDeclaration();
        }

        public ImportLibraryDeclaration ParseImportLibraryDeclaration()
        {
            // library kernel32 'kernel32.dll'

            var token = Previous();

            var libraryAlias = Consume(BuiltinTokenTypes.Word, "expect import library alias");
            var libraryPath = Consume(BuiltinTokenTypes.String, "expect import library path");

            return new ImportLibraryDeclaration(token, libraryAlias, libraryPath);
        }

        public ImportedFunctionDeclaration ParseImportedFunctionDeclaration()
        {
            // import kernel32 stdcall WriteConsoleA(string msg, int length, int bytesWritten, int reserved) from _WriteConsoleA@16;
            var token = Previous();
            var libraryAlias = Consume(BuiltinTokenTypes.Word, "expect import library alias");
            var callingConvention = CallingConvention.StdCall;
            if (Match(TokenTypes.CallingConvention)) callingConvention = ParseCallingConvention();

            var returnType = ParseTypeSymbol();
            var functionIdentifier = Consume(BuiltinTokenTypes.Word, "expect function identifier");
            Consume(TokenTypes.LParen, "expect parameter list");
            var parameterList = new List<ParameterDeclaration>();
            if (!AdvanceIfMatch(TokenTypes.RParen))
            {
                do
                {
                    var parameter = ParseParameterDeclaration();
                    parameterList.Add(parameter);
                } while (AdvanceIfMatch(TokenTypes.Comma));
                Consume(TokenTypes.RParen, "expect enclosing ) in parameter list");
            }
            var functionSymbol = functionIdentifier;
            if (AdvanceIfMatch(TokenTypes.From))
                functionSymbol = Consume(BuiltinTokenTypes.String, "expect import symbol");
            Consume(TokenTypes.SemiColon, "expect ; after imported function declaration");
            return new ImportedFunctionDeclaration(token, returnType, functionIdentifier, parameterList, callingConvention, libraryAlias, functionSymbol);
        }

        public FunctionDeclaration ParseFunctionDeclaration()
        {
            // export '_WriteConsoleA@16'
            // stdcall void WriteConsoleA(string msg, int length, int bytesWritten, int reserved)
            // {
            //
            //  ...
            // }

            bool isExport = false;
            IToken? exportAlias = null;
            if (AdvanceIfMatch(TokenTypes.Export))
            {
                isExport = true;
                exportAlias = Consume(BuiltinTokenTypes.String, "expect export alias");
            }
            
            var callingConvention = CallingConvention.StdCall;
            if (Match(TokenTypes.CallingConvention)) callingConvention = ParseCallingConvention();

            var returnType = ParseTypeSymbol();
            var token = Previous();
            var functionIdentifier = Consume(BuiltinTokenTypes.Word, "expect function identifier");
            Consume(TokenTypes.LParen, "expect parameter list");
            var parameterList = new List<ParameterDeclaration>();
            if (!AdvanceIfMatch(TokenTypes.RParen))
            {
                do
                {
                    var parameter = ParseParameterDeclaration();
                    parameterList.Add(parameter);
                } while (AdvanceIfMatch(TokenTypes.Comma));
                Consume(TokenTypes.RParen, "expect enclosing ) in parameter list");
            }
            Consume(TokenTypes.LCurly, "expect function body");
            var body = ParseBlock();

            return new FunctionDeclaration(token, returnType, functionIdentifier, parameterList, body.Statements, isExport, exportAlias ?? functionIdentifier, callingConvention);
        }


        public Block ParseBlock()
        {         
            var token = Previous();
            var statements = new List<Statement>();
            if (!AdvanceIfMatch(TokenTypes.RCurly))
            {
                do
                {
                    var statement = ParseStatement();
                    statements.Add(statement);
                } while (!AtEnd() && !Match(TokenTypes.RCurly));
                Consume(TokenTypes.RCurly, "expect enclosing } in block");
            }
            return new Block(token, statements);
        }

        public Statement ParseStatement()
        {
            if (AdvanceIfMatch(TokenTypes.If)) return ParseIfStatement();
            if (AdvanceIfMatch(TokenTypes.While)) return ParseWhileStatement();
            if (AdvanceIfMatch(TokenTypes.Break)) return new Break(Previous());
            if (AdvanceIfMatch(TokenTypes.Continue)) return new Continue(Previous());
            if (AdvanceIfMatch(TokenTypes.Return)) return ParseReturnStatement();
            if (AdvanceIfMatch(TokenTypes.LCurly)) return ParseBlock();
            if (Match(TokenTypes.SupportedType)) return ParseVariableDeclaration();
            return ParseExpressionStatement();
        }

        public IfStatement ParseIfStatement()
        {
            var token = Previous();
            Consume(TokenTypes.LParen, "expect condition");
            var conditional = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in if statement");
            var thenDo = ParseStatement();
            Statement? elseDo = null;
            if (AdvanceIfMatch(TokenTypes.Else)) 
                elseDo = ParseStatement();
            return new IfStatement(token, conditional, thenDo, elseDo);
        }

        public WhileStatement ParseWhileStatement()
        {
            var token = Previous();
            Consume(TokenTypes.LParen, "expect condition");
            var conditional = ParseExpression();
            Consume(TokenTypes.RParen, "expect enclosing ) in if statement");
            var thenDo = ParseStatement();
            
            return new WhileStatement(token, conditional, thenDo);
        }

        public ReturnStatement ParseReturnStatement()
        {
            var token = Previous();         
            var valueToReturn = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new ReturnStatement(token, valueToReturn);
        }

        public VariableDeclaration ParseVariableDeclaration()
        {
            var typeSymbol = ParseTypeSymbol();
            var token = Previous();
            var identifier = Consume(BuiltinTokenTypes.Word, "expect identifier symbol");
            Consume(TokenTypes.Equal, "expect initializer value in variable declaration");
            var initializerValue = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new VariableDeclaration(token, typeSymbol, identifier, initializerValue);
        }

        public ExpressionStatement ParseExpressionStatement()
        {
            var token = Previous();
            var expression = ParseExpression();
            Consume(TokenTypes.SemiColon, "expect ; after statement");
            return new ExpressionStatement(token, expression);  
        }

        public Expression ParseExpression()
        {
            return ParseAssignment();
        }

        public Expression ParseAssignment()
        {
            var expression = ParseBinaryLogicalAnd();
            if (AdvanceIfMatch(TokenTypes.Equal))
            {
                if (expression is Identifier identifier)
                {
                    var valueToAssign = ParseExpression();
                    return new Assignment(identifier.Token, null, identifier.Token, valueToAssign);
                }else if (expression is Dereference dereference)
                {
                    var token = Previous();
                    var valueToAssign = ParseExpression();
                    return new DereferenceAssignment(token, expression, valueToAssign);
                }
                else throw new ParsingException(Previous(), "unexpected assignment target on left hand side of =");
            }
            return expression;

        }

        public Expression ParseBinaryLogicalAnd()
        {
            var token = Previous();
            var expression = ParseBinaryLogicalOr();
            while (Match(TokenTypes.And))
            {
                var rhs = ParseExpression();

                expression = new BinaryLogicalAnd(token, expression, rhs);
            }
            return expression;
        }

        public Expression ParseBinaryLogicalOr()
        {
            var token = Previous();
            var expression = ParseBinaryComparison();
            while (Match(TokenTypes.Or))
            {
                var rhs = ParseExpression();

                expression = new BinaryLogicalOr(token, expression, rhs);
            }
            return expression;
        }


        public Expression ParseBinaryComparison()
        {
            var token = Previous();
            var expression = ParseBinarySubtraction();
            while (Match(TokenTypes.Comparison))
            {
                var comparisonType = ParseComparisonType();
                var rhs = ParseExpression();

                expression = new BinaryComparison(token, expression, rhs, comparisonType);
            }
            return expression;
        }


        public Expression ParseBinaryMultiplication()
        {
            var token = Previous();
            var expression = ParseCall();
            while (AdvanceIfMatch(TokenTypes.Asterisk))
            {
                var rhs = ParseCall();
                expression = new BinaryMultiplication(token, expression, rhs);
            }
            return expression;
        }

        public Expression ParseBinaryDivision()
        {
            var token = Previous();
            var expression = ParseBinaryMultiplication();
            while (AdvanceIfMatch(TokenTypes.ForwardSlash))
            {
                var rhs = ParseBinaryMultiplication();
                expression = new BinaryDivision(token, expression, rhs);
            }
            return expression;
        }

        public Expression ParseBinaryAddition()
        {
            var token = Previous();
            var expression = ParseBinaryDivision();
            while (AdvanceIfMatch(TokenTypes.Plus))
            {
                var rhs = ParseBinaryAddition();
                expression = new BinaryAddition(token, expression, rhs);
            }
            return expression;
        }

        public Expression ParseBinarySubtraction()
        {
            var token = Previous();
            var expression = ParseBinaryAddition();
            while (AdvanceIfMatch(TokenTypes.Minus))
            {
                var rhs = ParseBinaryAddition();
                expression = new BinarySubtraction(token, expression, rhs);
            }
            return expression;
        }


        public Expression ParseCall()
        {
            var expression = ParseUnary();
            while (Match(TokenTypes.LParen) || Match(TokenTypes.Arrow))
            {
                if (AdvanceIfMatch(TokenTypes.LParen))
                {
                    if (expression is Identifier identifier)
                    {
                        var arguments = new List<Expression>();
                        if (!AdvanceIfMatch(TokenTypes.RParen))
                        {
                            do
                            {
                                arguments.Add(ParseExpression());
                            } while (AdvanceIfMatch(TokenTypes.Comma));
                            Consume(TokenTypes.RParen, "expect enclosing ) after argument list");
                        }
                        expression = new Call(identifier.Token, identifier.Token, arguments);
                    }
                    else throw new ParsingException(Previous(), "invalid left hand side of call");
                }else
                {
                    Advance();
                    // it is a get expression
                    // IE struct->member
                    var memberName = Consume(BuiltinTokenTypes.Word, "expect member name");
                    expression = new GetFromReference(memberName, expression, memberName);
                }
                
            }
            return expression;
        }

        public Expression ParseUnary()
        {
            if (AdvanceIfMatch(TokenTypes.Asterisk))
            {
                var token = Previous();
                var expresion = ParsePrimary();
                return new Dereference(token, expresion);
            }
            if (AdvanceIfMatch(TokenTypes.Ampersand))
            {
                var identifier = Consume(BuiltinTokenTypes.Word, "expect symbol to reference");
                return new Reference(identifier);
            }
            return ParsePrimary();
        }

        public Expression ParsePrimary()
        {
            if (AdvanceIfMatch(BuiltinTokenTypes.Word)) return new Identifier(Previous());
            if (AdvanceIfMatch(BuiltinTokenTypes.String)) return new LiteralString(Previous(), Previous().Lexeme);
            if (AdvanceIfMatch(BuiltinTokenTypes.Integer)) return new LiteralInteger(Previous(), int.Parse(Previous().Lexeme));
            if(AdvanceIfMatch(BuiltinTokenTypes.Float)) return new LiteralFloatingPoint(Previous(), float.Parse(Previous().Lexeme));
            if (AdvanceIfMatch(TokenTypes.Minus))
            {
                if (AdvanceIfMatch(BuiltinTokenTypes.Integer)) return new LiteralInteger(Previous(), -int.Parse(Previous().Lexeme));
                if (AdvanceIfMatch(BuiltinTokenTypes.Float)) return new LiteralFloatingPoint(Previous(), -float.Parse(Previous().Lexeme));
                throw new ParsingException(Previous(), "unsupported right hand side of unary -");
            }
            if (AdvanceIfMatch(TokenTypes.LParen))
            {
                var token = Previous();
                var expression = ParseExpression();
                Consume(TokenTypes.RParen, "expect enclosing )");
                return new Group(token, expression);
            }
            throw new ParsingException(Current(), "encountered unexpected token");
        }

        private ParameterDeclaration ParseParameterDeclaration()
        {
            var typeSymbol = ParseTypeSymbol();
            var parameterName = Consume(BuiltinTokenTypes.Word, "expect parameter name");
            return new ParameterDeclaration(parameterName, typeSymbol);
        }

        private CallingConvention ParseCallingConvention()
        {
            var token = Consume(TokenTypes.CallingConvention, "expect calling convention");
            if (Enum.TryParse<CallingConvention>(token.Lexeme, out var callingConvention)) return callingConvention;
            throw new ParsingException(token, $"expected calling convention but got {token.Lexeme}");
        }

        private ComparisonType ParseComparisonType()
        {
            var token = Consume(TokenTypes.Comparison, "expect comparison");
            if (token.Lexeme == "==") return ComparisonType.Equal;
            if (token.Lexeme == "!=") return ComparisonType.NotEqual;
            if (token.Lexeme == ">") return ComparisonType.GreaterThan;
            if (token.Lexeme == ">=") return ComparisonType.GreaterThanEqual;
            if (token.Lexeme == "<") return ComparisonType.LessThan;
            if (token.Lexeme == "<=") return ComparisonType.LessThanEqual;
            throw new ParsingException(token, $"expected comparison but got {token.Lexeme}");
        }
        private TypeSymbol ParseTypeSymbol(TypeSymbol? typeSymbol = null)
        {
            if (typeSymbol == null)
            {
                var supportedType = ParseSupportedType();
                // for ptr and string typedefs
                if (supportedType == SupportedType.Ptr) typeSymbol = new TypeSymbol(Previous(), SupportedType.Ptr, new TypeSymbol(Previous(), SupportedType.Void, null));
                else if (supportedType == SupportedType.String) typeSymbol = new TypeSymbol(Previous(), SupportedType.Ptr, new TypeSymbol(Previous(), SupportedType.Byte, null));
                else typeSymbol = new TypeSymbol(Previous(), supportedType, null);
            } 
            
            if (AdvanceIfMatch(TokenTypes.Asterisk))
            {
                typeSymbol = new TypeSymbol(Previous(), SupportedType.Ptr, ParseTypeSymbol(typeSymbol));
            }
            return typeSymbol;
        }

        private SupportedType ParseSupportedType()
        {
            var token = Consume(TokenTypes.SupportedType, "expect typename");
            if (Enum.TryParse<SupportedType>(token.Lexeme, true, out var supportedType)) return supportedType;
            throw new ParsingException(token, $"expected type but got {token.Lexeme}");
        }

    }
}
