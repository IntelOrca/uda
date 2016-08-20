namespace uda.Intermediate
{
    internal class AssignmentStatement : ChildlessInstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.Assignment;
        public IWritableMemory Destination { get; }
        public IExpression Value { get; }

        public AssignmentStatement(IWritableMemory destination, IExpression value)
        {
            Destination = destination;
            Value = value;
        }

        public override string ToString()
        {
            return Destination.ToString() + " = " + Value.ToString();
        }
    }
}
