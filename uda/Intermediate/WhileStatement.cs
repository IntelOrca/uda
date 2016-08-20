using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class WhileStatement : InstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.While;
        public IExpression Expression { get; }
        public IInstructionNode Child { get; }

        public WhileStatement(IExpression expression, IInstructionNode child) : base(child)
        {
            Expression = expression;
            Child = child;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return new WhileStatement(Expression, children[0]);
        }

        public override string ToString()
        {
            return String.Format("while ({0}) {1}", Expression, Child);
        }
    }
}
