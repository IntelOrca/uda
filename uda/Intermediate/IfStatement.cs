using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace uda.Intermediate
{
	internal class IfStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly ImmutableArray<ExpressionNodePair> _expressionNodePairs;
		private readonly IInstructionNode _elseChild;

		public struct ExpressionNodePair
		{
			private readonly IExpression _expression;
			private readonly IInstructionNode _node;

			public IExpression Expression { get { return _expression; } }
			public IInstructionNode Node { get { return _node; } }

			public ExpressionNodePair(IExpression expression, IInstructionNode node)
			{
				_expression = expression;
				_node = node;
			}
		}

		public InstructionType Type { get { return InstructionType.If; } }

		public IReadOnlyList<ExpressionNodePair> ExpressionNodePairs { get { return _expressionNodePairs; } }
		public IInstructionNode ElseChild { get { return _elseChild; } }

		public IExpression FirstExpression { get { return _expressionNodePairs[0].Expression; } }

		public IfStatement(IExpression expression, IInstructionNode child) : base(child)
		{
			_expressionNodePairs = ImmutableArray.Create(new ExpressionNodePair(expression, child));
		}

		public IfStatement(IExpression expression, IInstructionNode child, IInstructionNode elseChild)
			: base(new[] { child, elseChild })
		{
			_expressionNodePairs = ImmutableArray.Create(new ExpressionNodePair(expression, child));
			_elseChild = elseChild;
		}

		public IfStatement(ImmutableArray<ExpressionNodePair> expressionNodePairs)
			: base(expressionNodePairs.Select(x => x.Node))
		{
			_expressionNodePairs = expressionNodePairs;
		}

		public IfStatement(ImmutableArray<ExpressionNodePair> expressionNodePairs, IInstructionNode elseChild)
			: base(expressionNodePairs.Select(x => x.Node).Concat(new[] { elseChild }).ExceptNulls())
		{
			_expressionNodePairs = expressionNodePairs;
			_elseChild = elseChild;
		}

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			Debug.Assert(Children.Count == children.Length);

			int numExpressionNodePairs = _expressionNodePairs.Length;
			var newExpressionNodePairs = ImmutableArray.CreateBuilder<ExpressionNodePair>(numExpressionNodePairs);
			for (int i = 0; i < numExpressionNodePairs; i++)
				newExpressionNodePairs.Add(new ExpressionNodePair(_expressionNodePairs[i].Expression, children[i]));

			IInstructionNode newElseChild = _elseChild == null ? null : children.Last();

			return new IfStatement(newExpressionNodePairs.ToImmutable(), newElseChild);
		}

		public override string ToString()
		{
			return String.Format("if ({0}) {1}", FirstExpression, FirstChild);
		}
	}
}
