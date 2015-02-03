using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class InstructionTree : InstructionNodeBase, IInstructionNode
	{
		private long _address;

		public InstructionType Type { get { return InstructionType.Block; } }
		public long Address { get { return _address; } }

		public InstructionTree(long address)
		{
			_address = address;
		}

		public InstructionTree(long address, IEnumerable<IInstructionNode> children) : base(children)
		{
			_address = address;
		}

		public InstructionTree(long address, ImmutableArray<IInstructionNode> children) : base(children)
		{
			_address = address;
		}

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return new InstructionTree(_address, children);
		}

		public override string ToString()
		{
			return String.Format("Tree 0x{0:X6}", _address);
		}
	}
}
