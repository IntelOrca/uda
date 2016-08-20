using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class WhileStatement : InstructionNodeBase, IInstructionNode
    {
        private readonly IExpression _expression;
        private readonly IInstructionNode _child;

        public InstructionType Type { get { return InstructionType.While; } }
        public IExpression Expression { get { return _expression; } }
        public IInstructionNode Child { get { return _child; } }

        public WhileStatement(IExpression expression, IInstructionNode child) : base(child)
        {
            _expression = expression;
            _child = child;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return new WhileStatement(_expression, children[0]);
        }

        public override string ToString()
        {
            return String.Format("while ({0}) {1}", _expression, _child);
        }
    }
}
