using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace uda.Intermediate
{
	internal class InstructionNode : InstructionNodeBase, IInstructionNode
	{
		public readonly static InstructionNode Empty = new InstructionNode(Enumerable.Empty<IInstructionNode>());

		public InstructionType Type { get { return InstructionType.Block; } }

		public InstructionNode(IEnumerable<IInstructionNode> children) : base(children) { }
		public InstructionNode(ImmutableArray<IInstructionNode> children) : base(children) { }

		public InstructionNode ReplaceInstruction(int index, IInstructionNode newInstruction)
		{
			var builder = ImmutableArray.CreateBuilder<IInstructionNode>(Children.Count);
			for (int i = 0; i < Children.Count; i++)
				builder.Add(i == index ? newInstruction : Children[i]);

			return new InstructionNode(builder.ToImmutable());
		}

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return new InstructionNode(children);
		}

		public override string ToString()
		{
			return String.Format("Node ({0})", Children.Count);
		}

		public static IInstructionNode Clean(IInstructionNode node)
		{
			// Avoid creating a new array of child nodes unless there is going to be a change
			ImmutableArray<IInstructionNode>.Builder newChildren = null;

			if (node.Type == InstructionType.Block && node.Children.Any(x => x.Type == InstructionType.Block)) {
				newChildren = ImmutableArray.CreateBuilder<IInstructionNode>();
				foreach (IInstructionNode child in node) {
					IInstructionNode cleanChild = Clean(child);
					if (cleanChild.Type == InstructionType.Block)
						newChildren.AddRange(cleanChild.Children);
					else
						newChildren.Add(child);
				}
				return node.CreateFromChildren(newChildren.ToImmutable());
			} else {
				int index = 0;
				foreach (IInstructionNode child in node) {
					IInstructionNode cleanChild = Clean(child);
					if (cleanChild != child) {
						if (newChildren == null) {
							newChildren = ImmutableArray.CreateBuilder<IInstructionNode>(node.Children.Count);
							newChildren.AddRange(node.Children);
						}
						newChildren[index] = cleanChild;
					}
					index++;
				}

				return newChildren == null ? node : node.CreateFromChildren(newChildren.ToImmutable());
			}
		}

		// TODO Think of a better name for this method.
		public static bool IsNodeDeadEnd(IInstructionNode node)
		{
			switch (node.Type) {
			case InstructionType.Goto:
				return true;
			case InstructionType.Return:
				return true;
			case InstructionType.While:
				WhileStatement whileStatement = (WhileStatement)node;
				return Expression.IsTautology(whileStatement.Expression);
			}

			foreach (IInstructionNode child in node)
				if (IsNodeDeadEnd(child))
					return true;

			return false;
		}
	}
}
