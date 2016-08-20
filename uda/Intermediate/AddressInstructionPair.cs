using System;

namespace uda.Intermediate
{
    internal class AddressInstructionPair
    {
        public long? Address { get; }
        public IInstructionNode Instruction { get; }

        public AddressInstructionPair(IInstructionNode instruction)
        {
            Instruction = instruction;
        }

        public AddressInstructionPair(long address, IInstructionNode instruction)
        {
            Address = address;
            Instruction = instruction;
        }

        public override string ToString()
        {
            if (Address.HasValue)
            {
                return String.Format("0x{0:X6}: {1}", Address, Instruction);
            }
            else
            {
                return String.Format("        : {0}", Instruction);
            }
        }
    }
}
