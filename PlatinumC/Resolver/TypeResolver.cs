using ParserLite.Exceptions;
using PlatinumC.Parser;
using PlatinumC.Shared;
using static PlatinumC.Shared.TypedFunctionDeclaration;

namespace PlatinumC.Resolver
{
    public class TypeResolver
    {
        private Dictionary<string, ResolvedType> _localVariables { get; set; } = new();
        private Dictionary<string, TypedFunctionDeclaration> _functions { get; set; } = new();
        private Dictionary<string, TypedImportedFunctionDeclaration> _importedFunctions { get; set; } = new();
        private TypedFunctionDeclaration? _currentFunction;
        private TypedFunctionDeclaration CurrentFunction => _currentFunction ?? throw new InvalidOperationException();
        private Dictionary<string, TypedImportLibraryDeclaration> _importedLibraries = new();
        private class LoopInfo {}
        private Stack<LoopInfo> _loops = new();

        public ResolverResult ResolveTypes(ParsingResult parsingResult)
        {
            _localVariables = new();
            _functions = new();
            _importedFunctions = new();
            _currentFunction = null;
            _importedLibraries = new();
            _loops = new();
            _currentFunction = null;
            var resolvedDeclarations = new List<TypedDeclaration>();
            foreach(var declaration in parsingResult.Declarations)
            {
                if (declaration is ImportLibraryDeclaration importLibraryDeclaration)
                    resolvedDeclarations.Add(Accept(importLibraryDeclaration));
            }

            foreach (var declaration in parsingResult.Declarations)
            {
                if (declaration is ImportedFunctionDeclaration importedFunctionDeclaration)
                    GatherDefintion(importedFunctionDeclaration);
                if (declaration is FunctionDeclaration functionDeclaration)
                    GatherDefintion(functionDeclaration);
            }


            foreach (var declaration in parsingResult.Declarations)
            {
                if (!(declaration is ImportLibraryDeclaration))
                    resolvedDeclarations.Add(declaration.Visit(this));
            }
            return new ResolverResult(resolvedDeclarations);
        }

        internal void GatherDefintion(FunctionDeclaration functionDeclaration)
        {
            var returnType = Resolve(functionDeclaration.ReturnType);
            var parameters = functionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(functionDeclaration.Token, $"parameter names must not repeat");

            var function = new TypedFunctionDeclaration(functionDeclaration, returnType, functionDeclaration.FunctionIdentifier, parameters, new(), false, functionDeclaration.CallingConvention, functionDeclaration.IsExport, functionDeclaration.ExportedAlias);
            if (_functions.ContainsKey(functionDeclaration.FunctionIdentifier.Lexeme) || _importedFunctions.ContainsKey(functionDeclaration.FunctionIdentifier.Lexeme))
                throw new ParsingException(functionDeclaration.FunctionIdentifier, $"function {functionDeclaration.FunctionIdentifier.Lexeme} is already defined");
            
            _functions[functionDeclaration.FunctionIdentifier.Lexeme] = function;
        }

        internal void GatherDefintion(ImportedFunctionDeclaration importedFunctionDeclaration)
        {
            var returnType = Resolve(importedFunctionDeclaration.ReturnType);
            var parameters = importedFunctionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(importedFunctionDeclaration.Token, $"parameter names must not repeat");

            if (!_importedLibraries.ContainsKey(importedFunctionDeclaration.LibraryAlias.Lexeme))
                throw new ParsingException(importedFunctionDeclaration.LibraryAlias, $"library with alias {importedFunctionDeclaration.LibraryAlias} is not defined");

            var importedFunction = new TypedImportedFunctionDeclaration(importedFunctionDeclaration, returnType, importedFunctionDeclaration.FunctionIdentifier, parameters, importedFunctionDeclaration.CallingConvention, importedFunctionDeclaration.LibraryAlias, importedFunctionDeclaration.FunctionSymbol);
            if (_importedFunctions.ContainsKey(importedFunction.FunctionIdentifier.Lexeme) || _functions.ContainsKey(importedFunction.FunctionIdentifier.Lexeme))
                throw new ParsingException(importedFunction.FunctionIdentifier, $"function {importedFunction.FunctionIdentifier} is already defined");
            _importedFunctions[importedFunction.FunctionIdentifier.Lexeme] = importedFunction;
        }


