using PlatinumC.Compiler;
using PlatinumC.Compiler.TargetX86;
using PlatinumC.Compiler.TargetX86.Instructions;
using PlatinumC.Interfaces;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public class TypedExpression: IVisitable<X86CompilationContext>
    {
        public Expression OriginalExpression { get; set; }
        public ResolvedType ResolvedType { get; set; }

        public TypedExpression(Expression originalExpression, ResolvedType resolvedType)
        {
            OriginalExpression = originalExpression;
            ResolvedType = resolvedType;
        }

        public virtual void Visit(X86CompilationContext context) { }
    }


    #region BinaryAddition
    public class TypedBinaryAddition_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryAddition_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | B |
            //          | A |
            // sp----->
            // Pops A and B from the stack and performs signed integer addition B + A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Add(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
        }
    }

    public class TypedBinaryAddition_Pointer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryAddition_Pointer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | B |
            //          | A |
            // sp----->
            // Pops A and B from the stack and performs signed integer addition B + A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            if (Rhs is TypedLiteralInteger integerLiteral)
            {
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Add(X86Register.ebx, integerLiteral.Value * Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
            } else
            {
                Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.IMul(X86Register.eax, Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Add(X86Register.ebx, X86Register.eax));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
            }
        }
    }


    public class TypedBinaryAddition_Float_Float : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryAddition_Float_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed floating point addition B + A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
            context.AddInstruction(X86Instructions.FAddp(Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
        }
    }

    #endregion

    #region BinarySubtraction

    public class TypedBinarySubtraction_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinarySubtraction_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | B |
            //          | A |
            // sp----->
            // Pops A and B from the stack and performs signed integer subtraction B - A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Sub(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
        }
    }

    public class TypedBinarySubtraction_Pointer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinarySubtraction_Pointer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | B |
            //          | A |
            // sp----->
            // Pops A and B from the stack and performs signed integer addition B + A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            if (Rhs is TypedLiteralInteger integerLiteral)
            {             
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Sub(X86Register.ebx, integerLiteral.Value * Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
            }
            else
            {
                Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.IMul(X86Register.eax, Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Sub(X86Register.ebx, X86Register.eax));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
            }
            
        }
    }

    public class TypedBinarySubtraction_Float_Float : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinarySubtraction_Float_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed floating point subtraction B - A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
            context.AddInstruction(X86Instructions.FSubp(Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
        }
    }

    #endregion

    #region BinaryMultiplication

    public class TypedBinaryMultiplication_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryMultiplication_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed integer multiplication B * A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.IMul(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
        }
    }

    public class TypedBinaryMultiplication_Float_Float : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryMultiplication_Float_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed floating point multiplication B * A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
            context.AddInstruction(X86Instructions.FMulp(Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
        }
    }

    #endregion

    #region BinaryDivision

    public class TypedBinaryDivision_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryDivision_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed integer division B / A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Mov(X86Register.eax, Offset.Create(X86Register.esp, 4, true)));
            context.AddInstruction(X86Instructions.Cdq());
            context.AddInstruction(X86Instructions.IDiv(Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Mov(Offset.Create(X86Register.esp, 0, true), X86Register.eax));
        }
    }

    public class TypedBinaryDivision_Float_Float : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryDivision_Float_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack and performs signed floating point division B / A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Fld(Offset.Create(X86Register.esp, 4, true)));
            context.AddInstruction(X86Instructions.FDivp(Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
        }
    }

    #endregion


    #region BinaryComparison
    public class TypedBinaryComparison_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public TypedBinaryComparison_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs, ComparisonType comparisonType)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
            ComparisonType = comparisonType;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | B |
            //          | A |
            // sp----->
            // Pops A and B from the stack and compares B to A 

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);

            var ifLabel = context.CreateLabel();
            var endLabel = context.CreateLabel();
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Cmp(X86Register.ebx, X86Register.eax));
            if (ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.Jz(ifLabel));
            if (ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.Jnz(ifLabel));
            if (ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.JmpGt(ifLabel));
            if (ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.JmpGte(ifLabel));
            if (ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.JmpLt(ifLabel));
            if (ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.JmpLte(ifLabel));
            // Comparison is false, push 0 to stack
            context.AddInstruction(X86Instructions.Push(0));
            context.AddInstruction(X86Instructions.Jmp(endLabel));
            context.AddInstruction(X86Instructions.Label(ifLabel));
            // Comparison is true, push 1 to stack
            context.AddInstruction(X86Instructions.Push(1));
            context.AddInstruction(X86Instructions.Label(endLabel));
        }
    }

    public class TypedBinaryComparison_Float_Float : TypedExpression
    {
        // https://stackoverflow.com/questions/7057501/x86-assembler-floating-point-compare
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public TypedBinaryComparison_Float_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs, ComparisonType comparisonType)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
            ComparisonType = comparisonType;
        }

        public override void Visit(X86CompilationContext context)
        {
            //          | B | esp + 4
            // sp-----> | A | esp + 0
            // 
            // Pops A and B from the stack, loads them into FPU register and compares B to A

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);

            var ifLabel = context.CreateLabel();
            var endLabel = context.CreateLabel();

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


            if (ComparisonType == ComparisonType.Equal) context.AddInstruction(X86Instructions.JmpEq(ifLabel));
            if (ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.JmpNeq(ifLabel));
            if (ComparisonType == ComparisonType.GreaterThan) context.AddInstruction(X86Instructions.Ja(ifLabel));
            if (ComparisonType == ComparisonType.GreaterThanEqual) context.AddInstruction(X86Instructions.Jae(ifLabel));
            if (ComparisonType == ComparisonType.LessThan) context.AddInstruction(X86Instructions.Jb(ifLabel));
            if (ComparisonType == ComparisonType.LessThanEqual) context.AddInstruction(X86Instructions.Jbe(ifLabel));
            // Comparison is false, push 0 to stack
            context.AddInstruction(X86Instructions.Push(0));
            context.AddInstruction(X86Instructions.Jmp(endLabel));
            context.AddInstruction(X86Instructions.Label(ifLabel));
            // Comparison is true, push 1 to stack
            context.AddInstruction(X86Instructions.Push(1));
            context.AddInstruction(X86Instructions.Label(endLabel));

        }
    }

    #endregion


    #region LogicalOperators

    public class TypedBinaryLogicalAnd_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryLogicalAnd_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  Evaluates Lhs, tests the value on top of the stack
            //      -If the result is not zero, pops the stack, then evaluates Rhs and leaves the rhs result on the stack (optimized instead of popping the result and pushing a 1 or 0)
            //      -If the result is zero, leaves zero on the stack and does not evaluate Rhs

            base.Visit(context);
            var endLabel = context.CreateLabel();
            Lhs.Visit(context);
            context.AddInstruction(X86Instructions.Test(X86Register.eax, Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Jz(endLabel));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Label(endLabel));
        }
    }

    public class TypedBinaryLogicalOr_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryLogicalOr_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  Evaluates Lhs, tests the value on top of the stack
            //      -If the result is zero, pops the stack, then evaluates Rhs and leaves the rhs result on the stack (optimized instead of popping the result and pushing a 1 or 0)
            //      -If the result is not zero, leaves value on the stack and does not evaluate Rhs

            base.Visit(context);
            var endLabel = context.CreateLabel();
            Lhs.Visit(context);
            context.AddInstruction(X86Instructions.Test(X86Register.eax, Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Jnz(endLabel));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Label(endLabel));
        }
    }

    #endregion


    public class TypedCall : TypedExpression
    {
        public TypedFunctionDeclaration Function { get; set; }
        public List<TypedExpression> Arguments { get; set; }
        public TypedCall(Expression originalExpression, ResolvedType resolvedType, TypedFunctionDeclaration function, List<TypedExpression> arguments) : base(originalExpression, resolvedType)
        {
            Function = function;
            Arguments = arguments;
        }

        public override void Visit(X86CompilationContext context)
        {
            for (int i = Arguments.Count - 1; i >= 0; i--)
            {
                Arguments[i].Visit(context);
            }
            context.AddInstruction(X86Instructions.Call(Function.FunctionIdentifier.Lexeme, false));
            if (Function.CallingConvention == CallingConvention.Cdecl) context.AddInstruction(X86Instructions.Add(X86Register.esp, Arguments.Count * context.SizeOfPtr));
            if (Function.CallingConvention != CallingConvention.StdCall) throw new InvalidOperationException($"Unsupported calling convention {Function.CallingConvention}");

            if (Function.ReturnType.Is(SupportedType.Float))
            {
                context.AddInstruction(X86Instructions.Sub(X86Register.esp, 4));
                context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
            }
            else if (!Function.ReturnType.Is(SupportedType.Void)) context.AddInstruction(X86Instructions.Push(X86Register.eax, false));
        }
    }

    public class TypedCallImportedFunction : TypedExpression
    {
        public TypedImportedFunctionDeclaration Function { get; set; }
        public List<TypedExpression> Arguments { get; set; }
        public TypedCallImportedFunction(Expression originalExpression, ResolvedType resolvedType, TypedImportedFunctionDeclaration function, List<TypedExpression> arguments) : base(originalExpression, resolvedType)
        {
            Function = function;
            Arguments = arguments;
        }

        public override void Visit(X86CompilationContext context)
        {
            for (int i = Arguments.Count - 1; i >= 0; i--)
            {
                Arguments[i].Visit(context);
            }
            context.AddInstruction(X86Instructions.Call(Function.FunctionIdentifier.Lexeme, true));
            if (Function.CallingConvention == CallingConvention.Cdecl) context.AddInstruction(X86Instructions.Add(X86Register.esp, Arguments.Count * context.SizeOfPtr));
            else if (Function.CallingConvention != CallingConvention.StdCall) throw new InvalidOperationException($"Unsupported calling convention {Function.CallingConvention}");

            if (Function.ReturnType.Is(SupportedType.Float))
            {
                context.AddInstruction(X86Instructions.Sub(X86Register.esp, 4));
                context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0, true)));
            }
            else if (!Function.ReturnType.Is(SupportedType.Void)) context.AddInstruction(X86Instructions.Push(X86Register.eax, false));
        }
    }


    public class TypedAssignment : TypedExpression
    {
        public IToken AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public TypedAssignment(Expression originalExpression, ResolvedType resolvedType, IToken assignmentTarget, TypedExpression valueToAssign) : base(originalExpression, resolvedType)
        {
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override void Visit(X86CompilationContext context)
        {
            var offset = context.GetIdentifierOffset(AssignmentTarget);
            ValueToAssign.Visit(context);
            // leave value on the stack instead of popping, then re-pushing
            context.AddInstruction(X86Instructions.Mov(X86Register.eax, Offset.Create(X86Register.esp, 0, true)));
            context.AddInstruction(X86Instructions.Mov(offset, X86Register.eax));
        }
    }

    public class TypedDereferenceAssignment : TypedExpression
    {
        public TypedDereference AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public TypedDereferenceAssignment(Expression originalExpression, ResolvedType resolvedType, TypedDereference assignmentTarget, TypedExpression valueToAssign) : base(originalExpression, resolvedType)
        {
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            ValueToAssign.Visit(context);
            AssignmentTarget.Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Mov(Offset.Create(X86Register.ebx, 0, true), X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.eax, false));

        }
    }

    public class TypedMemberAssignment : TypedExpression
    {
        public TypedExpression Instance { get; set; }
        public IToken AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public TypedMemberAssignment(Expression originalExpression, ResolvedType resolvedType, TypedExpression instance, IToken assignmentTarget, TypedExpression valueToAssign) : base(originalExpression, resolvedType)
        {
            Instance = instance;
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var offset = context.GetIdentifierOffset(AssignmentTarget);
            Instance.Visit(context);
            ValueToAssign.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Mov(Offset.Create(X86Register.eax, Instance.ResolvedType.GetMemberOffset(AssignmentTarget), true), X86Register.ebx));
        }
    }

    public class TypedIdentifier : TypedExpression
    {
        public IToken Identifier { get; set; }
        public TypedIdentifier(Expression originalExpression, ResolvedType resolvedType, IToken identifier) : base(originalExpression, resolvedType)
        {
            Identifier = identifier;
        }

        public override void Visit(X86CompilationContext context)
        {
            var offset = context.GetIdentifierOffset(Identifier);
            context.AddInstruction(X86Instructions.Push(offset));
        }
    }


    public class TypedReference : TypedExpression
    {
        public IToken Identifier { get; set; }
        public TypedReference(Expression originalExpression, ResolvedType resolvedType, IToken identifier) : base(originalExpression, resolvedType)
        {
            Identifier = identifier;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var offset = context.GetIdentifierOffset(Identifier);
            context.AddInstruction(X86Instructions.Lea(X86Register.eax, offset));
            context.AddInstruction(X86Instructions.Push(X86Register.eax, false));
        }
    }

    public class TypedDereference : TypedExpression
    {
        public TypedExpression Rhs { get; set; }
        public TypedDereference(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs) : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Mov(X86Register.ebx, Offset.Create(X86Register.eax, 0, true)));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx, false));
        }
    }

    public class TypedLiteralString : TypedExpression
    {
        public string Value { get; set; }
        public TypedLiteralString(Expression originalExpression, ResolvedType resolvedType, string value)
            : base(originalExpression, resolvedType)
        {
            Value = value;
        }

        public override void Visit(X86CompilationContext context)
        {
            var label = context.AddStringData(Value);
            context.AddInstruction(X86Instructions.Push(label, false));
        }
    }

    public class TypedLiteralInteger : TypedExpression
    {
        public int Value { get; set; }
        public TypedLiteralInteger(Expression originalExpression, ResolvedType resolvedType, int value)
            : base(originalExpression, resolvedType)
        {
            Value = value;
        }

        public override void Visit(X86CompilationContext context)
        {
            context.AddInstruction(X86Instructions.Push(Value));
        }
    }

    public class TypedLiteralFloatingPoint : TypedExpression
    {
        public float Value { get; set; }
        public TypedLiteralFloatingPoint(Expression originalExpression, ResolvedType resolvedType, float value)
            : base(originalExpression, resolvedType)
        {
            Value = value;
        }

        public override void Visit(X86CompilationContext context)
        {
            context.AddInstruction(X86Instructions.Push(Value));
        }
    }

    public class TypedGroup : TypedExpression
    {
        public TypedExpression Expression { get; set; }

        public TypedGroup(Group originalExpression, ResolvedType resolvedType, TypedExpression expression) : base(originalExpression, resolvedType)
        {
            Expression = expression;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Expression.Visit(context);
        }

    }

    public class TypedGetFromReference : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public IToken MemberTarget { get; set; }
        public TypedGetFromReference(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, IToken memberTarget) : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            MemberTarget = memberTarget;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Lhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Mov(X86Register.ebx, Offset.Create(X86Register.eax, Lhs.ResolvedType.GetMemberOffset(MemberTarget), true)));
        }
    }

}
