using uda.Intermediate;

namespace uda.Language
{
	internal class CLanguageWriter : ILanguageWriter
	{
		private void WriteInstruction(CodeWriter cw, IInstruction instruction)
		{
			switch (instruction.Type) {
			case InstructionType.Assignment:
				AssignmentInstruction assignInstr = (AssignmentInstruction)instruction;
				cw.AppendLine("{0} = {1};", assignInstr.Destination, assignInstr.Value);
				break;
			case InstructionType.Goto:
				GotoInstruction gotoInstr = (GotoInstruction)instruction;
				cw.AppendLine("goto loc_{0:X6};", gotoInstr.BasicBlock.BaseAddress);
				break;
			case InstructionType.IfStatement:
				IfStatement ifStatement = (IfStatement)instruction;
				cw.AppendLine("if ({0})", ifStatement.FirstExpression);
				cw.AppendLine("{");
				cw.BeginIndent();
				WriteBasicBlock(cw, ifStatement.FirstBlock);
				cw.EndIndent();
				cw.AppendLine("}");
				break;
			case InstructionType.Return:
				cw.AppendLine("return;");
				break;
			}
		}

		private void WriteBasicBlock(CodeWriter cw, IBasicBlock basicBlock)
		{
			if (basicBlock is BasicBlockReference)
				cw.AppendLine("goto loc_{0:X6};", basicBlock.BaseAddress);
		}

		private void WriteBasicBlock(CodeWriter cw, BasicBlock basicBlock)
		{
			cw.EndIndent();
			cw.AppendLine("loc_{0:X6}:", basicBlock.BaseAddress);
			cw.BeginIndent();
			foreach (IInstruction ii in basicBlock)
				WriteInstruction(cw, ii);
			cw.AppendLine();
		}

		public string Write(Function function)
		{
			BasicBlock entryBlock = function.BlockTable.EntryBlock;

			var cw = new CodeWriter();
			cw.AppendLine("void {0}()", function.Name);
			cw.AppendLine("{");
			cw.BeginIndent();

			foreach (BasicBlock block in function.BlockTable.Values)
				WriteBasicBlock(cw, block);

			cw.EndIndent();
			cw.AppendLine("}");

			return cw.ToString();
		}
	}
}