        private ResolvedType Resolve(TypeSymbol typeSymbol)
        {
            if (typeSymbol.SupportedType == SupportedType.Ptr)
            {
                if (typeSymbol.UnderlyingType == null) throw new InvalidOperationException();
                return new ResolvedType(SupportedType.Ptr, Resolve(typeSymbol.UnderlyingType));
            }
            return ResolvedType.Create(typeSymbol.SupportedType);
        }

        internal TypedExpression Accept(Identifier identifier)
        {
            if (_localVariables.TryGetValue(identifier.Token.Lexeme, out var resolvedType)) return new TypedIdentifier(identifier, resolvedType, identifier.Token);
            var foundVariable = CurrentFunction.Parameters.Find(x => x.ParameterName.Lexeme == identifier.Token.Lexeme);
            if (foundVariable == null) throw new ParsingException(identifier.Token, $"identifier {identifier.Token.Lexeme} is not defined");
            return new TypedIdentifier(identifier, foundVariable.ResolvedType, identifier.Token);
        }

        internal TypedExpression Accept(Call call)
        {
            _importedFunctions.TryGetValue(call.FunctionIdentifier.Lexeme, out var importedFunction);

            if (importedFunction == null)
            {
                if (!_functions.TryGetValue(call.FunctionIdentifier.Lexeme, out var function))
                    throw new ParsingException(call.FunctionIdentifier, $"function {call.FunctionIdentifier.Lexeme} is not defined");
                if (call.Arguments.Count != function.Parameters.Count)
                    throw new ParsingException(call.FunctionIdentifier, $"function {function.FunctionIdentifier.Lexeme} takes {function.Parameters.Count} arguments, {call.Arguments.Count} were provided");
                var resolvedArguments = new List<TypedExpression>();
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var resolvedArgument = call.Arguments[i].Visit(this);
                    if (!function.Parameters[i].ResolvedType.Is(resolvedArgument.ResolvedType))
                        throw new ParsingException(resolvedArgument.OriginalExpression.Token, $"call: {function.FunctionIdentifier.Lexeme} expected argument of type {function.Parameters[i].ResolvedType} but got {resolvedArgument.ResolvedType}");
                    resolvedArguments.Add(resolvedArgument);
                }
                return new TypedCall(call, function.ReturnType, function, resolvedArguments);
            }else
            {
                if (call.Arguments.Count != importedFunction.Parameters.Count)
                    throw new ParsingException(call.FunctionIdentifier, $"function {importedFunction.FunctionIdentifier.Lexeme} takes {importedFunction.Parameters.Count} arguments, {call.Arguments.Count} were provided");
                var resolvedArguments = new List<TypedExpression>();
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    var resolvedArgument = call.Arguments[i].Visit(this);
                    if (!importedFunction.Parameters[i].ResolvedType.Is(resolvedArgument.ResolvedType))
                        throw new ParsingException(resolvedArgument.OriginalExpression.Token, $"call: {importedFunction.FunctionIdentifier.Lexeme} expected argument of type {importedFunction.Parameters[i].ResolvedType} but got {resolvedArgument.ResolvedType}");
                    resolvedArguments.Add(resolvedArgument);
                }
                return new TypedCallImportedFunction(call, importedFunction.ReturnType, importedFunction, resolvedArguments);
            }

            
        }

        internal TypedExpression Accept(BinaryAddition binaryAddition)
        {
            var lhs = binaryAddition.Lhs.Visit(this);
            var rhs = binaryAddition.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binaryAddition.Token, $"right hand side of integer addition must also be an integer");
                return new TypedBinaryAddition_Integer_Integer(binaryAddition, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binaryAddition.Token, $"right hand side of floating point addition must also be floating point");
                return new TypedBinaryAddition_Float_Float(binaryAddition, ResolvedType.Create(SupportedType.Float), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Ptr))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binaryAddition.Token, $"right hand side of pointer addition must be an integer");
                return new TypedBinaryAddition_Pointer_Integer(binaryAddition, lhs.ResolvedType, lhs, rhs);
            }
            else throw new ParsingException(binaryAddition.Token, $"addition is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinarySubtraction binarySubtraction)
        {
            var lhs = binarySubtraction.Lhs.Visit(this);
            var rhs = binarySubtraction.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binarySubtraction.Token, $"right hand side of integer subtraction must also an be integer");
                return new TypedBinarySubtraction_Integer_Integer(binarySubtraction, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binarySubtraction.Token, $"right hand side of floating point subtraction must also be floating point");
                return new TypedBinarySubtraction_Float_Float(binarySubtraction, ResolvedType.Create(SupportedType.Float), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Ptr))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binarySubtraction.Token, $"right hand side of pointer subtraction must be an integer");
                return new TypedBinarySubtraction_Pointer_Integer(binarySubtraction, lhs.ResolvedType, lhs, rhs);
            }
            else throw new ParsingException(binarySubtraction.Token, $"subtraction is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryMultiplication binaryMultiplication)
        {
            var lhs = binaryMultiplication.Lhs.Visit(this);
            var rhs = binaryMultiplication.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binaryMultiplication.Token, $"right hand side of integer multiplication must also be an integer");
                return new TypedBinaryMultiplication_Integer_Integer(binaryMultiplication, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binaryMultiplication.Token, $"right hand side of floating point multiplication must also be floating point");
                return new TypedBinaryMultiplication_Float_Float(binaryMultiplication, ResolvedType.Create(SupportedType.Float), lhs, rhs);
            }
            else throw new ParsingException(binaryMultiplication.Token, $"multiplication is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryDivision binaryDivision)
        {
            var lhs = binaryDivision.Lhs.Visit(this);
            var rhs = binaryDivision.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Int)) throw new ParsingException(binaryDivision.Token, $"right hand side of integer division must also be an integer");
                return new TypedBinaryDivision_Integer_Integer(binaryDivision, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binaryDivision.Token, $"right hand side of floating point division must also be floating point");
                return new TypedBinaryDivision_Float_Float(binaryDivision, ResolvedType.Create(SupportedType.Float), lhs, rhs);
            }
            else throw new ParsingException(binaryDivision.Token, $"division is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryComparison binaryComparison)
        {
            var lhs = binaryComparison.Lhs.Visit(this);
            var rhs = binaryComparison.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int))
            {
                if (!(rhs.ResolvedType.Is(SupportedType.Int) || rhs.ResolvedType.Is(SupportedType.Ptr))) throw new ParsingException(binaryComparison.Token, $"right hand side of integer comparison must be another integer or a pointer");
                return new TypedBinaryComparison_Integer_Integer(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binaryComparison.Token, $"right hand side of floating point addition must also be floating point");
                return new TypedBinaryComparison_Float_Float(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Ptr))
            {
                if (!(rhs.ResolvedType.Is(SupportedType.Int) || rhs.ResolvedType.Is(lhs.ResolvedType))) throw new ParsingException(binaryComparison.Token, $"right hand side of pointer comparison must be an integer or another pointer");
                return new TypedBinaryComparison_Integer_Integer(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else throw new ParsingException(binaryComparison.Token, $"comparison is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryLogicalAnd binaryLogicalAnd)
        {
            var lhs = binaryLogicalAnd.Lhs.Visit(this);
            var rhs = binaryLogicalAnd.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Int))
            {
                return new TypedBinaryLogicalAnd_Integer_Integer(binaryLogicalAnd, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            throw new ParsingException(binaryLogicalAnd.Token, $"logical and operator is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryLogicalOr binaryLogicalOr)
        {
            var lhs = binaryLogicalOr.Lhs.Visit(this);
            var rhs = binaryLogicalOr.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Int))
            {
                return new TypedBinaryLogicalAnd_Integer_Integer(binaryLogicalOr, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            }
            throw new ParsingException(binaryLogicalOr.Token, $"logical or operator is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(LiteralString literalString)
        {
            return new TypedLiteralString(literalString, new ResolvedType(SupportedType.Ptr, ResolvedType.Create(SupportedType.Byte)), literalString.Value);
        }

        internal TypedExpression Accept(LiteralInteger literalInteger)
        {
            return new TypedLiteralInteger(literalInteger, ResolvedType.Create(SupportedType.Int), literalInteger.Value);
        }

        internal TypedExpression Accept(LiteralFloatingPoint literalFloatingPoint)
        {
            return new TypedLiteralFloatingPoint(literalFloatingPoint, ResolvedType.Create(SupportedType.Float), literalFloatingPoint.Value);
        }

        internal TypedExpression Accept(Group group)
        {
            var resolvedExpression = group.Expression.Visit(this);
            return new TypedGroup(group, resolvedExpression.ResolvedType, resolvedExpression);
        }



        internal TypedStatement Accept(VariableDeclaration variableDeclaration)
        {
            var variableType = Resolve(variableDeclaration.TypeSymbol);
            var initializer = variableDeclaration.Initializer.Visit(this);
            if (_localVariables.ContainsKey(variableDeclaration.Identifier.Lexeme) || CurrentFunction.Parameters.Any(x => x.ParameterName.Lexeme == variableDeclaration.Identifier.Lexeme)) 
                throw new ParsingException(variableDeclaration.Identifier, $"identifier {variableDeclaration.Identifier} is already defined");
            _localVariables[variableDeclaration.Identifier.Lexeme] = variableType;
            if (!variableType.Is(initializer.ResolvedType))
                throw new ParsingException(variableDeclaration.Identifier, $"initializer of type {initializer.ResolvedType} cannot be converted to type {variableType}");
            return new TypedVariableDeclaration(variableDeclaration, variableType, variableDeclaration.Identifier, initializer);
        }

        internal TypedStatement Accept(IfStatement ifStatement)
        {
            var condition = ifStatement.Condition.Visit(this);
            var thenDo = ifStatement.ThenDo.Visit(this);
            var elseDo = ifStatement.ElseDo?.Visit(this);
            return new TypedIfStatement(ifStatement, condition, thenDo, elseDo);
        }

        internal TypedStatement Accept(WhileStatement whileStatement)
        {
            var condition = whileStatement.Condition.Visit(this);
            _loops.Push(new());
            var thenDo = whileStatement.ThenDo.Visit(this);
            _loops.Pop();
            return new TypedWhileStatement(whileStatement, condition, thenDo);
        }

        internal TypedStatement Accept(Block block)
        {
            var statements = block.Statements.Select(x => x.Visit(this)).ToList();
            return new TypedBlock(block, statements);
        }

        internal TypedStatement Accept(Break @break)
        {
            if (_loops.Count > 0) return new TypedBreak(@break);
            throw new ParsingException(@break.Token, $"break is only supported inside of loop");
        }

        internal TypedStatement Accept(Continue @continue)
        {
            if (_loops.Count > 0) return new TypedContinue(@continue);
            throw new ParsingException(@continue.Token, $"continue is only supported inside of loop");
        }

        internal TypedStatement Accept(ReturnStatement returnStatement)
        {
            if (returnStatement.ValueToReturn == null)
            {
                if (CurrentFunction.ReturnType.Is(SupportedType.Void) && CurrentFunction.ReturnType.UnderlyingType == null) // TODO
                {
                    return new TypedReturnStatement(returnStatement, null);
                }
                throw new ParsingException(returnStatement.Token, $"expected return value to be of type {CurrentFunction.ReturnType} but got void");
            }
            var valueToReturn = returnStatement.ValueToReturn.Visit(this);
            if (!CurrentFunction.ReturnType.Is(valueToReturn.ResolvedType))
                throw new ParsingException(returnStatement.Token, $"expected return value of type {CurrentFunction.ReturnType} but got {valueToReturn.ResolvedType}");
            return new TypedReturnStatement(returnStatement, valueToReturn);
        }

        internal TypedDeclaration Accept(FunctionDeclaration functionDeclaration)
        {
            var returnType = Resolve(functionDeclaration.ReturnType);
            var parameters = functionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(functionDeclaration.Token, $"parameter names must not repeat");

            var function = new TypedFunctionDeclaration(functionDeclaration, returnType, functionDeclaration.FunctionIdentifier, parameters, new(), false, functionDeclaration.CallingConvention, functionDeclaration.IsExport, functionDeclaration.ExportedAlias);
            
            if (_currentFunction != null) throw new InvalidOperationException();
            _currentFunction = function;
            foreach(var statement in functionDeclaration.Body)
            {
                _currentFunction.Body.Add(statement.Visit(this));
            }
            _functions[functionDeclaration.FunctionIdentifier.Lexeme] = _currentFunction;
            _currentFunction = null;
            _localVariables.Clear();
            return function;
        }

        internal TypedDeclaration Accept(ImportLibraryDeclaration importLibraryDeclaration)
        {
            if (_importedLibraries.ContainsKey(importLibraryDeclaration.LibraryAlias.Lexeme))
                throw new ParsingException(importLibraryDeclaration.LibraryAlias, $"library with alias {importLibraryDeclaration.LibraryAlias.Lexeme} already exists");
            var importLibrary = new TypedImportLibraryDeclaration(importLibraryDeclaration, importLibraryDeclaration.LibraryAlias, importLibraryDeclaration.LibraryPath);
            _importedLibraries[importLibrary.LibraryAlias.Lexeme] = importLibrary;
            return importLibrary;
        }

        internal TypedDeclaration Accept(ImportedFunctionDeclaration importedFunctionDeclaration)
        {
            var returnType = Resolve(importedFunctionDeclaration.ReturnType);
            var parameters = importedFunctionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(importedFunctionDeclaration.Token, $"parameter names must not repeat");

            if (!_importedLibraries.ContainsKey(importedFunctionDeclaration.LibraryAlias.Lexeme))
                throw new ParsingException(importedFunctionDeclaration.LibraryAlias, $"library with alias {importedFunctionDeclaration.LibraryAlias} is not defined");

            var importedFunction = new TypedImportedFunctionDeclaration(importedFunctionDeclaration, returnType, importedFunctionDeclaration.FunctionIdentifier, parameters, importedFunctionDeclaration.CallingConvention, importedFunctionDeclaration.LibraryAlias, importedFunctionDeclaration.FunctionSymbol);

            _importedFunctions[importedFunction.FunctionIdentifier.Lexeme] = importedFunction;
            return importedFunction;
        }

        internal TypedExpression Accept(Assignment assignment)
        {
            if (assignment.Instance != null) throw new NotImplementedException();
            var valueToAssign = assignment.ValueToAssign.Visit(this);
            var foundParameter = CurrentFunction.Parameters.Find(x => x.ParameterName.Lexeme == assignment.AssignmentTarget.Lexeme);
            if (foundParameter != null)
            {
                if (!foundParameter.ResolvedType.Is(valueToAssign.ResolvedType))
                    throw new ParsingException(assignment.Token, $"unable to assign value of type {valueToAssign.ResolvedType} to identifier of type {foundParameter.ResolvedType}");
                return new TypedAssignment(assignment, foundParameter.ResolvedType, assignment.AssignmentTarget, valueToAssign);
            }
            if (!_localVariables.TryGetValue(assignment.AssignmentTarget.Lexeme, out var variable))
                throw new ParsingException(assignment.AssignmentTarget, $"symbol {assignment.AssignmentTarget.Lexeme} is not defined");
            return new TypedAssignment(assignment, variable, assignment.AssignmentTarget, valueToAssign);
                
        }

        internal TypedStatement Accept(ExpressionStatement expressionStatement)
        {
            var expression = expressionStatement.Expression.Visit(this);
            return new TypedEpressionStatement(expressionStatement, expression.ResolvedType, expressionStatement.Token, expression);
        }

        internal TypedExpression Accept(Reference reference)
        {
            var foundParameter = CurrentFunction.Parameters.Find(x => x.ParameterName.Lexeme == reference.Token.Lexeme);
            if (foundParameter != null)
            {
                return new Shared.TypedReference(reference, ResolvedType.Create(SupportedType.Ptr, foundParameter.ResolvedType), reference.Token);
            }
            if (!_localVariables.TryGetValue(reference.Token.Lexeme, out var variable))
                throw new ParsingException(reference.Token, $"symbol {reference.Token.Lexeme} is not defined");
            return new Shared.TypedReference(reference, ResolvedType.Create(SupportedType.Ptr, variable), reference.Token);
        }

        internal TypedExpression Accept(Dereference dereference)
        {
            var rhs = dereference.Rhs.Visit(this);
            if (!rhs.ResolvedType.IsPointer)
            {
                throw new ParsingException(dereference.Token, $"expect right hand side of dereference to be a pointer type");
            }
            return new TypedDereference(dereference, rhs.ResolvedType.UnderlyingType ?? throw new InvalidOperationException(), rhs);
        }

        internal TypedExpression Accept(GetFromReference getFromReference)
        {
            throw new NotImplementedException();
        }

        internal TypedExpression Accept(DereferenceAssignment dereferenceAssignment)
        {
            var valueToAssign = dereferenceAssignment.ValueToAssign.Visit(this);
            var assignmentTarget = dereferenceAssignment.AssignmentTarget.Visit(this);
            if (!(assignmentTarget is TypedDereference typedDereference)) throw new ParsingException(dereferenceAssignment.Token, "unexpected left hand side of assignment");

            if (!assignmentTarget.ResolvedType.Is(valueToAssign.ResolvedType))
                throw new ParsingException(dereferenceAssignment.Token, $"expect value of type {assignmentTarget.ResolvedType} but got {valueToAssign.ResolvedType} on right hand side of assignment");
            return new TypedDereferenceAssignment(dereferenceAssignment, assignmentTarget.ResolvedType, typedDereference, valueToAssign);
        }
    }
}
