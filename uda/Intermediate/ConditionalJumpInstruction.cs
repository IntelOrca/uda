using System;

namespace uda.Intermediate
{
    internal class ConditionalJumpInstruction : ChildlessInstructionNodeBase, IJumpInstruction
    {
        public InstructionType Type => InstructionType.ConditionalJump;
        public IExpression Expression { get; }
        public long Address { get; }

        public ConditionalJumpInstruction(IExpression expression, long address)
        {
            Expression = expression;
            Address = address;
        }

        public override string ToString()
        {
            return String.Format("if ({0}) jump 0x{1:X6}", Expression, Address);
        }
    }
}
