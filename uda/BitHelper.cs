namespace uda
{
	internal static class BitHelper
	{
		public static uint RotateLeft(uint value, int count)
		{
			return (value << count) | (value >> (32 - count));
		}

		public static uint RotateRight(uint value, int count)
		{
			return (value >> count) | (value << (32 - count));
		}

		public static int SwapEndian(int value)
		{
			return (int)(
				(((uint)value & 0x000000FF) << 24) |
				(((uint)value & 0x0000FF00) <<  8) |
				(((uint)value & 0x00FF0000) >>  8) |
				(((uint)value & 0xFF000000) >> 24));
		}
	}
}