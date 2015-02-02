namespace uda.Intermediate
{
	internal interface IJumpInstruction : IInstructionNode
	{
		long Address { get; }
	}
}
