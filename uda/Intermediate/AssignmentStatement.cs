using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class AssignmentStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly IWritableMemory _destination;
		private readonly IExpression _value;

		public InstructionType Type { get { return InstructionType.Assignment; } }
		public IWritableMemory Destination { get { return _destination; } }
		public IExpression Value { get { return _value; } }

		public AssignmentStatement(IWritableMemory destination, IExpression value)
		{
			_destination = destination;
			_value = value;
		}

		public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
		{
			return this;
		}

		public override string ToString()
		{
			return _destination.ToString() + " = " + _value.ToString();
		}
	}
}
