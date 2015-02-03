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
		Do,
		Goto,
		If,
		While,
		Return
	}
}
