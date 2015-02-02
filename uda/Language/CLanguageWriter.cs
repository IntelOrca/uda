using System.Linq;
using uda.Intermediate;

namespace uda.Language
{
	internal class CLanguageWriter : ILanguageWriter
	{
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
			case InstructionType.Goto:
				GotoStatement gotoInstr = (GotoStatement)node;
				cw.AppendLine("goto loc_{0:X6};", gotoInstr.Child.Address);
				break;
			case InstructionType.IfStatement:
				IfStatement ifStatement = (IfStatement)node;
				cw.AppendLine("if ({0})", ifStatement.FirstExpression);
				cw.AppendLine("{");
				cw.BeginIndent();
				WriteNode(cw, ifStatement.FirstChild);
				cw.EndIndent();
				cw.AppendLine("}");
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
					cw.AppendLine("{");
					cw.BeginIndent();
					WriteNode(cw, whileStatement.Child);
					cw.EndIndent();
					cw.AppendLine("}");
				}
				break;
			}
		}

		private void WriteTree(CodeWriter cw, InstructionTree tree, InstructionTree nextTree = null)
		{
			cw.EndIndent();
			cw.AppendLine("loc_{0:X6}:", tree.Address);
			cw.BeginIndent();

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
			cw.AppendLine("{");
			cw.BeginIndent();

			InstructionTree[] trees = function.InstructionTreeTable.ToArray();
			for (int i = 0; i < trees.Length; i++) {
				WriteTree(cw, trees[i], i != trees.Length - 1 ? trees[i + 1] : null);
			}

			cw.EndIndent();
			cw.AppendLine("}");

			return cw.ToString();
		}
	}
}
