namespace uda.Intermediate
{
	internal enum InstructionType
	{
		// Low level instructions
		ConditionalJump,
		Jump,

		// High level instructions
		Assignment,
		Goto,
		IfStatement,
		Return
	}
}
