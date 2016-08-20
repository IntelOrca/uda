using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class InstructionTreeReference : ChildlessInstructionNodeBase, IInstructionNode
    {
        private readonly InstructionTreeTable _instructionTreeTable;
        private readonly long _address;

        public InstructionType Type { get { return InstructionType.Block; } }
        public long Address { get { return _address; } }

        public InstructionTreeReference(InstructionTreeTable instructionTreeTable, long address)
        {
            _instructionTreeTable = instructionTreeTable;
            _address = address;
        }

        public override string ToString()
        {
            return string.Format("ref {0:X6}", _address);
        }
    }
}
