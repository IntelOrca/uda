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

			InstructionTree[] singleTrees = treeTable
				.Where(x => _treeReferences.ContainsKey(x.Address))
				.ToArray();

			foreach (InstructionTree tree in treeTable.ToArray()) {
				if (singleTreeReferences.Contains(tree.Address))
					continue;

				IInstructionNode newTree = tree;
				foreach (InstructionTree treeToInline in singleTrees)
					newTree = InlineTree(newTree, treeToInline);

				treeTable.Add((InstructionTree)newTree);
			}

			foreach (long address in singleTreeReferences)
				treeTable.Remove(address);
		}

		private IInstructionNode InlineTree(IInstructionNode node, InstructionTree treeToInline)
		{
			InstructionTreeReference treeReference = node as InstructionTreeReference;
			if (treeReference != null && treeReference.Address == treeToInline.Address)
				return new InstructionNode(treeToInline.Children);

			foreach (IInstructionNode child in node.Children)
				node = node.ReplaceChild(child, InlineTree(child, treeToInline));

			return node;
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
