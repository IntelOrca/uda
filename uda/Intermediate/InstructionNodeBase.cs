using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace uda.Intermediate
{
	internal abstract class InstructionNodeBase : IEnumerable<IInstructionNode>
	{
		private readonly ImmutableArray<IInstructionNode> _children;

		public IReadOnlyList<IInstructionNode> Children { get { return _children; } }
		public IInstructionNode FirstChild { get { return _children.FirstOrDefault(); } }
		public IInstructionNode LastChild { get { return _children.LastOrDefault(); } }

		protected InstructionNodeBase() : this(ImmutableArray<IInstructionNode>.Empty) { }
		protected InstructionNodeBase(IInstructionNode singleChild) : this(ImmutableArray.Create(singleChild)) { }
		protected InstructionNodeBase(IEnumerable<IInstructionNode> children) : this(ImmutableArray.CreateRange(children)) { }
		protected InstructionNodeBase(ImmutableArray<IInstructionNode> children)
		{
			_children = children;
		}

		public IInstructionNode ReplaceChild(int index, IInstructionNode newNode)
		{
			return ReplaceChild(_children[index], newNode);
		}

		public IInstructionNode ReplaceChild(IInstructionNode oldNode, IInstructionNode newNode)
		{
			if (oldNode == newNode)
				return (IInstructionNode)this;

			return CreateFromChildren(_children.Replace(oldNode, newNode));
		}

		public abstract IInstructionNode CreateFromChildren(ImmutableArray<IInstructionNode> children);

		public IEnumerator<IInstructionNode> GetEnumerator() { return _children.AsEnumerable().GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
