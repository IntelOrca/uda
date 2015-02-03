using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class DoWhileStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly IExpression _expression;
		private readonly IInstructionNode _child;

		public InstructionType Type { get { return InstructionType.Do; } }
		public IExpression Expression { get { return _expression; } }
		public IInstructionNode Child { get { return _child; } }

		public DoWhileStatement(IExpression expression, IInstructionNode child) : base(child)
		{
			_expression = expression;
			_child = child;
		}

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return new DoWhileStatement(_expression, children[0]);
		}

		public override string ToString()
		{
			return String.Format("do {0} while ({1})", _child, _expression);
		}
	}
}
