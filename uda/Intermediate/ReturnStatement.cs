using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class ReturnStatement : InstructionNodeBase, IInstructionNode
	{
		public InstructionType Type { get { return InstructionType.Return; } }

		public ReturnStatement() { }

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return this;
		}

		public override string ToString()
		{
			return "return";
		}
	}
}
