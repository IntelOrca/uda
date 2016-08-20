using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace uda.Intermediate
{
    internal class InstructionTreeTable : IEnumerable<InstructionTree>
    {
        private Dictionary<long, InstructionTree> _instructionTrees = new Dictionary<long, InstructionTree>();

        public InstructionTree EntryTree { get; set; }

        public InstructionTree this[long address]
        {
            get { return _instructionTrees[address]; }
        }

        public void Add(InstructionTree tree)
        {
            _instructionTrees[tree.Address] = tree;
            if (EntryTree != null && EntryTree.Address == tree.Address)
            {
                EntryTree = tree;
            }
        }

        public void Remove(long address)
        {
            _instructionTrees.Remove(address);
        }

        public void Clean()
        {
            foreach (InstructionTree tree in _instructionTrees.Values.ToArray())
            {
                IInstructionNode cleanTree = InstructionNode.Clean(tree);
                if (tree != cleanTree)
                {
                    Add((InstructionTree)cleanTree);
                }
            }
        }

        public IEnumerator<InstructionTree> GetEnumerator()
        {
            return _instructionTrees.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static InstructionTreeTable CreateFromInstructions(IEnumerable<AddressInstructionPair> inputInstructions)
        {
            InstructionTreeTable treeTable = new InstructionTreeTable();

            AddressInstructionPair[] addrInstrPairs = inputInstructions.AsArray();
            if (addrInstrPairs.Length == 0)
            {
                return treeTable;
            }

            long entryAddress = addrInstrPairs[0].Address.Value;

            // Find addresses that are jumped to
            HashSet<long> jumpDestinationAddresses = addrInstrPairs
                .Select(x => x.Instruction)
                .Where(x => x.Type == InstructionType.Jump || x.Type == InstructionType.ConditionalJump)
                .Select(x => ((IJumpInstruction)x).Address)
                .ToHashSet();

            // Include the first instruction address in the set too
            jumpDestinationAddresses.Add(entryAddress);

            List<IInstructionNode> currentTreeInstructions = new List<IInstructionNode>();
            long currentTreeAddress = 0;

            foreach (AddressInstructionPair aiPair in addrInstrPairs)
            {
                if (currentTreeInstructions.Count == 0)
                {
                    Debug.Assert(aiPair.Address.HasValue);
                    currentTreeAddress = aiPair.Address.Value;
                }

                IInstructionNode instr = aiPair.Instruction;
                bool beginNewTree = false;
                bool endTree = false;
                if (aiPair.Address.HasValue && jumpDestinationAddresses.Contains(aiPair.Address.Value))
                {
                    beginNewTree = true;
                }

                switch (instr.Type) {
                case InstructionType.Jump:
                    JumpInstruction jInstr = (JumpInstruction)instr;
                    instr = new GotoStatement(new InstructionTreeReference(treeTable, jInstr.Address));
                    endTree = true;
                    break;
                case InstructionType.ConditionalJump:
                    ConditionalJumpInstruction cjInstr = (ConditionalJumpInstruction)instr;
                    instr = new IfStatement(cjInstr.Expression, new InstructionTreeReference(treeTable, cjInstr.Address));
                    break;
                case InstructionType.Return:
                    endTree = true;
                    break;
                }

                if (beginNewTree && currentTreeInstructions.Count > 0)
                {
                    currentTreeInstructions.Add(new GotoStatement(new InstructionTreeReference(treeTable, aiPair.Address.Value)));
                    treeTable.Add(new InstructionTree(currentTreeAddress, currentTreeInstructions));
                    currentTreeInstructions.Clear();
                    currentTreeAddress = aiPair.Address.Value;
                }

                currentTreeInstructions.Add(instr);

                if (endTree)
                {
                    treeTable.Add(new InstructionTree(currentTreeAddress, currentTreeInstructions));
                    currentTreeInstructions.Clear();
                }
            }

            // Add last working tree to the table
            if (currentTreeInstructions.Count > 0)
            {
                treeTable.Add(new InstructionTree(currentTreeAddress, currentTreeInstructions));
            }

            // Set the entry tree to the tree with the entry address
            treeTable.EntryTree = treeTable[entryAddress];

            return treeTable;
        }
    }
}
