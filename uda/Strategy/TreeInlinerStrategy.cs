using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using uda.Intermediate;

namespace uda.Strategy
{
	/// <summary>
	/// Find stand-alone trees that are only referenced once and inline them at their only usage.
	/// </summary>
	internal class TreeInlinerStrategy : IDecompileStrategy
	{
		Dictionary<long, int> _treeReferences = new Dictionary<long, int>();

		public void Process(Function function)
		{
			InstructionTreeTable treeTable = function.InstructionTreeTable;

			foreach (InstructionTree tree in treeTable)
				FindTreeReferences(tree);

			HashSet<long> singleTreeReferences = _treeReferences
				.Where(x => x.Value == 1)
				.Select(x => x.Key)
				.ToHashSet();

			foreach (InstructionTree tree in treeTable.ToArray()) {
				if (singleTreeReferences.Contains(tree.Address))
					continue;

				bool remakeTree = false;
				var newInstructions = ImmutableArray.CreateBuilder<IInstructionNode>(tree.Children.Count);
				foreach (IInstructionNode instruction in tree.Children) {
					IInstructionNode newInstruction = instruction;

					InstructionTreeReference treeReference;
					switch (instruction.Type) {
					case InstructionType.Goto:
						GotoStatement gotoStatement = (GotoStatement)instruction;
						treeReference = gotoStatement.Child as InstructionTreeReference;
						if (treeReference != null && singleTreeReferences.Contains(treeReference.Address)) {
							foreach (IInstructionNode subTreeInstruction in treeTable[treeReference.Address].Children)
								newInstructions.Add(subTreeInstruction);
							newInstruction = null;
							remakeTree = true;
						}
						break;
					case InstructionType.IfStatement:
						IfStatement ifStatement = ((IfStatement)instruction);
						treeReference = ifStatement.FirstChild as InstructionTreeReference;
						if (treeReference != null && singleTreeReferences.Contains(treeReference.Address)) {
							newInstruction = new IfStatement(ifStatement.FirstExpression, new InstructionNode(treeTable[treeReference.Address].Children));
							remakeTree = true;
						}
						break;
					case InstructionType.While:
						break;
					}

					if (newInstruction != null)
						newInstructions.Add(newInstruction);
				}

				if (remakeTree)
					treeTable.Add(new InstructionTree(tree.Address, newInstructions.ToImmutable()));
			}

			foreach (long address in singleTreeReferences)
				treeTable.Remove(address);
		}

		private void FindTreeReferences(IInstructionNode node)
		{
			InstructionTreeReference treeReference = node as InstructionTreeReference;
			if (treeReference != null)
				AddTreeReference(treeReference.Address);

			foreach (IInstructionNode child in node.Children)
				FindTreeReferences(child);
		}

		private void AddTreeReference(long address)
		{
			int count;
			_treeReferences.TryGetValue(address, out count);
			_treeReferences[address] = count + 1;
		}
	}
}
