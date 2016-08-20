using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class JumpInstruction : InstructionNodeBase, IJumpInstruction
    {
        public InstructionType Type => InstructionType.Jump;
        public long Address { get; }

        public JumpInstruction(long address)
        {
            Address = address;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return this;
        }

        public override string ToString()
        {
            return String.Format("jump 0x{0:X6}", Address);
        }
    }
}
