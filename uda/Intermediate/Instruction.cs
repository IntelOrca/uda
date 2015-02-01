using System.Linq;

namespace uda.Intermediate
{
	internal static class Instruction
	{
		public static IExpression[] GetAllExpressions(IInstruction instruction)
		{
			switch (instruction.Type) {
			case InstructionType.Assignment:
				AssignmentInstruction assignInstr = (AssignmentInstruction)instruction;
				return new[] { (IExpression)assignInstr.Destination, assignInstr.Value };
			case InstructionType.IfStatement:
				IfStatement ifStatement = (IfStatement)instruction;
				return ifStatement.ExpressionBlockPairs.Select(x => x.Item1).ToArray();
			default:
				return new IExpression[0];
			}
		}
	}
}
