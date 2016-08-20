namespace uda.Intermediate
{
    internal class ReturnStatement : ChildlessInstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.Return;

        public ReturnStatement() { }

        public override string ToString()
        {
            return "return";
        }
    }
}
