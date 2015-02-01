namespace uda.Intermediate
{
	internal interface IJumpInstruction : IInstruction
	{
		long Address { get; }
	}
}
