using System.Collections.Immutable;

namespace uda.Intermediate
{
    internal class GotoStatement : InstructionNodeBase, IInstructionNode
    {
        public InstructionType Type => InstructionType.Goto;
        public InstructionTreeReference Child { get; }

        public GotoStatement(InstructionTreeReference child) : base(child)
        {
            Child = child;
        }

        public override IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children)
        {
            return new InstructionNode(children);
        }

        public override string ToString()
        {
            return "goto " + Child;
        }
    }
}
