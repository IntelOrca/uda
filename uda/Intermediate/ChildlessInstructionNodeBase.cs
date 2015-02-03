using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class ChildlessInstructionNodeBase : InstructionNodeBase
	{
		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return (IInstructionNode)this;
		}
	}
}
