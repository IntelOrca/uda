using System.Collections.Generic;
using System.Collections.Immutable;
using uda.Core;

namespace uda.Intermediate
{
	internal abstract class InstructionNodeBase : GreenNode<IInstructionNode>
	{
		protected InstructionNodeBase() : base() { }
		protected InstructionNodeBase(IInstructionNode singleChild) : base(singleChild) { }
		protected InstructionNodeBase(IEnumerable<IInstructionNode> children) : base(children) { }
		protected InstructionNodeBase(ImmutableArray<IInstructionNode> children) : base(children) { }
	}
}
