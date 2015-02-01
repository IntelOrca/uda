namespace uda.Intermediate
{
	internal class GotoInstruction : IInstruction
	{
		private readonly IBasicBlock _basicBlock;

		public InstructionType Type { get { return InstructionType.Goto; } }
		public IBasicBlock BasicBlock { get { return _basicBlock; } }

		public GotoInstruction(IBasicBlock basicBlock)
		{
			_basicBlock = basicBlock;
		}

		public override string ToString()
		{
			return "goto " + _basicBlock;
		}
	}
}
