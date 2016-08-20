using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uda.Intermediate;

namespace uda.Strategy
{
    internal class LocalSubsitutionStrategy : IDecompileStrategy
    {
        private class CachedLocal
        {
            private readonly int _id;
            private readonly string _name;
            private readonly string _originalName;
            private readonly int _size;
            private readonly IExpression _value;

            public int Id { get { return _id; } }
            public string Name { get { return _name; } }
            public string OriginalName { get { return _originalName; } }
            public int Size { get { return _size; } }
            public IExpression Value { get { return _value; } }

            public CachedLocal(int id, string name, string originalName, int size, IExpression value)
            {
                _id = id;
                _name = name;
                _originalName = originalName;
                _size = size;
                _value = value;
                _value = value;
            }

            public override string ToString()
            {
                return String.Format("{0}. {1} = {2}", _id, _name ?? _originalName, _value);
            }
        }

        public void Process(Function function)
        {
            InstructionTreeTable treeTable = function.InstructionTreeTable;
            foreach (InstructionTree tree in treeTable.ToArray()) {
                InstructionTree newTree = (InstructionTree)Process(tree);
                if (tree != newTree)
                    treeTable.Add(newTree);
            }
        }

        private IInstructionNode Process(IInstructionNode node)
        {
            return Process(node, ImmutableArray<CachedLocal>.Empty);
        }

        private IInstructionNode Process(IInstructionNode node, IReadOnlyList<CachedLocal> inCachedLocals)
        {
            List<CachedLocal> newCachedLocals = new List<CachedLocal>(inCachedLocals);

            switch (node.Type) {
            case InstructionType.Do:
                LocalExpression[] writtenToLocals = GetReferencedLocals(node, false, true).ToArray();
                newCachedLocals.RemoveAll(x => writtenToLocals.Any(y => x.Id == y.Id));
                break;
            }

            for (int i = 0; i < node.Children.Count; i++) {
                IInstructionNode child = node.Children[i];
                if (child.Type == InstructionType.Assignment) {
                    AssignmentStatement assignmentStatement = (AssignmentStatement)child;
                    IExpression newDestination = (IExpression)assignmentStatement.Destination;
                    IExpression newValue = assignmentStatement.Value;

                    // Subsitute left hand side
                    if (assignmentStatement.Destination is AddressOfExpression)
                        newDestination = Subsitute((IExpression)assignmentStatement.Destination, newCachedLocals);

                    // Subsitute right hand side
                    newValue = Subsitute(assignmentStatement.Value, newCachedLocals);

                    if (assignmentStatement.Destination != newDestination || assignmentStatement.Value != newValue) {
                        assignmentStatement = new AssignmentStatement((IWritableMemory)newDestination, newValue);
                        node = node.ReplaceChild(i, assignmentStatement);
                    }

                    // Cache left hand side
                    LocalExpression local = assignmentStatement.Destination as LocalExpression;
                    if (local != null) {
                        newCachedLocals.Add(new CachedLocal(
                            local.Id,
                            local.Name,
                            local.OriginalName,
                            local.Width,
                            assignmentStatement.Value
                        ));
                    }
                }
                if (child.Children.Count > 0)
                    node = node.ReplaceChild(i, Process(child, newCachedLocals));
            }

            return node;
        }

        private IEnumerable<LocalExpression> GetReferencedLocals(IInstructionNode node, bool read, bool write)
        {
            if (node.Type == InstructionType.Assignment) {
                AssignmentStatement assignmentStatement = (AssignmentStatement)node;
                if (assignmentStatement.Destination is LocalExpression) {
                    // Locals that are written to
                    if (write)
                        yield return (LocalExpression)assignmentStatement.Destination;
                } else if (read) {
                    // Locals that are read in the destination expression
                    foreach (LocalExpression expr in ((IExpression)assignmentStatement.Destination).GetDescendants())
                        yield return expr;
                }
            } else if (read) {
                // Locals that are read
                foreach (IExpression expr1 in Expression.GetAllExpressions(node))
                    foreach (LocalExpression expr2 in expr1.GetDescendants().OfType<LocalExpression>())
                        yield return expr2;
            }

            foreach (IInstructionNode child in node.Children)
                foreach (LocalExpression expr in GetReferencedLocals(child, read, write))
                    yield return expr;
        }

        private IExpression Subsitute(IExpression expr, IEnumerable<CachedLocal> locals)
        {
            ImmutableArray<IExpression>.Builder newChildren = null;

            if (expr is LocalExpression) {
                LocalExpression localExpr = (LocalExpression)expr;
                CachedLocal cachedLocal = locals.FirstOrDefault(x => localExpr.Id == x.Id);
                return cachedLocal == null ? localExpr : cachedLocal.Value;
            }

            int index = 0;
            foreach (IExpression child in expr.Children) {
                IExpression newChild = Subsitute(child, locals);
                if (child != newChild) {
                    if (newChildren == null) {
                        newChildren = ImmutableArray.CreateBuilder<IExpression>(expr.Children.Count);
                        newChildren.AddRange(expr.Children);
                    }
                    newChildren[index] = newChild;
                }
                index++;
            }

            if (newChildren == null)
                return expr;

            return expr.CreateFromChildren(newChildren.ToImmutable());
        }
    }
}
