using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using uda.Intermediate;

namespace uda.Strategy
{
    /// <summary>
    /// Organise all the locals so that they are numbered from zero in order of first usage.
    /// </summary>
    internal class LocalRenumberStrategy : IDecompileStrategy
    {
        private Dictionary<int, int> _idRemap = new Dictionary<int, int>();

        public void Process(Function function)
        {
            InstructionTreeTable treeTable = function.InstructionTreeTable;

            List<LocalExpression> localExpressions = new List<LocalExpression>();
            foreach (InstructionTree tree in treeTable)
            {
                localExpressions.AddRange(GetAllExpressionsInTree(tree).OfType<LocalExpression>());
            }

            int[] usedLocalIds = localExpressions
                .Select(x => x.Id)
                .Distinct()
                .ToArray();

            for (int i = 0; i < usedLocalIds.Length; i++)
            {
                _idRemap[usedLocalIds[i]] = i;
            }

            foreach (InstructionTree tree in treeTable.ToArray())
            {
                treeTable.Add(RemapTree(tree));
            }
        }

        private IEnumerable<IExpression> GetAllExpressionsInTree(IInstructionNode node)
        {
            foreach (IExpression expr in Expression.GetAllExpressions(node))
            {
                yield return expr;
            }

            foreach (IInstructionNode child in node.Children)
            {
                foreach (IExpression expr in GetAllExpressionsInTree(child))
                {
                    yield return expr;
                }
            }
        }

        private InstructionTree RemapTree(InstructionTree tree)
        {
            IReadOnlyList<IInstructionNode> originalInstructions = tree.Children;

            var instructions = ImmutableArray.CreateBuilder<IInstructionNode>(originalInstructions.Count);
            for (int i = 0; i < originalInstructions.Count; i++)
            {
                instructions.Add(RemapInstruction(originalInstructions[i]));
            }

            return new InstructionTree(tree.Address, instructions.ToImmutable());
        }

        private IInstructionNode RemapInstruction(IInstructionNode instruction)
        {
            switch (instruction.Type) {
            case InstructionType.Assignment:
                AssignmentStatement assignInstr = (AssignmentStatement)instruction;
                return new AssignmentStatement(
                    (IWritableMemory)RemapExpressionTree((IExpression)assignInstr.Destination),
                    RemapExpressionTree(assignInstr.Value)
                );
            case InstructionType.If:
                IfStatement ifStatement = (IfStatement)instruction;
                return new IfStatement(
                    RemapExpressionTree(ifStatement.FirstExpression),
                    RemapInstruction(ifStatement.FirstChild)
                );
            default:
                return instruction;
            }
        }

        private IExpression RemapExpressionTree(IExpression tree)
        {
            if (tree is LocalExpression)
            {
                return RemapLocalExpression((LocalExpression)tree);
            }
            else if (tree is AddressOfExpression)
            {
                AddressOfExpression expr = (AddressOfExpression)tree;
                return new AddressOfExpression(RemapExpressionTree(expr.Child));
            }
            else if (tree is AddExpression)
            {
                AddExpression expr = (AddExpression)tree;
                return new AddExpression(RemapExpressionTree(expr.LeftChild), RemapExpressionTree(expr.RightChild));
            }
            else if (tree is SubtractExpression)
            {
                SubtractExpression expr = (SubtractExpression)tree;
                return new SubtractExpression(RemapExpressionTree(expr.LeftChild), RemapExpressionTree(expr.RightChild));
            }
            else if (tree is EqualityExpression)
            {
                EqualityExpression expr = (EqualityExpression)tree;
                return new EqualityExpression(RemapExpressionTree(expr.LeftChild), RemapExpressionTree(expr.RightChild));
            }
            else if (tree is InequalityExpression)
            {
                InequalityExpression expr = (InequalityExpression)tree;
                return new InequalityExpression(RemapExpressionTree(expr.LeftChild), RemapExpressionTree(expr.RightChild));
            }
            else
            {
                return tree;
            }
        }

        private LocalExpression RemapLocalExpression(LocalExpression original)
        {
            return new LocalExpression(
                _idRemap[original.Id],
                original.Name,
                original.OriginalName,
                original.Offset,
                original.Width
            );
        }
    }
}
