using System.Collections;
using System.Collections.Generic;

namespace uda.Intermediate
{
	internal class BasicBlockReference : IBasicBlock
	{
		private BasicBlockTable _basicBlockTable;
		private readonly long _baseAddress;

		public long? BaseAddress { get { return _baseAddress; } }
		public BasicBlock BasicBlock { get { return _basicBlockTable[_baseAddress]; } }
		public IReadOnlyList<IInstruction> Instructions { get { return BasicBlock.Instructions; } }

		public BasicBlockReference(BasicBlockTable basicBlockTable, long baseAddress)
		{
			_basicBlockTable = basicBlockTable;
			_baseAddress = baseAddress;
		}

		public IEnumerator<IInstruction> GetEnumerator() { return BasicBlock.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public override string ToString()
		{
			return "ref " + BasicBlock;
		}
	}
}
