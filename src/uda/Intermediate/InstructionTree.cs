using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class InstructionTree : InstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.Block;
        public long Address { get; }

        public InstructionTree(long address)
        {
            Address = address;
        }

        public InstructionTree(long address, IEnumerable<IInstructionNode> children) : base(children)
        {
            Address = address;
        }

        public InstructionTree(long address, ImmutableArray<IInstructionNode> children) : base(children)
        {
            Address = address;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return new InstructionTree(Address, children);
        }

        public override string ToString()
        {
            return String.Format("Tree 0x{0:X6}", Address);
        }
    }
}
