using System;

namespace uda.Intermediate
{
	internal class JumpInstruction : InstructionNodeBase, IJumpInstruction
	{
		private readonly long _address;

		public InstructionType Type { get { return InstructionType.Jump; } }
		public long Address { get { return _address; } }

		public JumpInstruction(long address)
		{
			_address = address;
		}

		public override string ToString()
		{
			return String.Format("jump 0x{0:X6}", _address);
		}
	}
}
