using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace uda.Intermediate
{
	internal class BasicBlockTable : Dictionary<long, BasicBlock>
	{
		public BasicBlock EntryBlock { get; set; }

		public static BasicBlockTable CreateFromInstructions(IEnumerable<AddressInstructionPair> inputInstructions)
		{
			BasicBlockTable blockTable = new BasicBlockTable();

			AddressInstructionPair[] addrInstrPairs = inputInstructions.AsArray();
			if (addrInstrPairs.Length == 0)
				return blockTable;

			long entryAddress = addrInstrPairs[0].Address.Value;

			// Find addresses that are jumped to
			HashSet<long> jumpDestinationAddresses = addrInstrPairs
				.Select(x => x.Instruction)
				.Where(x => x.Type == InstructionType.Jump || x.Type == InstructionType.ConditionalJump)
				.Select(x => ((IJumpInstruction)x).Address)
				.ToHashSet();

			// Include the first instruction address in the set too
			jumpDestinationAddresses.Add(entryAddress);

			List<IInstruction> currentBlockInstructions = new List<IInstruction>();
			long currentBlockBaseAddress = 0;

			foreach (AddressInstructionPair aiPair in addrInstrPairs) {
				if (currentBlockInstructions.Count == 0) {
					Debug.Assert(aiPair.Address.HasValue);
					currentBlockBaseAddress = aiPair.Address.Value;
				}

				IInstruction instr = aiPair.Instruction;
				bool beginNewBasicBlock = false;
				bool endBasicBlock = false;
				if (aiPair.Address.HasValue && jumpDestinationAddresses.Contains(aiPair.Address.Value)) {
					beginNewBasicBlock = true;
				} else {
					switch (instr.Type) {
					case InstructionType.Jump:
						JumpInstruction jInstr = (JumpInstruction)instr;
						instr = new GotoInstruction(new BasicBlockReference(blockTable, jInstr.Address));
						endBasicBlock = true;
						break;
					case InstructionType.ConditionalJump:
						ConditionalJumpInstruction cjInstr = (ConditionalJumpInstruction)instr;
						instr = new IfStatement(cjInstr.Expression, new BasicBlockReference(blockTable, cjInstr.Address));
						endBasicBlock = true;
						break;
					case InstructionType.Return:
						endBasicBlock = true;
						break;
					}
				}

				if (beginNewBasicBlock && currentBlockInstructions.Count > 0) {
					currentBlockInstructions.Add(new GotoInstruction(new BasicBlockReference(blockTable, aiPair.Address.Value)));
					blockTable.Add(currentBlockBaseAddress, new BasicBlock(currentBlockBaseAddress, currentBlockInstructions));
					currentBlockInstructions.Clear();
					currentBlockBaseAddress = aiPair.Address.Value;
				}

				currentBlockInstructions.Add(instr);

				if (endBasicBlock) {
					blockTable.Add(currentBlockBaseAddress, new BasicBlock(currentBlockBaseAddress, currentBlockInstructions));
					currentBlockInstructions.Clear();
				}
			}

			// Add last working block to the table
			if (currentBlockInstructions.Count > 0)
				blockTable.Add(currentBlockBaseAddress, new BasicBlock(currentBlockBaseAddress, currentBlockInstructions));

			// Set the entry block to the block with the entry address
			blockTable.EntryBlock = blockTable[entryAddress];

			return blockTable;
		}
	}
}
