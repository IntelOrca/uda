namespace uda.Intermediate
{
	internal class ReturnInstruction : IInstruction
	{
		public InstructionType Type { get { return InstructionType.Return; } }

		public ReturnInstruction() { }

		public override string ToString()
		{
			return "return";
		}
	}
}
