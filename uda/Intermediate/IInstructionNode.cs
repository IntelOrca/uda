using System.Collections.Generic;
using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal interface IInstructionNode : IEnumerable<IInstructionNode>
	{
		InstructionType Type { get; }
		IReadOnlyList<IInstructionNode> Children { get; }
		IInstructionNode FirstChild { get; }
		IInstructionNode LastChild { get; }

		IInstructionNode ReplaceChild(int index, IInstructionNode newNode);
		IInstructionNode ReplaceChild(IInstructionNode oldNode, IInstructionNode newNode);
		IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children);
	}
}
