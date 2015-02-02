using System.Linq;
using uda.Intermediate;

namespace uda.Strategy
{
	/// <summary>
	/// Find trees that jump to themselves and turn them into while true loops.
	/// </summary>
	internal class LoopFinderStrategy : IDecompileStrategy
	{
		public void Process(Function function)
		{
			InstructionTreeTable treeTable = function.InstructionTreeTable;

			foreach (InstructionTree tree in treeTable.ToArray()) {
				InstructionTree newTree = null;
				IInstructionNode lastInstruction = tree.LastChild;
				if (lastInstruction.Type == InstructionType.Goto) {
					InstructionTreeReference treeReference = ((GotoStatement)lastInstruction).Child;
					if (treeReference.Address == tree.Address) {
						// Tree is a self loop
						if (tree.Children.Count == 1) {
							newTree = new InstructionTree(tree.Address, new[] {
								new WhileStatement(new LiteralExpression(1, 1), InstructionNode.Empty)
							});
						} else {
							newTree = new InstructionTree(tree.Address, new[] {
								new WhileStatement(
									new LiteralExpression(1, 1),
									new InstructionNode(tree.Children.Take(tree.Children.Count - 1))
								)
							});
						}
					}
				}

				if (newTree != null)
					treeTable.Add(newTree);
			}
		}
	}
}
