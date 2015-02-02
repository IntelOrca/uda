using System.Collections.Generic;

namespace uda.Intermediate
{
	internal interface IInstructionNode : IEnumerable<IInstructionNode>
	{
		InstructionType Type { get; }
		IReadOnlyList<IInstructionNode> Children { get; }
	}
}
