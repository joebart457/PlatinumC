using PlatinumC.Compiler;
using PlatinumC.Interfaces;
using PlatinumC.Resolver;
using System.Runtime.InteropServices;
using TokenizerCore.Interfaces;
using static PlatinumC.Shared.FunctionDeclaration;

namespace PlatinumC.Shared
{
    public abstract class Declaration: IVisitable<TypeResolver, TypedDeclaration>
    {
        public IToken Token { get; set; }

        public Declaration(IToken token)
        {
            Token = token;
        }

        public abstract TypedDeclaration Visit(TypeResolver resolver);
    }

    public class TypeDeclaration : Declaration
    {
        public class FieldDeclaration
        {
            public IToken FieldName { get; set; }
            public TypeSymbol TypeSymbol { get; set; }
            public FieldDeclaration(IToken fieldName, TypeSymbol typeSymbol)
            {
                FieldName = fieldName;
                TypeSymbol = typeSymbol;
            }

        }
        public IToken TypeName { get; set; }
        public List<FieldDeclaration> FieldDeclarations { get; set; }
        public TypeDeclaration(IToken token, IToken typeName, List<FieldDeclaration> fieldDeclarations) : base(token)
        {
            TypeName = typeName;
            FieldDeclarations = fieldDeclarations;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class FunctionDeclaration : Declaration
    {
        public class ParameterDeclaration
        {
            public IToken ParameterName { get; set; }
            public TypeSymbol ParameterType { get; set; }
            public ParameterDeclaration(IToken parameterName, TypeSymbol parameterType)
            {
                ParameterName = parameterName;
                ParameterType = parameterType;
            }
        }
        public TypeSymbol ReturnType { get; set; }
        public IToken FunctionIdentifier { get; set; }
        public List<ParameterDeclaration> Parameters { get; set; }
        public List<Statement> Body { get; set; }
        public bool IsExport { get; set; }
        public IToken ExportedAlias { get; set; }
        public CallingConvention CallingConvention { get; set; }
        public FunctionDeclaration(IToken token, TypeSymbol returnType, IToken functionIdentifier, List<ParameterDeclaration> parameters, List<Statement> body, bool isExport, IToken exportedAlias, CallingConvention callingConvention) : base(token)
        {
            ReturnType = returnType;
            FunctionIdentifier = functionIdentifier;
            Parameters = parameters;
            Body = body;
            IsExport = isExport;
            ExportedAlias = exportedAlias;
            CallingConvention = callingConvention;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class ImportLibraryDeclaration : Declaration
    {
        public IToken LibraryAlias { get; set; }
        public IToken LibraryPath { get; set; }

        public ImportLibraryDeclaration(IToken token, IToken libraryAlias, IToken libraryPath) : base(token)
        {
            LibraryAlias = libraryAlias;
            LibraryPath = libraryPath;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class ImportedFunctionDeclaration : Declaration
    {  
        public TypeSymbol ReturnType { get; set; }

        public IToken FunctionIdentifier { get; set; }
        public List<ParameterDeclaration> Parameters { get; set; }   
        public CallingConvention CallingConvention { get; set; }
        public IToken LibraryAlias { get; set; }
        public IToken FunctionSymbol { get; set; }
        public ImportedFunctionDeclaration(IToken token, TypeSymbol returnType, IToken functionIdentifier, List<ParameterDeclaration> parameters, CallingConvention callingConvention, IToken libraryAlias, IToken functionSymbol) : base(token)
        {
            ReturnType = returnType;
            FunctionIdentifier = functionIdentifier;
            Parameters = parameters;
            CallingConvention = callingConvention;
            LibraryAlias = libraryAlias;
            FunctionSymbol = functionSymbol;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class GlobalVariableDeclaration : Declaration
    {
        public TypeSymbol TypeSymbol { get; set; }
        public IToken Identifier { get; set; }
        public Expression? Initializer { get; set; }
        public GlobalVariableDeclaration(IToken token, TypeSymbol typeSymbol, IToken identifier, Expression? initializer) : base(token)
        {
            TypeSymbol = typeSymbol;
            Identifier = identifier;
            Initializer = initializer;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class ProgramIconDeclaration : Declaration
    {
        public IToken IconFilePath { get; set; }
        public ProgramIconDeclaration(IToken token) : base(token)
        {
            IconFilePath = token;
        }

        public override TypedDeclaration Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }
}