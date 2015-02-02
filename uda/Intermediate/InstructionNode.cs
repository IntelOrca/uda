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

		public override string ToString()
		{
			return String.Format("Node ({0})", Children.Count);
		}
	}
}
