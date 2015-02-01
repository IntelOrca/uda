﻿using System;

namespace uda.Intermediate
{
	internal class AddressInstructionPair
	{
		private readonly long? _address;
		private readonly IInstruction _instruction;

		public long? Address { get { return _address; } }
		public IInstruction Instruction { get { return _instruction; } }

		public AddressInstructionPair(IInstruction instruction)
		{
			_instruction = instruction;
		}

		public AddressInstructionPair(long address, IInstruction instruction)
		{
			_address = address;
			_instruction = instruction;
		}

		public override string ToString()
		{
			if (_address.HasValue)
				return String.Format("0x{0:X6}: {1}", _address, _instruction);
			else
				return String.Format("        : {0}", _instruction);
		}
	}
}