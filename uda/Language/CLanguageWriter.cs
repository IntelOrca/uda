using System.Collections.Generic;
using System.Linq;
using uda.Intermediate;

namespace uda.Language
{
	internal class CLanguageWriter : ILanguageWriter
	{
		private static void OpenBrace(CodeWriter cw)
		{
			cw.AppendLine("{");
			cw.BeginIndent();
		}

		private static void CloseBrace(CodeWriter cw)
		{
			cw.EndIndent();
			cw.AppendLine("}");
		}

		private void WriteNode(CodeWriter cw, IInstructionNode node)
		{
			if (node is InstructionTreeReference) {
				cw.AppendLine("goto loc_{0:X6};", ((InstructionTreeReference)node).Address);
				return;
			}

			switch (node.Type) {
			case InstructionType.Assignment:
				AssignmentStatement assignInstr = (AssignmentStatement)node;
				cw.AppendLine("{0} = {1};", assignInstr.Destination, assignInstr.Value);
				break;
			case InstructionType.Block:
				foreach (IInstructionNode child in node.Children)
					WriteNode(cw, child);
				break;
			case InstructionType.Do:
				DoWhileStatement doInstr = (DoWhileStatement)node;
				cw.AppendLine("do");
				OpenBrace(cw);
				WriteNode(cw, doInstr.Child);
				cw.EndIndent();
				cw.AppendLine("}} while ({0});", doInstr.Expression);
				break;
			case InstructionType.Goto:
				GotoStatement gotoInstr = (GotoStatement)node;
				cw.AppendLine("goto loc_{0:X6};", gotoInstr.Child.Address);
				break;
			case InstructionType.If:
				IfStatement ifStatement = (IfStatement)node;
				cw.AppendLine("if ({0})", ifStatement.FirstExpression);
				OpenBrace(cw);
				WriteNode(cw, ifStatement.FirstChild);
				CloseBrace(cw);
				break;
			case InstructionType.Return:
				cw.AppendLine("return;");
				break;
			case InstructionType.While:
				WhileStatement whileStatement = (WhileStatement)node;
				if (whileStatement.Child.Children.Count == 0) {
					cw.AppendLine("while ({0}) {{ }}", whileStatement.Expression);
				} else {
					cw.AppendLine("while ({0})", whileStatement.Expression);
					OpenBrace(cw);
					WriteNode(cw, whileStatement.Child);
					CloseBrace(cw);
				}
				break;
			}
		}

		private void WriteTree(CodeWriter cw, InstructionTree tree, InstructionTree nextTree = null, bool omitLabel = false)
		{
			if (!omitLabel) {
				cw.EndIndent();
				cw.AppendLine("loc_{0:X6}:", tree.Address);
				cw.BeginIndent();
			}

			for (int i = 0; i < tree.Children.Count; i++) {
				IInstructionNode instruction = tree.Children[i];

				if (i == tree.Children.Count - 1 && instruction.Type == InstructionType.Goto && nextTree != null) {
					long address = ((GotoStatement)instruction).Child.Address;
					if (address == nextTree.Address)
						continue;
				}

				WriteNode(cw, instruction);
			}

			if (nextTree != null)
				cw.AppendLine();
		}

		public string Write(Function function)
		{
			InstructionTree entryTree = function.InstructionTreeTable.EntryTree;

			var cw = new CodeWriter();
			cw.AppendLine("void {0}()", function.Name);
			OpenBrace(cw);

			List<InstructionTree> trees = function.InstructionTreeTable.ToList();
			trees.Remove(entryTree);
			trees.Insert(0, entryTree);

			for (int i = 0; i < trees.Count; i++)
				WriteTree(cw, trees[i], i != trees.Count - 1 ? trees[i + 1] : null, i == 0);

			CloseBrace(cw);

			return cw.ToString();
		}
	}
}
