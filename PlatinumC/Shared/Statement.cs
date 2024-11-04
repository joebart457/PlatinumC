using PlatinumC.Interfaces;
using PlatinumC.Resolver;
using TokenizerCore.Interfaces;

namespace PlatinumC.Shared
{
    public abstract class Statement: IVisitable<TypeResolver, TypedStatement>
    {
        public IToken Token { get; set; }

        public Statement(IToken token)
        {
            Token = token;
        }

        public abstract TypedStatement Visit(TypeResolver resolver);
    }

    public class VariableDeclaration : Statement
    {
        public TypeSymbol TypeSymbol { get; set; }
        public IToken Identifier { get; set; }
        public Expression? Initializer { get; set; }
        public VariableDeclaration(IToken token, TypeSymbol typeSymbol, IToken identifier, Expression? initializer) : base(token)
        {
            TypeSymbol = typeSymbol;
            Identifier = identifier;
            Initializer = initializer;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        public Statement ThenDo { get; set; }
        public Statement? ElseDo { get; set; }
        public IfStatement(IToken token, Expression condition, Statement thenDo, Statement? elseDo) : base(token)
        {
            Condition = condition;
            ThenDo = thenDo;
            ElseDo = elseDo;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class WhileStatement : Statement
    {
        public Expression Condition { get; set; }
        public Statement ThenDo { get; set; }
        public WhileStatement(IToken token, Expression condition, Statement thenDo) : base(token)
        {
            Condition = condition;
            ThenDo = thenDo;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Block : Statement
    {
        public List<Statement> Statements { get; set; }
        public Block(IToken token, List<Statement> statements) : base(token)
        {
            Statements = statements;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Break : Statement
    {
        public Break(IToken token) : base(token)
        {
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class Continue : Statement
    {
        public Continue(IToken token) : base(token)
        {
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression? ValueToReturn { get; set; }
        public ReturnStatement(IToken token, Expression? valueToReturn) : base(token)
        {
            ValueToReturn = valueToReturn;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }
        public ExpressionStatement(IToken token, Expression expression) : base(token)
        {
            Expression = expression;
        }

        public override TypedStatement Visit(TypeResolver resolver)
        {
            return resolver.Accept(this);
        }
    }
}
