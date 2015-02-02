using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uda.Intermediate
{
	internal class WhileStatement : InstructionNodeBase, IInstructionNode
	{
		private readonly IExpression _expression;
		private readonly IInstructionNode _child;

		public InstructionType Type { get { return InstructionType.While; } }
		public IExpression Expression { get { return _expression; } }
		public IInstructionNode Child { get { return _child; } }

		public WhileStatement(IExpression expression, IInstructionNode child) : base(child)
		{
			_expression = expression;
			_child = child;
		}

		public override string ToString()
		{
			return String.Format("while ({0}) {1}", _expression, _child);
		}
	}
}
