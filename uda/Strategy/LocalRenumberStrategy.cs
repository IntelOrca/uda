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
			BasicBlockTable blockTable = function.BlockTable;

			List<LocalExpression> localExpressions = new List<LocalExpression>();
			foreach (BasicBlock block in blockTable.Values) {
				foreach (IInstruction instruction in block.Instructions) {
					foreach (IExpression expr in Instruction.GetAllExpressions(instruction)) {
						localExpressions.AddRange(Expression.GetFlattenedExpressionTree(expr)
							.OfType<LocalExpression>()
							.ToArray()
						);
					}
				}
			}

			int[] usedLocalIds = localExpressions
				.Select(x => x.Id)
				.Distinct()
				.ToArray();

			for (int i = 0; i < usedLocalIds.Length; i++)
				_idRemap[usedLocalIds[i]] = i;

			foreach (long address in blockTable.Keys.ToArray())
				blockTable[address] = RemapBasicBlock(blockTable[address]);
		}

		private BasicBlock RemapBasicBlock(BasicBlock basicBlock)
		{
			IReadOnlyList<IInstruction> originalInstructions = basicBlock.Instructions;

			var instructions = ImmutableArray.CreateBuilder<IInstruction>(originalInstructions.Count);
			for (int i = 0; i < originalInstructions.Count; i++)
				instructions.Add(RemapInstruction(originalInstructions[i]));

			return new BasicBlock(basicBlock.BaseAddress, instructions.ToImmutable());
		}

		private IInstruction RemapInstruction(IInstruction instruction)
		{
			switch (instruction.Type) {
			case InstructionType.Assignment:
				AssignmentInstruction assignInstr = (AssignmentInstruction)instruction;
				return new AssignmentInstruction(
					(IWritableMemory)RemapExpressionTree((IExpression)assignInstr.Destination),
					RemapExpressionTree(assignInstr.Value)
				);
			case InstructionType.IfStatement:
				IfStatement ifStatement = (IfStatement)instruction;
				return new IfStatement(
					RemapExpressionTree(ifStatement.FirstExpression),
					ifStatement.FirstBlock
				);
			default:
				return instruction;
			}
		}

		private IExpression RemapExpressionTree(IExpression tree)
		{
			if (tree is LocalExpression) {
				return RemapLocalExpression((LocalExpression)tree);
			} else if (tree is AddressOfExpression) {
				AddressOfExpression expr = (AddressOfExpression)tree;
				return new AddressOfExpression(RemapExpressionTree(expr.Child));
			} else if (tree is AddExpression) {
				AddExpression expr = (AddExpression)tree;
				return new AddExpression(RemapExpressionTree(expr.Left), RemapExpressionTree(expr.Right));
			} else if (tree is SubtractExpression) {
				SubtractExpression expr = (SubtractExpression)tree;
				return new SubtractExpression(RemapExpressionTree(expr.Left), RemapExpressionTree(expr.Right));
			} else if (tree is EqualityExpression) {
				EqualityExpression expr = (EqualityExpression)tree;
				return new EqualityExpression(RemapExpressionTree(expr.Left), RemapExpressionTree(expr.Right));
			} else if (tree is InequalityExpression) {
				InequalityExpression expr = (InequalityExpression)tree;
				return new InequalityExpression(RemapExpressionTree(expr.Left), RemapExpressionTree(expr.Right));
			} else {
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
