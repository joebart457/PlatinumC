using ParserLite.Exceptions;
using PlatinumC.Parser;
using PlatinumC.Shared;
using TokenizerCore.Interfaces;
using static PlatinumC.Shared.TypedFunctionDeclaration;

namespace PlatinumC.Resolver
{
    public class TypeResolver
    {
        private Dictionary<string, ResolvedType> _localVariables { get; set; } = new();
        private Dictionary<string, ResolvedType> _globalVariables { get; set;} = new();
        private Dictionary<string, TypedFunctionDeclaration> _functions { get; set; } = new();
        private Dictionary<string, TypedImportedFunctionDeclaration> _importedFunctions { get; set; } = new();
        private TypedFunctionDeclaration? _currentFunction;
        private TypedFunctionDeclaration CurrentFunction => _currentFunction ?? throw new Exception("statement is invalid outside of function body");
        private Dictionary<string, TypedImportLibraryDeclaration> _importedLibraries = new();
        private class LoopInfo {}
        private Stack<LoopInfo> _loops = new();
        private Dictionary<string, ResolvedType> _customTypes = new();
        public ResolverResult ResolveTypes(ParsingResult parsingResult)
        {
            _localVariables = new();
            _globalVariables = new();
            _functions = new();
            _importedFunctions = new();
            _currentFunction = null;
            _importedLibraries = new();
            _loops = new();
            _customTypes = new();
            _currentFunction = null;
            var resolvedDeclarations = new List<TypedDeclaration>();
            foreach(var declaration in parsingResult.Declarations)
            {
                if (declaration is ImportLibraryDeclaration importLibraryDeclaration)
                    resolvedDeclarations.Add(Accept(importLibraryDeclaration));
                if (declaration is TypeDeclaration typeDeclaration)
                    GatherDefinition(typeDeclaration);
            }

            foreach (var declaration in parsingResult.Declarations)
            {

                if (declaration is TypeDeclaration typeDeclaration)
                    resolvedDeclarations.Add(Accept(typeDeclaration));
            }


            foreach (var declaration in parsingResult.Declarations)
            {
                if (declaration is ImportedFunctionDeclaration importedFunctionDeclaration)
                    GatherDefinition(importedFunctionDeclaration);
                if (declaration is FunctionDeclaration functionDeclaration)
                    GatherDefinition(functionDeclaration);
            }


            foreach (var declaration in parsingResult.Declarations)
            {
                if (!(declaration is ImportLibraryDeclaration || declaration is TypeDeclaration))
                    resolvedDeclarations.Add(declaration.Visit(this));
            }
            return new ResolverResult(resolvedDeclarations);
        }

        internal void GatherDefinition(FunctionDeclaration functionDeclaration)
        {
            var returnType = Resolve(functionDeclaration.ReturnType);
            var parameters = functionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(functionDeclaration.Token, $"parameter names must not repeat");
            var parameterWithCustomType = parameters.Find(x => x.ResolvedType.IsCustomType);
            if (parameterWithCustomType != null) 
                throw new ParsingException(parameterWithCustomType.ParameterName, $"custom types cannot be used as function parameters, use pointers to custom types instead. (Parameter: {parameterWithCustomType.ParameterName})");
            var function = new TypedFunctionDeclaration(functionDeclaration, returnType, functionDeclaration.FunctionIdentifier, parameters, new(), false, functionDeclaration.CallingConvention, functionDeclaration.IsExport, functionDeclaration.ExportedAlias);
            if (_functions.ContainsKey(functionDeclaration.FunctionIdentifier.Lexeme) || _importedFunctions.ContainsKey(functionDeclaration.FunctionIdentifier.Lexeme))
                throw new ParsingException(functionDeclaration.FunctionIdentifier, $"function {functionDeclaration.FunctionIdentifier.Lexeme} is already defined");
            
            _functions[functionDeclaration.FunctionIdentifier.Lexeme] = function;
        }

