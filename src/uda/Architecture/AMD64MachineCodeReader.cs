using System;
using System.Collections.Generic;
using System.IO;
using uda.Intermediate;

namespace uda.Architecture
{
    internal class AMD64MachineCodeReader : IMachineCodeReader, IDisposable
    {
        private readonly FileStream _peFileStream;
        private readonly BinaryReader _peBinaryReader;

        private enum RegisterFlag
        {
            Carry,
            Overflow,
            Zero
        }

        private enum ConditionalCode
        {
            NotZero
        }

        public AMD64MachineCodeReader(string portableExecutablePath)
        {
            _peFileStream = new FileStream(portableExecutablePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _peBinaryReader = new BinaryReader(_peFileStream);
        }

        public void Dispose()
        {
            _peFileStream.Dispose();
        }

        public long GetPhysicalAddress(long virtualAddress)
        {
            return virtualAddress - 0x400000;
        }

        public long GetVirtualAddress(long physicalAddress)
        {
            return physicalAddress + 0x400000;
        }

        public IEnumerable<AddressInstructionPair> Read(long startVirtualAddress)
        {
            long virtualAddress;
            IInstructionNode[] iis;

            virtualAddress = startVirtualAddress;
            _peFileStream.Seek(GetPhysicalAddress(startVirtualAddress), SeekOrigin.Begin);

            while ((iis = Read()) != null)
            {
                yield return new AddressInstructionPair(virtualAddress, iis[0]);
                for (int i = 1; i < iis.Length; i++)
                {
                    yield return new AddressInstructionPair(iis[i]);
                }

                virtualAddress = GetVirtualAddress(_peFileStream.Position);
                if (iis[0].Type == InstructionType.Return)
                {
                    break;
                }
            }
        }

        private IInstructionNode[] Read()
        {
            long virtualAddress = GetVirtualAddress(_peFileStream.Position);

            int prefix = 0;
            int modRegRm;
            int size;

            int byte0 = _peFileStream.ReadByte();

            // Read prefix
            switch (byte0) {
            case 0x66:
                prefix = byte0;
                byte0 = _peFileStream.ReadByte();
                break;
            }

            // Read opcode
            switch (byte0) {
            case 0x48:
            case 0x49:
            case 0x4A:
            case 0x4B:
            case 0x4C:
            case 0x4D:
            case 0x4E:
            case 0x4F:
                size = prefix == 0x66 ? 16 : 32;
                return ReadDecReg(byte0 & 7, size);
            case 0x75:
                return ReadJcc(ConditionalCode.NotZero, 8);
            case 0x83:
                modRegRm = _peBinaryReader.ReadByte();
                return ReadAddRegImm(modRegRm & 7, 32, 8);
            case 0xB8:
            case 0xB9:
            case 0xBA:
            case 0xBB:
            case 0xBC:
            case 0xBD:
            case 0xBE:
            case 0xBF:
                size = prefix == 0x66 ? 16 : 32;
                return ReadMovRegImm(byte0 & 7, size, size);
            case 0xC3:
                return new [] { new ReturnStatement() };
            case 0xC6:
                modRegRm = _peBinaryReader.ReadByte();
                return ReadMovAddrImm(modRegRm & 7, 32, 8, 8);
            }

            return null;
        }

        private IInstructionNode[] ReadDecReg(int regId, int regSize)
        {
            LocalExpression reg = GetRegister(regId, regSize);

            string instr = String.Format("dec {0}", reg.OriginalName);
            return new[]
            {
                new AssignmentStatement(reg, new SubtractExpression(reg, new LiteralExpression(1, regSize))),
                new AssignmentStatement(GetRegisterFlag(RegisterFlag.Zero), new EqualityExpression(reg, new LiteralExpression(0, regSize)))
            };
        }

        private IInstructionNode[] ReadJcc(ConditionalCode cc, int offsetSize)
        {
            long offset;
            switch (offsetSize) {
            case 8:
                offset = _peBinaryReader.ReadSByte();
                break;
            case 16:
                offset = _peBinaryReader.ReadInt16();
                break;
            case 32:
                offset = _peBinaryReader.ReadInt32();
                break;
            case 64:
                offset = _peBinaryReader.ReadInt64();
                break;
            default:
                throw new InvalidOperationException();
            }

            IExpression expression;
            switch (cc) {
            case ConditionalCode.NotZero:
                expression = new InequalityExpression(GetRegisterFlag(RegisterFlag.Zero), new LiteralExpression(0, 1));
                break;
            default:
                throw new InvalidOperationException();
            }

            long address = GetVirtualAddress(_peFileStream.Position);
            return new[] { new ConditionalJumpInstruction(expression, address + offset) };
        }

        private IInstructionNode[] ReadAddRegImm(int regId, int regSize, int opSize)
        {
            LocalExpression reg = GetRegister(regId, regSize);
            LiteralExpression imm = ReadImmediate(opSize);

            string instr = String.Format("add {0}, {1:X2}h", reg.OriginalName, imm);
            return new[] { new AssignmentStatement(reg, new AddExpression(reg, imm)) };
        }

        private IInstructionNode[] ReadMovRegImm(int regId, int regSize, int opSize)
        {
            LocalExpression reg = GetRegister(regId, regSize);
            LiteralExpression imm = ReadImmediate(opSize);

            string instr = String.Format("mov {0}, {1:X2}h", reg.OriginalName, imm);
            return new[] { new AssignmentStatement(reg, imm) };
        }

        private IInstructionNode[] ReadMovAddrImm(int regId, int regSize, int memSize, int opSize)
        {
            LocalExpression reg = GetRegister(regId, regSize);
            LiteralExpression imm = ReadImmediate(opSize);

            string instr = String.Format("mov {0} ptr [{1}], {2:X2}h", GetSizeName(memSize), reg.OriginalName, imm);
            return new[] { new AssignmentStatement(new AddressOfExpression(reg), imm) };
        }

        private LiteralExpression ReadImmediate(int size)
        {
            switch (size) {
            case 8: return new LiteralExpression(_peBinaryReader.ReadByte());
            case 16: return new LiteralExpression(_peBinaryReader.ReadInt16());
            case 32: return new LiteralExpression(_peBinaryReader.ReadInt32());
            case 64: return new LiteralExpression(_peBinaryReader.ReadInt64());
            default: throw new InvalidOperationException("ReadImmediate size must be 8, 16, 32 or 64.");
            }
        }

        private static LocalExpression GetRegister(int id, int size)
        {
            string regName = GetRegisterName(id, size);
            int offset = 0;
            if (size == 8 && id >= 4)
            {
                id -= 4;
                offset = 8;
            }
            return new LocalExpression(id, null, regName, offset, size);
        }

        private static LocalExpression GetRegisterFlag(RegisterFlag flag)
        {
            string[] flagNames = new string[] { "cf", "of", "zf" };
            return new LocalExpression(8 + (int)flag, null, flagNames[(int)flag], 0, 1);
        }

        private static string GetRegisterName(int id, int size)
        {
            string[] sz8 = new[] { "al", "cl", "dl", "bl", "ah", "ch", "dh", "ch" };
            string[] sz16 = new[] { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di" };

            switch (size) {
            case 8: return sz8[id];
            case 16: return sz16[id];
            case 32: return 'e' + sz16[id];
            case 64: return 'r' + sz16[id];
            default: throw new InvalidOperationException("Invalid register size.");
            }
        }

        private static string GetSizeName(int size)
        {
            switch (size) {
            case 8: return "byte";
            case 16: return "word";
            case 32: return "dword";
            case 64: return "qword";
            default: throw new InvalidOperationException("Invalid size.");
            }
        }
    }
}
