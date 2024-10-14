using PlatinumC.Interfaces;
using PlatinumC.Resolver;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public abstract class Expression: IVisitable<TypeResolver, TypedExpression>
    {
        public IToken Token { get; set; }

        public Expression(IToken token)
        {
            Token = token;
        }

        public abstract TypedExpression Visit(TypeResolver resolver);
    }

    public class Identifier : Expression
    {
        public Identifier(IToken token) : base(token)
        {
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Reference : Expression
    {
        public Reference(IToken token) : base(token)
        {
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Dereference : Expression
    {
        public Expression Rhs { get; set; }
        public Dereference(IToken token, Expression rhs) : base(token)
        {
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class DereferenceAssignment : Expression
    {
        public Expression AssignmentTarget { get; set; }
        public Expression ValueToAssign { get; set; }
        public DereferenceAssignment(IToken token, Expression assignmentTarget, Expression valueToAssign) : base(token)
        {
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Call : Expression
    {
        public IToken FunctionIdentifier { get; set; }
        public List<Expression> Arguments { get; set; }
        public Call(IToken token, IToken functionIdentifier, List<Expression> arguments) : base(token)
        {
            FunctionIdentifier = functionIdentifier;
            Arguments = arguments;
        }
        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Assignment : Expression
    {
        public Expression? Instance { get; set; }
        public IToken AssignmentTarget { get; set; }
        public Expression ValueToAssign { get; set; }
        public Assignment(IToken token, Expression? instance, IToken assignmentTarget, Expression valueToAssign)
            : base(token)
        {
            Instance = instance;
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinaryAddition: Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinaryAddition(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinarySubtraction : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinarySubtraction(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinaryMultiplication : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinaryMultiplication(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinaryDivision : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinaryDivision(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        GreaterThanEqual,
        LessThanEqual,
        Equal,
        NotEqual,
    }

    public class BinaryComparison : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public ComparisonType ComparisonType { get; set; }
        public BinaryComparison(IToken token, Expression lhs, Expression rhs, ComparisonType comparisonType)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
            ComparisonType = comparisonType;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinaryLogicalAnd : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinaryLogicalAnd(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class BinaryLogicalOr : Expression
    {
        public Expression Lhs { get; set; }
        public Expression Rhs { get; set; }
        public BinaryLogicalOr(IToken token, Expression lhs, Expression rhs)
            : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }


    public class LiteralString : Expression
    {
        public string Value { get; set; }
        public LiteralString(IToken token, string value) : base(token)
        {
            Value = value;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class LiteralInteger : Expression
    {
        public int Value { get; set; }
        public LiteralInteger(IToken token, int value) : base(token)
        {
            Value = value;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class LiteralFloatingPoint : Expression
    {
        public float Value { get; set; }
        public LiteralFloatingPoint(IToken token, float value) : base(token)
        {
            Value = value;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Group : Expression
    {
        public Expression Expression { get; set; }

        public Group(IToken token, Expression expression) : base(token)
        {
            Expression = expression;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }

    }

    public class GetFromReference : Expression
    {
        public Expression Instance { get; set; }
        public IToken MemberTarget { get; set; }
        public GetFromReference(IToken token, Expression instance, IToken memberTarget) : base(token)
        {
            Instance = instance;
            MemberTarget = memberTarget;
        }

        public override TypedExpression Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }

    }


}
