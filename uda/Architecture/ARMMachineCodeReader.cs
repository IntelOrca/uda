using System;
using System.Collections.Generic;
using System.IO;
using uda.Intermediate;

namespace uda.Architecture
{
	internal class ARMMachineCodeReader : IMachineCodeReader, IDisposable
	{
		FileStream _peFileStream;
		BinaryReader _peBinaryReader;

		/// <summary>
		/// CCCC XXXX XXXX XXXX XXXX XXXX XXXX XXXX
		/// </summary>
		private enum ConditionalCode
		{
			EQ, NE, CS, CC, MI, PL, VS, VC, HI, LS, GE, LT, GT, LE, AL, NV,
		}

		/// <summary>
		/// XXXX TTXX XXXX XXXX XXXX XXXX XXXX XXXX
		/// </summary>
		private enum ARMInstructionType
		{
			Arithmetic,
			LoadOrStore,
			BlockOrBranch,
			SoftwareInterrupt
		}

		/// <summary>
		/// XXXX XXTX XXXX XXXX XXXX XXXX XXXX XXXX
		/// </summary>
		private enum ARMInstructionSubType { Block, Branch }

		/// <summary>
		/// XXXX XXXO OOOX XXXX XXXX XXXX XXXX XXXX
		/// </summary>
		private enum ArithmeticOpcode
		{
			AND, EOR, SUB, RSB, ADD, ADC, SBC, RSC, TST, TEQ, CMP, CMN, ORR, MOV, BIC, MVN
		}

		private enum RegisterFlag
		{
			Carry,
			Overflow,
			Zero
		}

