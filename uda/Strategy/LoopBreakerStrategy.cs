using System.Collections.Generic;
using System.Linq;
using uda.Intermediate;

namespace uda.Strategy
{
	/// <summary>
	/// Find expressionless loops that end with a break which could be moved out.
	/// </summary>
	internal class LoopBreakerStrategy : IDecompileStrategy
	{
		public void Process(Function function)
		{
			InstructionTreeTable treeTable = function.InstructionTreeTable;

			foreach (InstructionTree tree in treeTable.ToArray()) {
				InstructionTree newTree = (InstructionTree)Process(tree);
				if (tree != newTree)
					treeTable.Add(newTree);
			}
		}

		private IInstructionNode Process(IInstructionNode node)
		{
			if (node.Type != InstructionType.While) {
				for (int i = 0; i < node.Children.Count; i++)
					node = node.ReplaceChild(i, Process(node.Children[i]));
				return node;
			}

			WhileStatement whileStatement = (WhileStatement)node;
			if (!Expression.IsTautology(whileStatement.Expression))
				return node;

			IInstructionNode loopChild = whileStatement.Child;
			if (loopChild.LastChild == null)
				return node;

			if (loopChild.LastChild.Type != InstructionType.If)
				return node;

			IfStatement ifStatement = (IfStatement)loopChild.LastChild;
			if (ifStatement.ExpressionNodePairs.Count > 1 || ifStatement.ElseChild != null)
				return node;

			// TODO check for instructions in the if statement that might prevent this from working, e.g. continue

			IInstructionNode newLoopChild = new InstructionNode(loopChild.Take(loopChild.Children.Count - 1));
			return new InstructionNode(new IInstructionNode[] {
				new DoWhileStatement(new BooleanNotExpression(ifStatement.FirstExpression), newLoopChild),
				new InstructionNode(ifStatement.Children)
			});
		}
	}
}
