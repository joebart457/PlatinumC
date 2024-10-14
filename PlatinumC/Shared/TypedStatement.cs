using PlatinumC.Compiler;
using PlatinumC.Compiler.TargetX86.Instructions;
using PlatinumC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public class TypedStatement: IVisitable<X86CompilationContext>, IVisitable<TypedFunctionDeclaration>
    {
        public Statement OriginalStatement { get; set; }

        public TypedStatement(Statement originalStatement)
        {
            OriginalStatement = originalStatement;
        }

        public virtual void Visit(X86CompilationContext context)
        {

        }

        public virtual void Visit(TypedFunctionDeclaration functionParent)
        {
            
        }
    }

    public class TypedVariableDeclaration : TypedStatement
    {
        public ResolvedType ResolvedType { get; set; }
        public IToken Identifier { get; set; }
        public TypedExpression Initializer { get; set; }
        public TypedVariableDeclaration(Statement originalStatement, ResolvedType resolvedType, IToken identifier, TypedExpression initializer) : base(originalStatement)
        {
            ResolvedType = resolvedType;
            Identifier = identifier;
            Initializer = initializer;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var storageOffset = context.GetIdentifierOffset(Identifier);
            Initializer.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Mov(storageOffset, X86Register.eax));
        }

        public override void Visit(TypedFunctionDeclaration functionParent)
        {
            base.Visit(functionParent);
            functionParent.AddLocalVariable(this);
        }
    }

    public class TypedIfStatement : TypedStatement
    {
        public TypedExpression Condition { get; set; }
        public TypedStatement ThenDo { get; set; }
        public TypedStatement? ElseDo { get; set; }
        public TypedIfStatement(Statement originalStatement, TypedExpression condition, TypedStatement thenDo, TypedStatement? elseDo) : base(originalStatement)
        {
            Condition = condition;
            ThenDo = thenDo;
            ElseDo = elseDo;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var ifLabel = context.CreateLabel();
            var endLabel = context.CreateLabel();



            if (Condition is TypedBinaryComparison_Integer_Integer comparison_Integer_Integer)
            {
                comparison_Integer_Integer.Lhs.Visit(context);
                comparison_Integer_Integer.Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Cmp(X86Register.ebx, X86Register.eax));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.Jz(ifLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.Jnz(ifLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.JmpGt(ifLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.JmpGte(ifLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.JmpLt(ifLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.JmpLte(ifLabel));
                ElseDo?.Visit(context);
                context.AddInstruction(X86Instructions.Jmp(endLabel));
                context.AddInstruction(X86Instructions.Label(ifLabel));
                ThenDo.Visit(context);
                context.AddInstruction(X86Instructions.Label(endLabel));
            }
            else if (Condition is TypedBinaryComparison_Float_Float comparison_Float_Float)
            {
                comparison_Float_Float.Lhs.Visit(context);
                comparison_Float_Float.Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
                context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 0, true)));
                context.AddInstruction(X86Instructions.Add(X86Register.esp, 8));
                context.AddInstruction(X86Instructions.FComip());
                context.AddInstruction(X86Instructions.Fstp(X87Register.st0));
   
                //  FCOMI instruction does not set sign or overflow flags, so jumps must be made a bit differently
                //     +--------------+---+---+-----+------------------------------------+
                //     | Test         | Z | C | Jcc | Notes                              |
                //     +--------------+---+---+-----+------------------------------------+
                //     | ST0 < ST(i)  | X | 1 | JB  | ZF will never be set when CF = 1   |
                //     | ST0 <= ST(i) | 1 | 1 | JBE | Either ZF or CF is ok              |
                //     | ST0 == ST(i) | 1 | X | JE  | CF will never be set in this case  |
                //     | ST0 != ST(i) | 0 | X | JNE |                                    |
                //     | ST0 >= ST(i) | X | 0 | JAE | As long as CF is clear we are good |
                //     | ST0 > ST(i)  | 0 | 0 | JA  | Both CF and ZF must be clear       |
                //     +--------------+---+---+-----+------------------------------------+
                //     Legend: X: don't care, 0: clear, 1: set
     

                if (comparison_Float_Float.ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.JmpEq(ifLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.JmpNeq(ifLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.Ja(ifLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.Jae(ifLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.Jb(ifLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.Jbe(ifLabel));
                ElseDo?.Visit(context);
                context.AddInstruction(X86Instructions.Jmp(endLabel));
                context.AddInstruction(X86Instructions.Label(ifLabel));
                ThenDo.Visit(context);
                context.AddInstruction(X86Instructions.Label(endLabel));
            } else throw new NotImplementedException();

        }

        public override void Visit(TypedFunctionDeclaration functionParent)
        {
            base.Visit(functionParent);
            ThenDo.Visit(functionParent);
            ElseDo?.Visit(functionParent);
        }
    }

    public class TypedWhileStatement : TypedStatement
    {
        public TypedExpression Condition { get; set; }
        public TypedStatement ThenDo { get; set; }
        public TypedWhileStatement(Statement originalStatement, TypedExpression condition, TypedStatement thenDo) : base(originalStatement)
        {
            Condition = condition;
            ThenDo = thenDo;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var startLabel = context.CreateLabel();
            var bodyLabel = context.CreateLabel();
            var endLabel = context.EnterLoop(startLabel);

            if (Condition is TypedBinaryComparison_Integer_Integer comparison_Integer_Integer)
            {
                context.AddInstruction(X86Instructions.Label(startLabel));
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Cmp(X86Register.ebx, X86Register.eax));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.Jz(bodyLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.Jnz(bodyLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.JmpGt(bodyLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.JmpGte(bodyLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.JmpLt(bodyLabel));
                if (comparison_Integer_Integer.ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.JmpLte(bodyLabel));

                context.AddInstruction(X86Instructions.Jmp(endLabel));
                context.AddInstruction(X86Instructions.Label(bodyLabel));
                ThenDo.Visit(context);
                context.AddInstruction(X86Instructions.Label(endLabel));
            }
            else if (Condition is TypedBinaryComparison_Float_Float comparison_Float_Float)
            {
                context.AddInstruction(X86Instructions.Label(startLabel));
                context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
                context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 0, true)));
                context.AddInstruction(X86Instructions.Add(X86Register.esp, 8));
                context.AddInstruction(X86Instructions.FComip());
                context.AddInstruction(X86Instructions.Fstp(X87Register.st0));

                //  FCOMI instruction does not set sign or overflow flags, so jumps must be made a bit differently
                //     +--------------+---+---+-----+------------------------------------+
                //     | Test         | Z | C | Jcc | Notes                              |
                //     +--------------+---+---+-----+------------------------------------+
                //     | ST0 < ST(i)  | X | 1 | JB  | ZF will never be set when CF = 1   |
                //     | ST0 <= ST(i) | 1 | 1 | JBE | Either ZF or CF is ok              |
                //     | ST0 == ST(i) | 1 | X | JE  | CF will never be set in this case  |
                //     | ST0 != ST(i) | 0 | X | JNE |                                    |
                //     | ST0 >= ST(i) | X | 0 | JAE | As long as CF is clear we are good |
                //     | ST0 > ST(i)  | 0 | 0 | JA  | Both CF and ZF must be clear       |
                //     +--------------+---+---+-----+------------------------------------+
                //     Legend: X: don't care, 0: clear, 1: set


                if (comparison_Float_Float.ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.JmpEq(bodyLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.JmpNeq(bodyLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.Ja(bodyLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.Jae(bodyLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.Jb(bodyLabel));
                if (comparison_Float_Float.ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.Jbe(bodyLabel));
                context.AddInstruction(X86Instructions.Jmp(endLabel));
                context.AddInstruction(X86Instructions.Label(bodyLabel));
                ThenDo.Visit(context);
                context.AddInstruction(X86Instructions.Label(endLabel));
            }
            else throw new NotImplementedException();

            context.ExitLoop();
        }

        public override void Visit(TypedFunctionDeclaration functionParent)
        {
            base.Visit(functionParent);
            ThenDo.Visit(functionParent);
        }
    }

    public class TypedBlock : TypedStatement
    {
        public List<TypedStatement> Statements { get; set; }
        public TypedBlock(Statement originalStatement, List<TypedStatement> statements) : base(originalStatement)
        {
            Statements = statements;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            foreach (var statement in Statements)
            {
                statement.Visit(context);
            }
        }

        public override void Visit(TypedFunctionDeclaration functionParent)
        {
            base.Visit(functionParent);
            foreach (var statement in Statements)
            {
                statement.Visit(functionParent);
            }
        }
    }

    public class TypedBreak : TypedStatement
    {
        public TypedBreak(Statement originalStatement) : base(originalStatement)
        {
        }

        public override void Visit(X86CompilationContext context)
        {
            var breakLabel = context.GetLoopBreakLabel();
            context.AddInstruction(X86Instructions.Jmp(breakLabel));
        }
    }

    public class TypedContinue : TypedStatement
    {
        public TypedContinue(Statement originalStatement) : base(originalStatement)
        {
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var continueLabel = context.GetLoopContinueLabel();
            context.AddInstruction(X86Instructions.Jmp(continueLabel));
        }
    }

    public class TypedReturnStatement : TypedStatement
    {
        public TypedExpression ValueToReturn { get; set; }
        public TypedReturnStatement(Statement originalStatement, TypedExpression valueToReturn) : base(originalStatement)
        {
            ValueToReturn = valueToReturn;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            ValueToReturn.Visit(context);

            if (ValueToReturn.ResolvedType.Is(SupportedType.Int))
            {
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            }

            if (ValueToReturn.ResolvedType.Is(SupportedType.Float))
            {
                context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 0, true)));
                context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            }

            context.AddInstruction(X86Instructions.Mov(X86Register.esp, X86Register.ebp));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebp));
            if (context.CurrentFunction.CallingConvention == CallingConvention.Cdecl) context.AddInstruction(X86Instructions.Ret());
            else if (context.CurrentFunction.CallingConvention == CallingConvention.StdCall)
            {
                var parameterCount = context.CurrentFunction.Parameters.Count;
                if (parameterCount == 0) context.AddInstruction(X86Instructions.Ret());
                else context.AddInstruction(X86Instructions.Ret(parameterCount * 4));
            }
            else throw new NotImplementedException();

        }
    }

    public class TypedEpressionStatement : TypedStatement
    {
        public TypedExpression Expression { get; set; }
        public TypedEpressionStatement(Statement originalStatement, ResolvedType resolvedType, IToken identifier, TypedExpression expression) : base(originalStatement)
        {
            Expression = expression;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);     
            Expression.Visit(context);
        }

    }


}