		public ARMMachineCodeReader(string binaryPath)
		{
			_peFileStream = new FileStream(binaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			_peBinaryReader = new BinaryReader(_peFileStream);
		}

		public void Dispose()
		{
			_peFileStream.Dispose();
		}

		public IEnumerable<AddressInstructionPair> Read(long startAddress)
		{
			long address;
			IInstruction[] iis;

			address = startAddress;
			_peFileStream.Seek(address, SeekOrigin.Begin);

			while ((iis = Read()) != null) {
				yield return new AddressInstructionPair(address, iis[0]);
				for (int i = 1; i < iis.Length; i++)
					yield return new AddressInstructionPair(iis[i]);

				address = _peFileStream.Position;
				if (iis[0].Type == InstructionType.Return)
					break;
			}
		}

		private IInstruction[] Read()
		{
			long address = _peFileStream.Position;
			int instruction = BitHelper.SwapEndian(_peBinaryReader.ReadInt32());

			switch (GetInstructionType(instruction)) {
			case ARMInstructionType.Arithmetic:
				return ReadArithmetic(instruction);
			case ARMInstructionType.LoadOrStore:
				return ReadLoadStore(instruction);
			case ARMInstructionType.BlockOrBranch:
				return null;
			case ARMInstructionType.SoftwareInterrupt:
				return null;
			}

			return null;
		}



		private IInstruction[] ReadArithmetic(int instruction)
		{
			ArithmeticOpcode opcode = (ArithmeticOpcode)((instruction >> 21) & 0x0F);
			bool setFlags = (instruction & (1 << 20)) != 0;
			LocalExpression operand1 = GetRegister((instruction >> 16) & 0x0F, 32);
			LocalExpression destination = GetRegister((instruction >> 12) & 0x0F, 32);
			IExpression operand2 = GetOperand2(instruction);
			IExpression operation;

			switch (opcode) {
			case ArithmeticOpcode.AND:
				operation = new AndExpression(operand1, operand2);
				break;
			case ArithmeticOpcode.ADD:
				operation = new AddExpression(operand1, operand2);
				break;
			case ArithmeticOpcode.ORR:
				operation = new OrExpression(operand1, operand2);
				break;
			case ArithmeticOpcode.MOV:
				operation = operand2;
				break;
			default:
				return null;
			}

			return new[] { new AssignmentInstruction(destination, operation) };
		}

		private IExpression GetOperand2(int instruction)
		{
			bool isImmediateOperand = (instruction & (1 << 25)) != 0;
			if (isImmediateOperand) {
				int rotate = ((instruction >> 8) & 0x0F) * 2;
				int imm = instruction & 0xFF;
				return new LiteralExpression((int)BitHelper.RotateRight((uint)imm, rotate));
			} else {
				LocalExpression rm = GetRegister(instruction & 0x07, 32);
				bool shiftAmountIsRegister = ((instruction >> 3) & 1) == 1;
				int shiftType = (instruction >> 5) & 3;
				IExpression shiftAmount = shiftAmountIsRegister ?
					(IExpression)GetRegister((instruction >> 8) & 0x07, 8) :
					(IExpression)new LiteralExpression((instruction >> 7) & 0x1F);

				switch (shiftType) {
				case 0: return new LogicalLeftShiftExpression(rm, shiftAmount);
				case 1: return new LogicalRightShiftExpression(rm, shiftAmount);
				case 2: return new ArithmeticRightShiftExpression(rm, shiftAmount);
				case 3: return new RotateRightShiftExpression(rm, shiftAmount);
				}

				throw new InvalidOperationException();
			}
		}

		private IInstruction[] ReadLoadStore(int instruction)
		{
			bool isImmediateOffset = (instruction & (1 << 25)) == 0;
			bool preIndex = (instruction & (1 << 24)) != 0;
			bool addOffset = (instruction & (1 << 23)) != 0;
			bool byteAccess = (instruction & (1 << 22)) != 0;
			bool writeback = (instruction & (1 << 21)) != 0;
			bool isLoad = (instruction & (1 << 20)) != 0;
			int addressRegisterIndex = (instruction >> 15) & 0x7;
			int valueRegisterIndex = (instruction >> 12) & 0x7;
			int offset = instruction & 0xFFF;

			int size = byteAccess ? 8 : 32;
			IExpression addressRegister = GetRegister(addressRegisterIndex, size);
			IExpression valueRegister = GetRegister(valueRegisterIndex, size);
			IExpression source, target;

			var result = new List<AssignmentInstruction>(2);

			IExpression offsetExpression = GetOffsetExpression(addressRegister, offset, addOffset);
			if (preIndex) {
				IExpression addressExpression;
				if (writeback && offset != 0) {
					result.Add(new AssignmentInstruction((IWritableMemory)addressRegister, offsetExpression));
					addressExpression = addressRegister;
				} else {
					addressExpression = offsetExpression;
				}
				if (isLoad) {
					source = new AddressOfExpression(addressExpression);
					target = valueRegister;
				} else {
					source = valueRegister;
					target = new AddressOfExpression(addressExpression);
				}
				result.Add(new AssignmentInstruction((IWritableMemory)target, source));
			} else {
				if (isLoad) {
					source = new AddressOfExpression(addressRegister);
					target = valueRegister;
				} else {
					source = valueRegister;
					target = new AddressOfExpression(addressRegister);
				}
				result.Add(new AssignmentInstruction((IWritableMemory)target, source));
				if (writeback)
					result.Add(new AssignmentInstruction((IWritableMemory)addressRegister, offsetExpression));
			}
			return result.ToArray();
		}

		private IExpression GetOffsetExpression(IExpression baseExpr, int offset, bool addOffset)
		{
			if (offset == 0)
				return baseExpr;

			return addOffset ?
				(IExpression)new AddExpression(baseExpr, new LiteralExpression(offset)) :
				(IExpression)new SubtractExpression(baseExpr, new LiteralExpression(offset));
		}

		private ARMInstructionType GetInstructionType(int instruction)
		{
			return (ARMInstructionType)((instruction >> 26) & 0x03);
		}

		private ConditionalCode GetConditionCode(int instruction)
		{
			return (ConditionalCode)((instruction >> 28) & 0x0F);
		}

		private static LocalExpression GetRegister(int id, int size)
		{
			string regName = GetRegisterName(id);
			return new LocalExpression(id, null, regName, 0, size);
		}

		private static LocalExpression GetRegisterFlag(RegisterFlag flag)
		{
			string[] flagNames = new string[] { "cf", "of", "zf" };
			return new LocalExpression(8 + (int)flag, null, flagNames[(int)flag], 0, 1);
		}

		private static string GetRegisterName(int id)
		{
			switch (id) {
			default: return "R" + id;
			case 13: return "SP";
			case 14: return "LR";
			case 15: return "PC";
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
