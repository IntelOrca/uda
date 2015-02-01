using System;
using System.Collections.Immutable;

namespace uda.Intermediate
{
	internal class IfStatement : IInstruction
	{
		private readonly ImmutableArray<Tuple<IExpression, IBasicBlock>> _expressionBlockPairs;
		private readonly IBasicBlock _elseBasicBlock;

		public InstructionType Type { get { return InstructionType.IfStatement; } }

		public ImmutableArray<Tuple<IExpression, IBasicBlock>> ExpressionBlockPairs { get { return _expressionBlockPairs; } }
		public IBasicBlock ElseBlock { get { return _elseBasicBlock; } }

		public IExpression FirstExpression { get { return _expressionBlockPairs[0].Item1; } }
		public IBasicBlock FirstBlock { get { return _expressionBlockPairs[0].Item2; } }

		public IfStatement(IExpression expression, IBasicBlock basicBlock)
		{
			_expressionBlockPairs = ImmutableArray.Create(new Tuple<IExpression, IBasicBlock>(expression, basicBlock));
		}

		public IfStatement(IExpression expression, IBasicBlock basicBlock, IBasicBlock elseBasicBlock)
		{
			_expressionBlockPairs = ImmutableArray.Create(new Tuple<IExpression, IBasicBlock>(expression, basicBlock));
			_elseBasicBlock = elseBasicBlock;
		}

		public override string ToString()
		{
			return String.Format("if ({0}) {1}", FirstExpression, FirstBlock);
		}
	}
}
