using System.Collections.Generic;

namespace uda.Intermediate
{
	internal interface IBasicBlock : IEnumerable<IInstruction>
	{
		long? BaseAddress { get; }
		IReadOnlyList<IInstruction> Instructions { get; }
	}
}
