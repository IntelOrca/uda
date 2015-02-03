using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class ReturnStatement : ChildlessInstructionNodeBase, IInstructionNode
	{
		public InstructionType Type { get { return InstructionType.Return; } }

		public ReturnStatement() { }

		public override string ToString()
		{
			return "return";
		}
	}
}
