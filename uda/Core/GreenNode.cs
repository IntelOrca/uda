using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace uda.Core
{
    internal interface IGreenNode<T> : IEnumerable<T>
    {
        IReadOnlyList<T> Children { get; }
        T FirstChild { get; }
        T LastChild { get; }

        T ReplaceChild(int index, T newNode);
        T CreateFromChildren(ImmutableArray<T> children);
        IEnumerable<T> GetDescendants();
    }

    internal abstract class GreenNode<T> : IGreenNode<T> where T : class
    {
        private readonly ImmutableArray<T> _children;

        public IReadOnlyList<T> Children { get { return _children; } }
        public T FirstChild { get { return _children.FirstOrDefault(); } }
        public T LastChild { get { return _children.LastOrDefault(); } }

        protected GreenNode()
            : this(ImmutableArray<T>.Empty)
        {
        }

        protected GreenNode(T singleChild) :
            this(ImmutableArray.Create(singleChild))
        {
        }

        protected GreenNode(IEnumerable<T> children) :
            this(children is ImmutableArray<T> ? (ImmutableArray<T>)children : ImmutableArray.CreateRange(children))
        {
        }

        protected GreenNode(ImmutableArray<T> children)
        {
            _children = children;
        }

        public T ReplaceChild(int index, T newNode)
        {
            if (ReferenceEquals(_children[index], newNode))
            {
                return (T)(object)this;
            }

            var newChildren = ImmutableArray.CreateBuilder<T>();
            newChildren.AddRange(_children);
            newChildren[index] = newNode;

            return CreateFromChildren(newChildren.ToImmutableArray());
        }

        public abstract T CreateFromChildren(ImmutableArray<T> children);

        public IEnumerable<T> GetDescendants()
        {
            foreach (IGreenNode<T> child in _children)
            {
                foreach (T descendant in child.GetDescendants())
                {
                    yield return descendant;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _children.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
