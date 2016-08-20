using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace uda.Intermediate
{
    internal class IfStatement : InstructionNodeBase, IInstructionNode
    {
        public struct ExpressionNodePair
        {
            public IExpression Expression { get; }
            public IInstructionNode Node { get; }

            public ExpressionNodePair(IExpression expression, IInstructionNode node)
            {
                Expression = expression;
                Node = node;
            }
        }

        public InstructionType Type => InstructionType.If;

        public ImmutableArray<ExpressionNodePair> ExpressionNodePairs { get; }
        public IInstructionNode ElseChild { get; }

        public IExpression FirstExpression => ExpressionNodePairs[0].Expression;

        public IfStatement(IExpression expression, IInstructionNode child) : base(child)
        {
            ExpressionNodePairs = ImmutableArray.Create(new ExpressionNodePair(expression, child));
        }

        public IfStatement(IExpression expression, IInstructionNode child, IInstructionNode elseChild)
            : base(new[] { child, elseChild })
        {
            ExpressionNodePairs = ImmutableArray.Create(new ExpressionNodePair(expression, child));
            ElseChild = elseChild;
        }

        public IfStatement(ImmutableArray<ExpressionNodePair> expressionNodePairs)
            : base(expressionNodePairs.Select(x => x.Node))
        {
            ExpressionNodePairs = expressionNodePairs;
        }

        public IfStatement(ImmutableArray<ExpressionNodePair> expressionNodePairs, IInstructionNode elseChild)
            : base(expressionNodePairs.Select(x => x.Node).Concat(new[] { elseChild }).ExceptNulls())
        {
            ExpressionNodePairs = expressionNodePairs;
            ElseChild = elseChild;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            Debug.Assert(Children.Count == children.Length);

            int numExpressionNodePairs = ExpressionNodePairs.Length;
            var newExpressionNodePairs = ImmutableArray.CreateBuilder<ExpressionNodePair>(numExpressionNodePairs);
            for (int i = 0; i < numExpressionNodePairs; i++)
            {
                newExpressionNodePairs.Add(new ExpressionNodePair(ExpressionNodePairs[i].Expression, children[i]));
            }

            IInstructionNode newElseChild = ElseChild == null ? null : children.Last();

            return new IfStatement(newExpressionNodePairs.ToImmutable(), newElseChild);
        }

        public override string ToString()
        {
            return String.Format("if ({0}) {1}", FirstExpression, FirstChild);
        }
    }
}
