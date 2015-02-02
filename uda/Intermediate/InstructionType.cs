namespace uda.Intermediate
{
	internal enum InstructionType
	{
		Block,

		// Low level instructions
		ConditionalJump,
		Jump,

		// High level instructions
		Assignment,
		Goto,
		IfStatement,
		While,
		Return
	}
}
