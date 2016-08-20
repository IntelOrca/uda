using uda.Core;

namespace uda.Intermediate
{
    internal interface IInstructionNode : IGreenNode<IInstructionNode>
    {
        InstructionType Type { get; }
    }
}
