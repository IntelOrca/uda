using System.Collections.Generic;

namespace uda.Intermediate
{
	internal class GotoStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly InstructionTreeReference _child;

		public InstructionType Type { get { return InstructionType.Goto; } }
		public InstructionTreeReference Child { get { return _child; } }

		public GotoStatement(InstructionTreeReference child) : base(child)
		{
			_child = child;
		}

		public override string ToString()
		{
			return "goto " + _child;
		}
	}
}
