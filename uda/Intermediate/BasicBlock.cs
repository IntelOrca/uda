using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace uda.Intermediate
{
	internal class BasicBlock : IBasicBlock, IEnumerable<IInstruction>
	{
		private readonly long? _baseAddress;
		private readonly ImmutableArray<IInstruction> _instructions;

		public long? BaseAddress { get { return _baseAddress; } }
		public IReadOnlyList<IInstruction> Instructions { get { return _instructions; } }

		public BasicBlock(IEnumerable<IInstruction> instructions) : this(null, ImmutableArray.CreateRange(instructions)) { }
		public BasicBlock(long? baseAddress, IEnumerable<IInstruction> instructions) : this(baseAddress, ImmutableArray.CreateRange(instructions)) { }
		public BasicBlock(long? baseAddress, ImmutableArray<IInstruction> instructions)
		{
			_baseAddress = baseAddress;
			_instructions = instructions;
		}

		public BasicBlock ReplaceInstruction(int index, IInstruction newInstruction)
		{
			var builder = ImmutableArray.CreateBuilder<IInstruction>(_instructions.Length);
			for (int i = 0; i < _instructions.Length; i++)
				builder.Add(i == index ? newInstruction : _instructions[i]);

			return new BasicBlock(_baseAddress, builder.ToImmutable());
		}

		public IEnumerator<IInstruction> GetEnumerator()
		{
			return _instructions.AsEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			if (_baseAddress.HasValue)
				return String.Format("BasicBlock ({0:X6})", _baseAddress);
			else
				return "BasicBlock";
		}
	}
}
