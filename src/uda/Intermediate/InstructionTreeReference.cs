using System;

namespace uda.Intermediate
{
    internal class InstructionTreeReference : ChildlessInstructionNodeBase, IInstructionNode
    {
        private readonly InstructionTreeTable _instructionTreeTable;

        public InstructionType Type => InstructionType.Block;
        public long Address { get; }

        public InstructionTreeReference(InstructionTreeTable instructionTreeTable, long address)
        {
            _instructionTreeTable = instructionTreeTable;
            Address = address;
        }

        public override string ToString()
        {
            return String.Format("ref {0:X6}", Address);
        }
    }
}
