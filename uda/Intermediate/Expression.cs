using System;
using System.Collections.Generic;
using System.Linq;

namespace uda.Intermediate
{
	internal interface IExpression
	{
		IEnumerable<IExpression> Children { get; }
	}

	internal interface IBinaryExpression : IExpression
	{
		IExpression Left { get; }
		IExpression Right { get; }
	}

	internal static class Expression
	{
		public static IEnumerable<IExpression> GetFlattenedExpressionTree(IExpression head)
		{
			IExpression[] children = head.Children.AsArray();
			if (children.Length == 0) {
				yield return head;
				yield break;
			}

			foreach (IExpression expr1 in children)
				foreach (IExpression expr2 in GetFlattenedExpressionTree(expr1))
					yield return expr2;
		}
	}

	internal class LiteralExpression : IExpression
	{
		private readonly long _value;
		private readonly int _size;

		public LiteralExpression(long value, int size)
		{
			_value = value;
			_size = size;
		}

		public LiteralExpression(byte value) : this(value, 8) { }
		public LiteralExpression(short value) : this(value, 16) { }
		public LiteralExpression(int value) : this(value, 32) { }
		public LiteralExpression(long value) : this(value, 64) { }

		public IEnumerable<IExpression> Children { get { return Enumerable.Empty<IExpression>(); } }

		public override string ToString()
		{
			return _value.ToString();
		}
	}

	internal class AddressOfExpression : IExpression, IWritableMemory
	{
		private readonly IExpression _child;

		public IExpression Child { get { return _child; } }

		public AddressOfExpression(IExpression child)
		{
			_child = child;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _child }; } }

		public override string ToString()
		{
			return "[" + _child + "]";
		}
	}

	internal class LocalExpression : IExpression, IWritableMemory
	{
		private readonly int _id;
		private readonly string _name;
		private readonly string _originalName;
		private readonly int _offset;
		private readonly int _width;

		public int Id { get { return _id; } }
		public string Name { get { return _name; } }
		public string OriginalName { get { return _originalName; } }
		public int Offset { get { return _offset; } }
		public int Width { get { return _width; } }

		public LocalExpression(int id, string name, string originalName, int offset, int width)
		{
			_id = id;
			_name = name;
			_originalName = originalName;
			_offset = offset;
			_width = width;
		}

		public IEnumerable<IExpression> Children { get { return Enumerable.Empty<IExpression>(); } }

		public override string ToString()
		{
			if (String.IsNullOrEmpty(_name))
				return "local" + _id;
			return _name;
		}
	}

	internal class AddExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public AddExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " + " + _right + ")";
		}
	}

	internal class SubtractExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public SubtractExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " - " + _right + ")";
		}
	}

	internal class AndExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public AndExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " & " + _right + ")";
		}
	}

	internal class OrExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public OrExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " | " + _right + ")";
		}
	}

	internal class LogicalLeftShiftExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public LogicalLeftShiftExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " << " + _right + ")";
		}
	}

	internal class LogicalRightShiftExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public LogicalRightShiftExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " >> " + _right + ")";
		}
	}

	internal class ArithmeticRightShiftExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public ArithmeticRightShiftExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " >> " + _right + ")";
		}
	}

	internal class RotateRightShiftExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public RotateRightShiftExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " >>> " + _right + ")";
		}
	}

	internal class EqualityExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public EqualityExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " == " + _right + ")";
		}
	}

	internal class InequalityExpression : IBinaryExpression
	{
		private readonly IExpression _left, _right;

		public IExpression Left { get { return _left; } }
		public IExpression Right { get { return _right; } }

		public InequalityExpression(IExpression left, IExpression right)
		{
			_left = left;
			_right = right;
		}

		public IEnumerable<IExpression> Children { get { return new[] { _left, _right }; } }

		public override string ToString()
		{
			return "(" + _left + " != " + _right + ")";
		}
	}
}
