﻿using PlatinumC.Compiler;
using PlatinumC.Compiler.TargetX86.Instructions;
using PlatinumC.Interfaces;
using System.Runtime.InteropServices;
using TokenizerCore.Interfaces;
using static PlatinumC.Shared.TypedFunctionDeclaration;

namespace PlatinumC.Shared
{
    public class TypedDeclaration: IVisitable<X86CompilationContext>
    {
        public Declaration OriginalDeclaration { get; set; }

        public TypedDeclaration(Declaration originalDeclaration)
        {
            OriginalDeclaration = originalDeclaration;
        }

        public virtual void Visit(X86CompilationContext context)
        {

        }
    }

    public class TypedTypeDeclaration : TypedDeclaration
    {
        public class TypedFieldDeclaration
        {
            public IToken FieldName { get; set; }
            public ResolvedType ResolvedType { get; set; }
            public TypedFieldDeclaration(IToken fieldName, ResolvedType resolvedType)
            {
                FieldName = fieldName;
                ResolvedType = resolvedType;
            }

        }
        public IToken ClassName { get; set; }
        public List<TypedFieldDeclaration> FieldDeclarations { get; set; }
        public TypedTypeDeclaration(Declaration originalDeclaration, IToken className, List<TypedFieldDeclaration> fieldDeclarations) : base(originalDeclaration)
        {
            ClassName = className;
            FieldDeclarations = fieldDeclarations;
        }
    }

    public class TypedFunctionDeclaration : TypedDeclaration
    {
        public class TypedParameterDeclaration
        {
            public IToken ParameterName { get; set; }
            public ResolvedType ResolvedType { get; set; }
            public TypedParameterDeclaration(IToken parameterName, ResolvedType resolvedType)
            {
                ParameterName = parameterName;
                ResolvedType= resolvedType;
            }
        }
        public ResolvedType ReturnType { get; set; }
        public IToken FunctionIdentifier { get; set; }
        public List<TypedParameterDeclaration> Parameters { get; set; }
        public List<TypedStatement> Body { get; set; }
        public bool IsImport { get; set; }
        public CallingConvention CallingConvention { get; set; }
        private List<TypedVariableDeclaration> _localVariableDeclarations = new();
        public List<TypedVariableDeclaration> LocalVariableDeclarations => _localVariableDeclarations;
        public bool IsExport { get; set; }
        public IToken ExportedSymbol { get; set; }
        public TypedFunctionDeclaration(Declaration originalDeclaration, ResolvedType returnType, IToken functionIdentifier, List<TypedParameterDeclaration> parameters, List<TypedStatement> body, bool isImport, CallingConvention callingConvention, bool isExport, IToken exportedSymbol) : base(originalDeclaration)
        {
            ReturnType = returnType;
            FunctionIdentifier = functionIdentifier;
            Parameters = parameters;
            Body = body;
            IsImport = isImport;
            CallingConvention = callingConvention;
            IsExport = isExport;
            ExportedSymbol = exportedSymbol;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            foreach (var statement in Body) statement.Visit(this);
            context.EnterFunction(this);
            context.AddInstruction(X86Instructions.Label(GetDecoratedFunctionIdentifier()));
            context.AddInstruction(X86Instructions.Push(X86Register.ebp, false));
            context.AddInstruction(X86Instructions.Mov(X86Register.ebp, X86Register.esp));
            context.AddInstruction(X86Instructions.Sub(X86Register.esp, 4 * (context.CurrentFunction.LocalVariables.Count + 1)));

            foreach(var statement in Body)
            {
                statement.Visit(context);
            }

            context.ExitFunction();
        }


        public void AddLocalVariable(TypedVariableDeclaration variableDeclaration)
        {
            _localVariableDeclarations.Add(variableDeclaration);
        }

        public string GetDecoratedFunctionIdentifier()
        {
            if (CallingConvention == CallingConvention.Cdecl) return $"_{FunctionIdentifier.Lexeme}";
            if (CallingConvention == CallingConvention.StdCall) return $"_{FunctionIdentifier.Lexeme}@{Parameters.Count * 4}";
            throw new NotImplementedException();
        }
    }

    public class TypedImportLibraryDeclaration : TypedDeclaration
    {
        public IToken LibraryAlias { get; set; }
        public IToken LibraryPath { get; set; }

        public TypedImportLibraryDeclaration(ImportLibraryDeclaration originalDeclaration, IToken libraryAlias, IToken libraryPath): base(originalDeclaration)
        {
            LibraryAlias = libraryAlias;
            LibraryPath = libraryPath;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            context.AddImportLibrary(this);
        }

    }

    public class TypedImportedFunctionDeclaration : TypedDeclaration
    {
        public ResolvedType ReturnType { get; set; }

        public IToken FunctionIdentifier { get; set; }
        public List<TypedParameterDeclaration> Parameters { get; set; }
        public CallingConvention CallingConvention { get; set; }
        public IToken LibraryAlias { get; set; }
        public IToken FunctionSymbol { get; set; }

        public TypedImportedFunctionDeclaration(ImportedFunctionDeclaration originalDeclaration, ResolvedType returnType, IToken functionIdentifier, List<TypedParameterDeclaration> parameters, CallingConvention callingConvention, IToken libraryAlias, IToken functionSymbol) : base(originalDeclaration)
        {
            ReturnType = returnType;
            FunctionIdentifier = functionIdentifier;
            Parameters = parameters;
            CallingConvention = callingConvention;
            LibraryAlias = libraryAlias;
            FunctionSymbol = functionSymbol;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            context.AddImportedFunction(this);
        }

    }
}
