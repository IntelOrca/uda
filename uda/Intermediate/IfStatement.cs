using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class IfStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly ImmutableArray<Tuple<IExpression, IInstructionNode>> _expressionNodePairs;
		private readonly IInstructionNode _elseChild;

		public InstructionType Type { get { return InstructionType.IfStatement; } }

		public ImmutableArray<Tuple<IExpression, IInstructionNode>> ExpressionNodePairs { get { return _expressionNodePairs; } }
		public IInstructionNode ElseChild { get { return _elseChild; } }

		public IExpression FirstExpression { get { return _expressionNodePairs[0].Item1; } }
		public IInstructionNode FirstChild { get { return _expressionNodePairs[0].Item2; } }

		public IfStatement(IExpression expression, IInstructionNode child)
		{
			_expressionNodePairs = ImmutableArray.Create(new Tuple<IExpression, IInstructionNode>(expression, child));
		}

		public IfStatement(IExpression expression, IInstructionNode child, IInstructionNode elseChild)
		{
			_expressionNodePairs = ImmutableArray.Create(new Tuple<IExpression, IInstructionNode>(expression, child));
			_elseChild = elseChild;
		}

		public override string ToString()
		{
			return String.Format("if ({0}) {1}", FirstExpression, FirstChild);
		}
	}
}