        internal void GatherDefinition(ImportedFunctionDeclaration importedFunctionDeclaration)
        {
            var returnType = Resolve(importedFunctionDeclaration.ReturnType);
            var parameters = importedFunctionDeclaration.Parameters.Select(x => new TypedParameterDeclaration(x.ParameterName, Resolve(x.ParameterType))).ToList();
            if (parameters.GroupBy(x => x.ParameterName.Lexeme).Any(x => x.Count() > 1)) throw new ParsingException(importedFunctionDeclaration.Token, $"parameter names must not repeat");
            var parameterWithCustomType = parameters.Find(x => x.ResolvedType.IsCustomType);
            if (parameterWithCustomType != null)
                throw new ParsingException(parameterWithCustomType.ParameterName, $"custom types cannot be used as function parameters, consider splitting into individual fields if external function is defined this way. (Parameter: {parameterWithCustomType.ParameterName})");
            if (!_importedLibraries.ContainsKey(importedFunctionDeclaration.LibraryAlias.Lexeme))
                throw new ParsingException(importedFunctionDeclaration.LibraryAlias, $"library with alias {importedFunctionDeclaration.LibraryAlias} is not defined");

            var importedFunction = new TypedImportedFunctionDeclaration(importedFunctionDeclaration, returnType, importedFunctionDeclaration.FunctionIdentifier, parameters, importedFunctionDeclaration.CallingConvention, importedFunctionDeclaration.LibraryAlias, importedFunctionDeclaration.FunctionSymbol);
            if (_importedFunctions.ContainsKey(importedFunction.FunctionIdentifier.Lexeme) || _functions.ContainsKey(importedFunction.FunctionIdentifier.Lexeme))
                throw new ParsingException(importedFunction.FunctionIdentifier, $"function {importedFunction.FunctionIdentifier} is already defined");
            _importedFunctions[importedFunction.FunctionIdentifier.Lexeme] = importedFunction;
        }

        internal void GatherDefinition(TypeDeclaration typeDeclaration)
        {
            if (_customTypes.ContainsKey(typeDeclaration.TypeName.Lexeme))
                throw new ParsingException(typeDeclaration.TypeName, $"type with name {typeDeclaration.TypeName.Lexeme} already exists!");
            
            _customTypes[typeDeclaration.TypeName.Lexeme] = ResolvedType.Create(typeDeclaration.TypeName, new());
        }

        internal TypedTypeDeclaration Accept(TypeDeclaration typeDeclaration)
        {
            if (!_customTypes.TryGetValue(typeDeclaration.TypeName.Lexeme, out var partialTypeDeclaration))
                throw new ParsingException(typeDeclaration.TypeName, $"type with name {typeDeclaration.TypeName.Lexeme} does not exist!");
            var resolvedFields = new List<TypedTypeDeclaration.TypedFieldDeclaration>();
            foreach (var field in typeDeclaration.FieldDeclarations)
            {
                if (resolvedFields.Any(x => x.FieldName.Lexeme == field.FieldName.Lexeme))
                    throw new ParsingException(field.FieldName, $"field with name {field.FieldName.Lexeme} already exists on type {typeDeclaration.TypeName.Lexeme}");
                var resolvedType = Resolve(field.TypeSymbol);
                if (resolvedType.Is(partialTypeDeclaration)) throw new ParsingException(field.FieldName, $"illegal recursive type declaration for type {typeDeclaration.TypeName.Lexeme}, field {field.FieldName.Lexeme}");
                resolvedFields.Add(new(field.FieldName, resolvedType));
                partialTypeDeclaration.Fields.Add((field.FieldName.Lexeme, resolvedType));
            }
            return new TypedTypeDeclaration(typeDeclaration, typeDeclaration.TypeName, resolvedFields);
        }


