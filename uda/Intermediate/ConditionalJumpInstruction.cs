using System;

namespace uda.Intermediate
{
	internal class ConditionalJumpInstruction : InstructionNodeBase, IJumpInstruction
	{
		private readonly IExpression _expression;
		private readonly long _address;

		public InstructionType Type { get { return InstructionType.ConditionalJump; } }
		public IExpression Expression { get { return _expression; } }
		public long Address { get { return _address; } }

		public ConditionalJumpInstruction(IExpression expression, long address)
		{
			_expression = expression;
			_address = address;
		}

		public override string ToString()
		{
			return String.Format("if ({0}) jump 0x{1:X6}", _expression, _address);
		}
	}
}
