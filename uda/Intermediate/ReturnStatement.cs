namespace uda.Intermediate
{
	internal class ReturnStatement : InstructionNodeBase, IInstructionNode
	{
		public InstructionType Type { get { return InstructionType.Return; } }

		public ReturnStatement() { }

		public override string ToString()
		{
			return "return";
		}
	}
}
