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
        public virtual IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            throw new InvalidOperationException();
        }
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
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
                context.AddInstruction(X86Instructions.Push(X86Register.ebx));
            } else
            {
                Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.IMul(X86Register.eax, Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Add(X86Register.ebx, X86Register.eax));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Addss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Movss(Offset.Create(X86Register.esp, 0), XmmRegister.xmm0));
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
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
                context.AddInstruction(X86Instructions.Push(X86Register.ebx));
            }
            else
            {
                Rhs.Visit(context);
                context.AddInstruction(X86Instructions.Pop(X86Register.eax));
                context.AddInstruction(X86Instructions.IMul(X86Register.eax, Lhs.ResolvedType.ReferencedTypeSize));
                context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
                context.AddInstruction(X86Instructions.Sub(X86Register.ebx, X86Register.eax));
                context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Subss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Movss(Offset.Create(X86Register.esp, 0), XmmRegister.xmm0));
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
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Mulss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Movss(Offset.Create(X86Register.esp, 0), XmmRegister.xmm0));
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
            context.AddInstruction(X86Instructions.Mov(X86Register.eax, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Cdq());
            context.AddInstruction(X86Instructions.IDiv(Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Mov(Offset.Create(X86Register.esp, 0), X86Register.eax));
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
            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Divss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            context.AddInstruction(X86Instructions.Movss(Offset.Create(X86Register.esp, 0), XmmRegister.xmm0));
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

    public class TypedBinaryComparison_Byte_Byte : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public TypedBinaryComparison_Byte_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs, ComparisonType comparisonType)
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
            context.AddInstruction(X86Instructions.Cmp(X86ByteRegister.bl, X86ByteRegister.al));
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
            // Pops A and B from the stack, loads them into xmm0 and xmm1 register and compares B to A

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);

            var ifLabel = context.CreateLabel();
            var endLabel = context.CreateLabel();

            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 4)));
            context.AddInstruction(X86Instructions.Movss(XmmRegister.xmm1, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 8));

            if (ComparisonType == ComparisonType.Equal || ComparisonType == ComparisonType.NotEqual) context.AddInstruction(X86Instructions.Ucomiss(XmmRegister.xmm0, XmmRegister.xmm1));
            else context.AddInstruction(X86Instructions.Comiss(XmmRegister.xmm0, XmmRegister.xmm1));


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
            context.AddInstruction(X86Instructions.Test(X86Register.eax, Offset.Create(X86Register.esp, 0)));
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
            context.AddInstruction(X86Instructions.Test(X86Register.eax, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Jnz(endLabel));
            context.AddInstruction(X86Instructions.Add(X86Register.esp, 4));
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Label(endLabel));
        }
    }

    #endregion


    #region BitwiseOperators

    public class TypedBinaryAnd_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryAnd_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise and B & A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.And(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }
    }

    public class TypedBinaryOr_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryOr_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise or B | A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Or(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }
    }

    public class TypedBinaryXor_Integer_Integer : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryXor_Integer_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise xor B ^ A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Xor(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }
    }

    public class TypedBinaryAnd_Byte_Byte : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryAnd_Byte_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise and B & A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.And(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }
    }

    public class TypedBinaryOr_Byte_Byte : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryOr_Byte_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise or B | A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Or(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }
    }

    public class TypedBinaryXor_Byte_Byte : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public TypedBinaryXor_Byte_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, TypedExpression rhs)
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
            // Pops A and B from the stack and performs bitwise xor B ^ A, pushes result to stack

            base.Visit(context);
            Lhs.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            context.AddInstruction(X86Instructions.Pop(X86Register.ebx));
            context.AddInstruction(X86Instructions.Xor(X86Register.ebx, X86Register.eax));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
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
                context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0)));
            }
            else if (!Function.ReturnType.Is(SupportedType.Void)) context.AddInstruction(X86Instructions.Push(X86Register.eax));
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
                context.AddInstruction(X86Instructions.Fstp(Offset.Create(X86Register.esp, 0)));
            }
            else if (!Function.ReturnType.Is(SupportedType.Void)) context.AddInstruction(X86Instructions.Push(X86Register.eax));
        }
    }


    public class TypedAssignment : TypedExpression
    {
        public TypedExpression AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public TypedAssignment(Expression originalExpression, ResolvedType resolvedType, TypedExpression assignmentTarget, TypedExpression valueToAssign) : base(originalExpression, resolvedType)
        {
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);

            ValueToAssign.Visit(context);
            var offset = AssignmentTarget.GetEvaluatedOffset(context);
            context.AddInstruction(X86Instructions.Mov(X86Register.ebx, Offset.Create(X86Register.esp, 0)));
            if (ResolvedType.Is(SupportedType.Byte))
            {
                if (offset is RegisterOffset_Byte registerOffset_Byte) context.AddInstruction(X86Instructions.Mov(registerOffset_Byte, X86ByteRegister.bl));
                else if (offset is SymbolOffset_Byte symbolOffset_Byte) context.AddInstruction(X86Instructions.Mov(symbolOffset_Byte, X86ByteRegister.bl));
                else throw new InvalidOperationException();
            }
            else
            {
                if (offset is RegisterOffset registerOffset) context.AddInstruction(X86Instructions.Mov(registerOffset, X86Register.ebx));
                else if (offset is SymbolOffset symbolOffset) context.AddInstruction(X86Instructions.Mov(symbolOffset, X86Register.ebx));
                else throw new InvalidOperationException();
            }
            // ebx left on stack so no need to push
                
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
            // Even bytes are stored as at least dword locally
            context.AddInstruction(X86Instructions.Push(offset));
        }

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            return context.GetIdentifierOffset(Identifier);
        }
    }

    public class TypedGlobalIdentifier : TypedExpression
    {
        public IToken Identifier { get; set; }
        public TypedGlobalIdentifier(Expression originalExpression, ResolvedType resolvedType, IToken identifier) : base(originalExpression, resolvedType)
        {
            Identifier = identifier;
        }

        public override void Visit(X86CompilationContext context)
        {
            var offset = context.GetGlobalOffset(Identifier);
            // Bytes are NOT stored as dwords globally, so they need special handling
            if (ResolvedType.Is(SupportedType.Byte))
            {
                context.AddInstruction(X86Instructions.Movsx(X86Register.eax, Offset.CreateSymbolByteOffset(offset.Symbol, offset.Offset)));
                context.AddInstruction(X86Instructions.Push(X86Register.eax));
            }
            else context.AddInstruction(X86Instructions.Push(offset));
        }

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            var offset = context.GetGlobalOffset(Identifier);
            if (ResolvedType.Is(SupportedType.Byte)) return Offset.CreateSymbolByteOffset(offset.Symbol, offset.Offset);
            return offset;
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
            context.AddInstruction(X86Instructions.Push(X86Register.eax));
        }
    }

    public class TypedGlobalReference : TypedExpression
    {
        public IToken Identifier { get; set; }
        public TypedGlobalReference(Expression originalExpression, ResolvedType resolvedType, IToken identifier) : base(originalExpression, resolvedType)
        {
            Identifier = identifier;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            var offset = context.GetGlobalOffset(Identifier);
            context.AddInstruction(X86Instructions.Lea(X86Register.eax, offset));
            context.AddInstruction(X86Instructions.Push(X86Register.eax));
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
            if (ResolvedType.Is(SupportedType.Byte)) context.AddInstruction(X86Instructions.Movsx(X86Register.ebx, Offset.CreateByteOffset(X86Register.eax, 0)));
            else context.AddInstruction(X86Instructions.Mov(X86Register.ebx, Offset.Create(X86Register.eax, 0)));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            if (ResolvedType.Is(SupportedType.Byte)) return Offset.CreateByteOffset(X86Register.eax, 0);
            return Offset.Create(X86Register.eax, 0);
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

    public class TypedLiteralNullPointer : TypedExpression
    {
        public TypedLiteralNullPointer(Expression originalExpression, ResolvedType resolvedType)
            : base(originalExpression, resolvedType)
        {
        }

        public override void Visit(X86CompilationContext context)
        {
            context.AddInstruction(X86Instructions.Push(0));
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
            var label = context.AddSinglePrecisionFloatingPointData(Value);
            context.AddInstruction(X86Instructions.Push(label, true));
        }
    }

    public class TypedLiteralByte : TypedExpression
    {
        public byte Value { get; set; }
        public TypedLiteralByte(Expression originalExpression, ResolvedType resolvedType, byte value)
            : base(originalExpression, resolvedType)
        {
            Value = value;
        }

        public override void Visit(X86CompilationContext context)
        {
            // because twos compliment representation this is effectively the same as 
            // mov al
            // push eax
            // except this is more efficient in that it prevents a register stall
            // (moving data into part of a register, then later accessing the entire register)
            // and uses less instructions
            context.AddInstruction(X86Instructions.Push((int)Value));
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

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            return Expression.GetEvaluatedOffset(context);
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
            
            if (ResolvedType.Is(SupportedType.Byte)) context.AddInstruction(X86Instructions.Movsx(X86Register.ebx, Offset.CreateByteOffset(X86Register.eax, Lhs.ResolvedType.UnderlyingType!.GetMemberOffset(MemberTarget))));
            else context.AddInstruction(X86Instructions.Mov(X86Register.ebx, Offset.Create(X86Register.eax, Lhs.ResolvedType.UnderlyingType!.GetMemberOffset(MemberTarget))));
            context.AddInstruction(X86Instructions.Push(X86Register.ebx));
        }

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            Lhs.Visit(context);
            context.AddInstruction(X86Instructions.Pop(X86Register.eax));
            if (ResolvedType.Is(SupportedType.Byte)) return Offset.CreateByteOffset(X86Register.eax, Lhs.ResolvedType.UnderlyingType!.GetMemberOffset(MemberTarget));
            else return Offset.Create(X86Register.eax, Lhs.ResolvedType.UnderlyingType!.GetMemberOffset(MemberTarget));
        }
    }

    public class TypedGetFromLocalStruct : TypedExpression
    {
        public TypedExpression Lhs { get; set; }
        public IToken MemberTarget { get; set; }
        public TypedGetFromLocalStruct(Expression originalExpression, ResolvedType resolvedType, TypedExpression lhs, IToken memberTarget) : base(originalExpression, resolvedType)
        {
            Lhs = lhs;
            MemberTarget = memberTarget;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            IOffset targetOffset = GetEvaluatedOffset(context);

            if (ResolvedType.Is(SupportedType.Byte))
            {
                if (targetOffset is RegisterOffset_Byte byteOffset)
                {
                    context.AddInstruction(X86Instructions.Movsx(X86Register.ebx, byteOffset));
                    context.AddInstruction(X86Instructions.Push(X86Register.ebx));
                }
                else if (targetOffset is SymbolOffset_Byte symbolOffset_Byte)
                {
                    context.AddInstruction(X86Instructions.Movsx(X86Register.ebx, symbolOffset_Byte));
                    context.AddInstruction(X86Instructions.Push(X86Register.ebx));
                }
                else throw new InvalidOperationException();
            }
            else
            {
                if (targetOffset is RegisterOffset registerOffset) context.AddInstruction(X86Instructions.Push(registerOffset));
                else if (targetOffset is SymbolOffset symbolOffset) context.AddInstruction(X86Instructions.Push(symbolOffset));
                else throw new InvalidOperationException();
            }
        }

        public override IOffset GetEvaluatedOffset(X86CompilationContext context)
        {
            IOffset targetOffset = Lhs.GetEvaluatedOffset(context);
            targetOffset.Offset = targetOffset.Offset + Lhs.ResolvedType.GetMemberOffset(MemberTarget);
            return targetOffset;
        }
    }

    #region TypeCasts

    public class TypedCast_Integer_From_Byte : TypedExpression
    {
        // Cast a byte to an integer
        public TypedExpression Rhs { get; set; }
        public TypedCast_Integer_From_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs) : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Rhs.Visit(context);
            // Due to how we handle bytes and byte operations,
            // we can safely assume no conversion is needed
        }
    }

    public class TypedCast_Float_From_Integer : TypedExpression
    {
        // Cast an integer to a single precision floating point
        public TypedExpression Rhs { get; set; }
        public TypedCast_Float_From_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs) : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Cvtsi2ss(XmmRegister.xmm0, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Movss(Offset.Create(X86Register.esp, 0), XmmRegister.xmm0));
        }
    }

    public class TypedCast_Integer_From_Float : TypedExpression
    {
        // Cast a single precision floating point to an integer
        public TypedExpression Rhs { get; set; }
        public TypedCast_Integer_From_Float(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs) : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Cvtss2si(X86Register.eax, Offset.Create(X86Register.esp, 0)));
            context.AddInstruction(X86Instructions.Mov(Offset.Create(X86Register.esp, 0), X86Register.eax));
        }
    }

    public class TypedCast_Pointer_From_Pointer : TypedExpression
    {
        // Cast a pointer of one type to a pointer of another type
        public TypedExpression Rhs { get; set; }
        public TypedCast_Pointer_From_Pointer(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs) : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            base.Visit(context);
            Rhs.Visit(context);
        }
    }

    #endregion


    #region UnaryOperators

    public class TypedUnary_Negation_Integer: TypedExpression
    {
        public TypedExpression Rhs { get; set; }
        public TypedUnary_Negation_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | A |
            // sp----->
            // subtracts A from 0 and modifies it in place on the stack

            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Neg(Offset.Create(X86Register.esp, 0)));
        }
    }

    public class TypedUnary_Negation_Byte : TypedExpression
    {
        public TypedExpression Rhs { get; set; }
        public TypedUnary_Negation_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | A |
            // sp----->
            // subtracts A from 0 and modifies it in place on the stack

            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Neg(Offset.Create(X86Register.esp, 0)));
        }
    }


    public class TypedUnary_Not_Integer : TypedExpression
    {
        public TypedExpression Rhs { get; set; }
        public TypedUnary_Not_Integer(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | A |
            // sp----->
            // performs bitwise negation on A, modifies it in place on the stack

            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Not(Offset.Create(X86Register.esp, 0)));
        }
    }

    public class TypedUnary_Not_Byte : TypedExpression
    {
        public TypedExpression Rhs { get; set; }
        public TypedUnary_Not_Byte(Expression originalExpression, ResolvedType resolvedType, TypedExpression rhs)
            : base(originalExpression, resolvedType)
        {
            Rhs = rhs;
        }

        public override void Visit(X86CompilationContext context)
        {
            //  result: | A |
            // sp----->
            // performs bitwise negation on A, modifies it in place on the stack

            base.Visit(context);
            Rhs.Visit(context);
            context.AddInstruction(X86Instructions.Not(Offset.Create(X86Register.esp, 0)));
        }
    }


    #endregion
}