        private ResolvedType Resolve(TypeSymbol typeSymbol)
        {
            if (typeSymbol.SupportedType == SupportedType.Ptr)
            {
                if (typeSymbol.UnderlyingType == null) throw new InvalidOperationException();
                return ResolvedType.CreatePointer(Resolve(typeSymbol.UnderlyingType));
            }
            else if (typeSymbol.SupportedType == SupportedType.Custom)
            {
                if (_customTypes.TryGetValue(typeSymbol.Token.Lexeme, out var resolvedType)) return resolvedType;
                throw new ParsingException(typeSymbol.Token, $"type {typeSymbol.Token.Lexeme} is not defined");
            }
            else if (typeSymbol.SupportedType == SupportedType.Array)
            {
                if (typeSymbol.UnderlyingType == null) throw new InvalidOperationException();
                return ResolvedType.CreateArray(Resolve(typeSymbol.UnderlyingType), typeSymbol.ArraySize);
            }
            return ResolvedType.Create(typeSymbol.SupportedType);
        }

        internal TypedExpression Accept(Identifier identifier)
        {
            if (_localVariables.TryGetValue(identifier.Token.Lexeme, out var resolvedType)) return new TypedIdentifier(identifier, resolvedType, identifier.Token);
        
            var foundVariable = CurrentFunction.Parameters.Find(x => x.ParameterName.Lexeme == identifier.Token.Lexeme);
            if (foundVariable == null)
            {
                if (_globalVariables.TryGetValue(identifier.Token.Lexeme, out var globalVariableType)) return new TypedGlobalIdentifier(identifier, globalVariableType, identifier.Token);
                throw new ParsingException(identifier.Token, $"identifier {identifier.Token.Lexeme} is not defined");
            }
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
            else if (lhs.ResolvedType.IsPointer)
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
            else if (lhs.ResolvedType.IsPointer)
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
                if (!(rhs.ResolvedType.Is(SupportedType.Int) || rhs.ResolvedType.IsPointer)) throw new ParsingException(binaryComparison.Token, $"right hand side of integer comparison must be another integer or a pointer");
                return new TypedBinaryComparison_Integer_Integer(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Float))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Float)) throw new ParsingException(binaryComparison.Token, $"right hand side of floating point addition must also be floating point");
                return new TypedBinaryComparison_Float_Float(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else if (lhs.ResolvedType.IsPointer)
            {
                if (!(rhs.ResolvedType.Is(SupportedType.Int) || rhs.ResolvedType.Is(lhs.ResolvedType))) throw new ParsingException(binaryComparison.Token, $"right hand side of pointer comparison must be an integer or another pointer");
                return new TypedBinaryComparison_Integer_Integer(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
            }
            else if (lhs.ResolvedType.Is(SupportedType.Byte))
            {
                if (!rhs.ResolvedType.Is(SupportedType.Byte)) throw new ParsingException(binaryComparison.Token, $"right hand side of byte comparison must be another byte");
                return new TypedBinaryComparison_Byte_Byte(binaryComparison, ResolvedType.Create(SupportedType.Int), lhs, rhs, binaryComparison.ComparisonType);
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

        internal TypedExpression Accept(BinaryBitwiseAnd binaryBitwiseAnd)
        {
            var lhs = binaryBitwiseAnd.Lhs.Visit(this);
            var rhs = binaryBitwiseAnd.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Int))
                return new TypedBinaryAnd_Integer_Integer(binaryBitwiseAnd, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            if (lhs.ResolvedType.Is(SupportedType.Byte) && rhs.ResolvedType.Is(SupportedType.Byte))
                return new TypedBinaryAnd_Byte_Byte(binaryBitwiseAnd, ResolvedType.Create(SupportedType.Byte), lhs, rhs);
            throw new ParsingException(binaryBitwiseAnd.Token, $"bitwise and is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryBitwiseOr binaryBitwiseOr)
        {
            var lhs = binaryBitwiseOr.Lhs.Visit(this);
            var rhs = binaryBitwiseOr.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Int))
                return new TypedBinaryOr_Integer_Integer(binaryBitwiseOr, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            if (lhs.ResolvedType.Is(SupportedType.Byte) && rhs.ResolvedType.Is(SupportedType.Byte))
                return new TypedBinaryOr_Byte_Byte(binaryBitwiseOr, ResolvedType.Create(SupportedType.Byte), lhs, rhs);
            throw new ParsingException(binaryBitwiseOr.Token, $"bitwise or is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(BinaryBitwiseXor binaryBitwiseXor)
        {
            var lhs = binaryBitwiseXor.Lhs.Visit(this);
            var rhs = binaryBitwiseXor.Rhs.Visit(this);
            if (lhs.ResolvedType.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Int))
                return new TypedBinaryXor_Integer_Integer(binaryBitwiseXor, ResolvedType.Create(SupportedType.Int), lhs, rhs);
            if (lhs.ResolvedType.Is(SupportedType.Byte) && rhs.ResolvedType.Is(SupportedType.Byte))
                return new TypedBinaryXor_Byte_Byte(binaryBitwiseXor, ResolvedType.Create(SupportedType.Byte), lhs, rhs);
            throw new ParsingException(binaryBitwiseXor.Token, $"bitwise xor is not supported for types {lhs.ResolvedType} and {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(LiteralString literalString)
        {
            return new TypedLiteralString(literalString, new ResolvedType(SupportedType.Ptr, ResolvedType.Create(SupportedType.Byte)), literalString.Value);
        }

        internal TypedExpression Accept(LiteralNullPointer literalNullPointer)
        {
            return new TypedLiteralNullPointer(literalNullPointer, new ResolvedType(SupportedType.Ptr, ResolvedType.Create(SupportedType.Void)));
        }

        internal TypedExpression Accept(LiteralInteger literalInteger)
        {
            return new TypedLiteralInteger(literalInteger, ResolvedType.Create(SupportedType.Int), literalInteger.Value);
        }

        internal TypedExpression Accept(LiteralFloatingPoint literalFloatingPoint)
        {
            return new TypedLiteralFloatingPoint(literalFloatingPoint, ResolvedType.Create(SupportedType.Float), literalFloatingPoint.Value);
        }

        internal TypedExpression Accept(LiteralByte literalByte)
        {
            return new TypedLiteralByte(literalByte, ResolvedType.Create(SupportedType.Byte), literalByte.Value);
        }

        internal TypedExpression Accept(Group group)
        {
            var resolvedExpression = group.Expression.Visit(this);
            return new TypedGroup(group, resolvedExpression.ResolvedType, resolvedExpression);
        }

        internal TypedExpression Accept(Cast cast)
        {
            var typeToCastTo = Resolve(cast.TypeToCastTo);
            var rhs = cast.Rhs.Visit(this);
            if (typeToCastTo.Equals(rhs.ResolvedType)) return rhs; // Types are the same, no cast needed
            if (typeToCastTo.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Float)) return new TypedCast_Integer_From_Float(cast, ResolvedType.Create(SupportedType.Int), rhs);
            if (typeToCastTo.Is(SupportedType.Int) && rhs.ResolvedType.Is(SupportedType.Byte)) return new TypedCast_Integer_From_Byte(cast, ResolvedType.Create(SupportedType.Int), rhs);
            if (typeToCastTo.Is(SupportedType.Float) && rhs.ResolvedType.Is(SupportedType.Int)) return new TypedCast_Float_From_Integer(cast, ResolvedType.Create(SupportedType.Float), rhs);
            if (typeToCastTo.Is(SupportedType.Void)) throw new ParsingException(cast.Token, "cannot cast to void type");
            if (typeToCastTo.IsPointer && rhs.ResolvedType.IsPointer)
            {
                if (typeToCastTo.UnderlyingType!.Is(SupportedType.Void) || rhs.ResolvedType.UnderlyingType!.Is(SupportedType.Void)) return new TypedCast_Pointer_From_Pointer(cast, typeToCastTo, rhs);
                if (typeToCastTo.ReferencedTypeSize == rhs.ResolvedType.ReferencedTypeSize) return new TypedCast_Pointer_From_Pointer(cast, typeToCastTo, rhs);
                throw new ParsingException(cast.Token, $"unable to cast from pointer type {rhs.ResolvedType} to type {typeToCastTo}. Underlying types must be of equal size.");
            }
            throw new ParsingException(cast.Token, $"unable to cast type {rhs.ResolvedType} to type {typeToCastTo}");
        }



        internal TypedStatement Accept(VariableDeclaration variableDeclaration)
        {
            var variableType = Resolve(variableDeclaration.TypeSymbol);
            var initializer = variableDeclaration.Initializer?.Visit(this);
            if (_localVariables.ContainsKey(variableDeclaration.Identifier.Lexeme) || CurrentFunction.Parameters.Any(x => x.ParameterName.Lexeme == variableDeclaration.Identifier.Lexeme)) 
                throw new ParsingException(variableDeclaration.Identifier, $"identifier {variableDeclaration.Identifier} is already defined");
            _localVariables[variableDeclaration.Identifier.Lexeme] = variableType;
            if (initializer != null && !variableType.Is(initializer.ResolvedType))
                throw new ParsingException(variableDeclaration.Identifier, $"initializer of type {initializer.ResolvedType} cannot be converted to type {variableType}");
            if (variableType.IsArray && initializer != null) throw new ParsingException(variableDeclaration.Identifier, "cannot provide initializer for array");
            return new TypedVariableDeclaration(variableDeclaration, variableType, variableDeclaration.Identifier, initializer);
        }

        internal TypedStatement Accept(IfStatement ifStatement)
        {
            var condition = ifStatement.Condition.Visit(this);
            if (!(condition.ResolvedType.Is(SupportedType.Int) || condition.ResolvedType.Is(SupportedType.Byte)))
                throw new ParsingException(ifStatement.Token, "expect condition to resolve to integer or byte type");
            var thenDo = ifStatement.ThenDo.Visit(this);
            var elseDo = ifStatement.ElseDo?.Visit(this);
            return new TypedIfStatement(ifStatement, condition, thenDo, elseDo);
        }

        internal TypedStatement Accept(WhileStatement whileStatement)
        {
            var condition = whileStatement.Condition.Visit(this);
            if (!(condition.ResolvedType.Is(SupportedType.Int) || condition.ResolvedType.Is(SupportedType.Byte)))
                throw new ParsingException(whileStatement.Token, "expect condition to resolve to integer or byte type");
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
            var encounteredReturn = VerifyAllBranchesReturn(_currentFunction.Body);
            if (!encounteredReturn)
            {
                if (_currentFunction.ReturnType.Is(SupportedType.Void)) _currentFunction.Body.Add(new TypedReturnStatement(new ReturnStatement(functionDeclaration.FunctionIdentifier, null), null));
                else throw new ParsingException(functionDeclaration.FunctionIdentifier, $"function {functionDeclaration.FunctionIdentifier.Lexeme} not all codepaths return a value");
            }
            _functions[functionDeclaration.FunctionIdentifier.Lexeme] = _currentFunction;
            _currentFunction = null;
            _localVariables.Clear();
            return function;
        }

        private bool VerifyAllBranchesReturn(List<TypedStatement> statements)
        {
            bool encounteredReturn = false;
            foreach(var statement in statements)
            {
                if (statement is TypedReturnStatement) encounteredReturn = true;
                if (statement is TypedIfStatement ifStatement)
                {
                    bool elseReturns = ifStatement.ElseDo == null ?  true : VerifyAllBranchesReturn([ifStatement.ElseDo]);
                    bool returnedInBothBranches = VerifyAllBranchesReturn([ifStatement.ThenDo]) && elseReturns;
                    if (statement == statements.Last())
                    {
                        if (!returnedInBothBranches && !CurrentFunction.ReturnType.Is(SupportedType.Void)) throw new ParsingException(ifStatement.OriginalStatement.Token, $"not all branches return a value");
                    }
                    if (returnedInBothBranches) encounteredReturn = true;
                }
                if (statement is TypedWhileStatement whileStatement) encounteredReturn |= VerifyAllBranchesReturn([whileStatement.ThenDo]);
                if (statement is TypedBlock typedBlock) encounteredReturn |= VerifyAllBranchesReturn(typedBlock.Statements);
            }
            return encounteredReturn;
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
            var assignmentTarget = assignment.AssignmentTarget.Visit(this);
            var valueToAssign = assignment.ValueToAssign.Visit(this);
            if (!assignmentTarget.ResolvedType.Is(valueToAssign.ResolvedType)) 
                throw new ParsingException(assignment.Token, $"unable to assign value of type {valueToAssign.ResolvedType} to identifier of type {assignmentTarget.ResolvedType}");
            if (assignmentTarget.ResolvedType.IsArray)
                throw new ParsingException(assignment.Token, "unable to assign directly to array type");

            return new TypedAssignment(assignment, assignmentTarget.ResolvedType, assignmentTarget, valueToAssign);
        }

        internal TypedStatement Accept(ExpressionStatement expressionStatement)
        {
            var expression = expressionStatement.Expression.Visit(this);
            return new TypedEpressionStatement(expressionStatement, expression.ResolvedType, expressionStatement.Token, expression);
        }

        internal TypedExpression Accept(Reference reference)
        {
            if (_localVariables.TryGetValue(reference.Token.Lexeme, out var resolvedType)) return new Shared.TypedReference(reference, ResolvedType.CreatePointer(resolvedType), reference.Token);

            var foundParameter = CurrentFunction.Parameters.Find(x => x.ParameterName.Lexeme == reference.Token.Lexeme);
            if (foundParameter == null)
            {
                if (_globalVariables.TryGetValue(reference.Token.Lexeme, out var globalVariableType)) return new TypedGlobalReference(reference, globalVariableType, reference.Token);
                throw new ParsingException(reference.Token, $"identifier {reference.Token.Lexeme} is not defined");
            }
            return new Shared.TypedReference(reference, ResolvedType.CreatePointer(foundParameter.ResolvedType), reference.Token);
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
            var instance = getFromReference.Instance.Visit(this);
            if (!instance.ResolvedType.IsPointer || (!instance.ResolvedType.UnderlyingType!.IsCustomType))
                throw new ParsingException(getFromReference.Token, $"left hand side of field access must be pointer");
            var field = instance.ResolvedType.UnderlyingType!.Fields.Find(x => x.fieldName == getFromReference.MemberTarget.Lexeme);
            if (field == default) throw new ParsingException(getFromReference.MemberTarget, $"field {getFromReference.MemberTarget.Lexeme} does not exist on type {instance.ResolvedType.UnderlyingType}");

            return new TypedGetFromReference(getFromReference, field.fieldType, instance, getFromReference.MemberTarget);
        }

        internal TypedExpression Accept(GetFromLocalStruct getFromLocalStruct)
        {
            var instance = getFromLocalStruct.Instance.Visit(this);
            if (!instance.ResolvedType.IsCustomType)
                throw new ParsingException(getFromLocalStruct.Token, $"type {instance.ResolvedType} does not contain any members");
            var field = instance.ResolvedType.Fields.Find(x => x.fieldName == getFromLocalStruct.MemberTarget.Lexeme);
            if (field == default) throw new ParsingException(getFromLocalStruct.MemberTarget, $"field {getFromLocalStruct.MemberTarget.Lexeme} does not exist on type {instance.ResolvedType.UnderlyingType}");

            return new TypedGetFromLocalStruct(getFromLocalStruct, field.fieldType, instance, getFromLocalStruct.MemberTarget);
        }
   
        internal TypedExpression Accept(UnaryNegation unaryNegation)
        {
            var rhs = unaryNegation.Rhs.Visit(this);

            if (rhs.ResolvedType.Is(SupportedType.Int)) return new TypedUnary_Negation_Integer(unaryNegation, ResolvedType.Create(SupportedType.Int), rhs);
            if (rhs.ResolvedType.Is(SupportedType.Byte)) return new TypedUnary_Negation_Integer(unaryNegation, ResolvedType.Create(SupportedType.Byte), rhs);

            throw new ParsingException(unaryNegation.Token, $"unable to perform negation on type {rhs.ResolvedType}");
        }

        internal TypedExpression Accept(UnaryNot unaryNot)
        {
            var rhs = unaryNot.Rhs.Visit(this);

            if (rhs.ResolvedType.Is(SupportedType.Int)) return new TypedUnary_Negation_Integer(unaryNot, ResolvedType.Create(SupportedType.Int), rhs);
            if (rhs.ResolvedType.Is(SupportedType.Byte)) return new TypedUnary_Negation_Integer(unaryNot, ResolvedType.Create(SupportedType.Byte), rhs);

            throw new ParsingException(unaryNot.Token, $"unable to perform bitwise negation on type {rhs.ResolvedType}");
        }

        internal TypedDeclaration Accept(GlobalVariableDeclaration globalVariableDeclaration)
        {
            if (_currentFunction != null) throw new ParsingException(globalVariableDeclaration.Token, "cannot declare global variable inside of function");
            var variableType = Resolve(globalVariableDeclaration.TypeSymbol);
            if (globalVariableDeclaration.Initializer == null)
            {
                return new TypedGlobalVariableDeclaration_UninitializedObject(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier);
            }
            var initializer = globalVariableDeclaration.Initializer.Visit(this);
            if (_globalVariables.ContainsKey(globalVariableDeclaration.Identifier.Lexeme))
                throw new ParsingException(globalVariableDeclaration.Identifier, $"global identifier {globalVariableDeclaration.Identifier} is already defined");
            _globalVariables[globalVariableDeclaration.Identifier.Lexeme] = variableType;
            if (!variableType.Is(initializer.ResolvedType))
                throw new ParsingException(globalVariableDeclaration.Identifier, $"initializer of type {initializer.ResolvedType} cannot be converted to type {variableType}");

            if (initializer is TypedLiteralFloatingPoint typedLiteralFloatingPoint)
                return new TypedGlobalVariableDeclaration_Float(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier, typedLiteralFloatingPoint.Value);
            if (initializer is TypedLiteralInteger typedLiteralInteger)
                return new TypedGlobalVariableDeclaration_Integer(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier, typedLiteralInteger.Value);
            if (initializer is TypedLiteralByte typedLiteralByte)
                return new TypedGlobalVariableDeclaration_Byte(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier, typedLiteralByte.Value);
            if (initializer is TypedLiteralString typedLiteralString)
                return new TypedGlobalVariableDeclaration_String(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier, typedLiteralString.Value);
            if (initializer is TypedLiteralNullPointer typedLiteralNullPointer)
                return new TypedGlobalVariableDeclaration_NullPointer(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier);
            if (initializer is TypedCast_Pointer_From_Pointer typedCast_Pointer_From_Pointer)
            {
                if (initializer is TypedLiteralNullPointer typedLiteralNullPointer1)
                {
                    if (!typedCast_Pointer_From_Pointer.ResolvedType.Is(variableType))
                        throw new ParsingException(globalVariableDeclaration.Identifier, $"unable to assign type {typedCast_Pointer_From_Pointer.ResolvedType} to type {variableType}");
                    return new TypedGlobalVariableDeclaration_NullPointer(globalVariableDeclaration, variableType, globalVariableDeclaration.Identifier);
                }
            }
            throw new ParsingException(globalVariableDeclaration.Identifier, "global variables must have a compile-time constant initializer");
        }

        internal TypedDeclaration Accept(ProgramIconDeclaration programIconDeclaration)
        {
            return new TypedProgramIconDeclaration(programIconDeclaration, programIconDeclaration.IconFilePath);
        }

        internal TypedExpression Accept(BinaryArrayIndex binaryArrayIndex)
        {
            var lhs = binaryArrayIndex.Lhs.Visit(this);
            if (!lhs.ResolvedType.IsArray)
                throw new ParsingException(binaryArrayIndex.Token, "expect left hand side of index to be array type");
            var rhs = binaryArrayIndex.Rhs.Visit(this);
            if (!rhs.ResolvedType.Is(SupportedType.Int))
                throw new ParsingException(binaryArrayIndex.Token, $"expect index of array to be integer type");
            if (rhs is TypedLiteralInteger typedLiteralInteger) return new TypedBinaryArrayIndex_LiteralInteger(binaryArrayIndex, lhs.ResolvedType.UnderlyingType!, lhs, typedLiteralInteger);
            return new  TypedBinaryArrayIndex(binaryArrayIndex, lhs.ResolvedType.UnderlyingType!, lhs, rhs);
        }
    }
}
