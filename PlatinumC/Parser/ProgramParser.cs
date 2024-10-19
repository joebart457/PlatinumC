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
        private readonly List<(int precedence, List<BinaryOperator> operators)> BinaryOperators = new()
        {
            
            (
                3,
                new()
                {
                    new(3, TokenTypes.Asterisk, (token, lhs, rhs) => new BinaryMultiplication(token, lhs, rhs)),
                    new(3, TokenTypes.ForwardSlash, (token, lhs, rhs) => new BinaryDivision(token, lhs, rhs)),
                }
            ),
            (
                4,
                new()
                {
                    new(4, TokenTypes.Plus, (token, lhs, rhs) => new BinaryAddition(token, lhs, rhs)),
                    new(4, TokenTypes.Minus, (token, lhs, rhs) => new BinarySubtraction(token, lhs, rhs)),

                }
            ),
            (
                6,
                new()
                {
                    new(6, TokenTypes.LessThan, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.LessThan)),
                    new(6, TokenTypes.LessThanEqual, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.LessThanEqual)),
                    new(6, TokenTypes.GreaterThan, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.GreaterThan)),
                    new(6, TokenTypes.GreaterThanEqual, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.GreaterThanEqual)),
                }
            ),
            (
                7,
                new()
                {
                    new(7, TokenTypes.EqualEqual, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.Equal)),
                    new(7, TokenTypes.NotEqual, (token, lhs, rhs) => new BinaryComparison(token, lhs, rhs, ComparisonType.NotEqual)),
                }
            ),
            (
                8,
                new()
                {
                    new(8, TokenTypes.Ampersand, (token, lhs, rhs) => new BinaryBitwiseAnd(token, lhs, rhs)),
                }
            ),
            (
                9,
                new()
                {
                    new(9, TokenTypes.Pipe, (token, lhs, rhs) => new BinaryBitwiseOr(token, lhs, rhs)),
                }
            ),
            (
                10,
                new()
                {
                    new(10, TokenTypes.UpCarat, (token, lhs, rhs) => new BinaryBitwiseXor(token, lhs, rhs)),
                }
            ),
            (
                11,
                new()
                {
                    new(11, TokenTypes.And, (token, lhs, rhs) => new BinaryLogicalAnd(token, lhs, rhs)),
                }
            ),
            (
                12,
                new()
                {
                    new(12, TokenTypes.Or, (token, lhs, rhs) => new BinaryLogicalOr(token, lhs, rhs)),
                }
            ),

        };
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
            if (AdvanceIfMatch(TokenTypes.SemiColon)) return new ReturnStatement(token, null);
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

        public Expression ParseBinary(int? index = null)
        {
            if (index == null) index = BinaryOperators.Count - 1;
            if (index < 0)
                return ParseCall();
            var expression = ParseBinary(index - 1);
            var operators = BinaryOperators[index.Value].operators;
            var matchedOperator = operators.FirstOrDefault(x => Match(x.OperatorTokenType));
            if (matchedOperator != null)
            {
                var operatorToken = Consume(matchedOperator.OperatorTokenType, "expect operator");
                expression = matchedOperator.Yield(operatorToken, expression, ParseBinary(index));
            }
            return expression;
        }

        public Expression ParseAssignment()
        {
            var expression = ParseBinary();
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
            Expression? expression = null;
            while(Match(TokenTypes.Asterisk) || Match(TokenTypes.Ampersand) || Match(TokenTypes.Not) || Match(TokenTypes.BitwiseNot))
            {
                if (AdvanceIfMatch(TokenTypes.Asterisk))
                {
                    var token = Previous();
                    var expresion = ParseCall();
                    expression = new Dereference(token, expresion);
                }
                else if (AdvanceIfMatch(TokenTypes.Ampersand))
                {
                    var identifier = Consume(BuiltinTokenTypes.Word, "expect symbol to reference");
                    expression = new Reference(identifier);
                }
                else if (AdvanceIfMatch(TokenTypes.Not))
                {
                    var token = Previous();
                    var expresion = ParseCall();
                    expression = new UnaryNot(token, expresion);
                }
                else if (AdvanceIfMatch(TokenTypes.BitwiseNot))
                {
                    var token = Previous();
                    var expresion = ParseCall();
                    expression = new UnaryNegation(token, expresion);
                }
                else throw new ParsingException(Current(), $"unable to determine unary operation");
            }
            if (expression != null) return expression;
            return ParsePrimary();
        }

        public Expression ParsePrimary()
        {
            if (AdvanceIfMatch(BuiltinTokenTypes.Word)) return new Identifier(Previous());
            if (AdvanceIfMatch(BuiltinTokenTypes.String)) return new LiteralString(Previous(), Previous().Lexeme);
            if (AdvanceIfMatch(BuiltinTokenTypes.Integer)) return new LiteralInteger(Previous(), int.Parse(Previous().Lexeme));
            if (AdvanceIfMatch(BuiltinTokenTypes.Byte)) return new LiteralByte(Previous(), byte.Parse(Previous().Lexeme));
            if (AdvanceIfMatch(BuiltinTokenTypes.Float)) return new LiteralFloatingPoint(Previous(), float.Parse(Previous().Lexeme));
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

    public class BinaryOperator
    {
        public int Precedence { get; set; }
        public string OperatorTokenType { get; set; }
        public Func<IToken, Expression, Expression, Expression> ExpressionGenerator { get; set; }
        public BinaryOperator(int precedence, string operatorTokenType, Func<IToken, Expression, Expression, Expression> expressionGenerator)
        {
            Precedence = precedence;
            OperatorTokenType = operatorTokenType;
            ExpressionGenerator = expressionGenerator;
        }

        public Expression Yield(IToken token, Expression lhs, Expression rhs) => ExpressionGenerator(token, lhs, rhs);
    }
}
