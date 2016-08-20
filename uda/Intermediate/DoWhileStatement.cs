using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class DoWhileStatement : InstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.Do;
        public IExpression Expression { get; }
        public IInstructionNode Child { get; }

        public DoWhileStatement(IExpression expression, IInstructionNode child)
            : base(child)
        {
            Expression = expression;
            Child = child;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return new DoWhileStatement(Expression, children[0]);
        }

        public override string ToString()
        {
            return String.Format("do {0} while ({1})", Child, Expression);
        }
    }
}
