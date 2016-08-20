using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using uda.Core;

namespace uda.Intermediate
{
    internal interface IExpression : IGreenNode<IExpression> { }

    internal interface INullaryExpression : IExpression { }

    internal interface IUnaryExpression : IExpression
    {
        IExpression Child { get; }
    }

    internal interface IBinaryExpression : IExpression
    {
        IExpression LeftChild { get; }
        IExpression RightChild { get; }
    }

    internal static class Expression
    {
        public static IExpression[] GetAllExpressions(IInstructionNode instruction)
        {
            switch (instruction.Type) {
            case InstructionType.Assignment:
                AssignmentStatement assignInstr = (AssignmentStatement)instruction;
                return new[] { (IExpression)assignInstr.Destination, assignInstr.Value };
            case InstructionType.If:
                IfStatement ifStatement = (IfStatement)instruction;
                return ifStatement.ExpressionNodePairs.Select(x => x.Expression).ToArray();
            default:
                return new IExpression[0];
            }
        }

        public static IEnumerable<IExpression> GetFlattenedExpressionTree(IExpression head)
        {
            if (head.Children.Count == 0)
            {
                yield return head;
                yield break;
            }

            foreach (IExpression expr1 in head.Children)
            {
                foreach (IExpression expr2 in GetFlattenedExpressionTree(expr1))
                {
                    yield return expr2;
                }
            }
        }

        public static bool IsTautology(IExpression expression)
        {
            if (expression is LiteralExpression)
            {
                LiteralExpression literalExpression = (LiteralExpression)expression;
                return literalExpression.ValueUnsigned != 0;
            }

            return false;
        }
    }

    internal abstract class ExpressionBase : GreenNode<IExpression>, IExpression
    {
        protected ExpressionBase() : base() { }
        protected ExpressionBase(IExpression singleChild) : base(singleChild) { }
        protected ExpressionBase(IEnumerable<IExpression> children) : base(children) { }
        protected ExpressionBase(ImmutableArray<IExpression> children) : base(children) { }
    }

    internal abstract class NullaryExpressionBase : ExpressionBase, INullaryExpression
    {
        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return this;
        }
    }

    internal abstract class UnaryExpressionBase : ExpressionBase, IUnaryExpression
    {
        public IExpression Child { get; }

        protected UnaryExpressionBase(IExpression child)
            : base(child)
        {
            Child = child;
        }
    }

    internal abstract class BinaryExpressionBase : ExpressionBase, IBinaryExpression
    {
        public IExpression LeftChild { get; }
        public IExpression RightChild { get; }

        protected BinaryExpressionBase(IExpression leftChild, IExpression rightChild)
            : base(new[] { leftChild, rightChild })
        {
            LeftChild = leftChild;
            RightChild = rightChild;
        }
    }

    internal class LiteralExpression : NullaryExpressionBase
    {
        private readonly long _value;

        public int Size { get; }

        public ulong ValueUnsigned
        {
            get
            {
                return (ulong)(_value & ((1 << Size) - 1));
            }
        }

        public long ValueSigned
        {
            get
            {
                long result = _value & ((1 << Size) - 1);
                if ((result & (1 << (Size - 1))) != 0)
                {
                    result = (long)((ulong)result | ~(ulong)((1 << Size) - 1));
                }
                return result;
            }
        }

        public LiteralExpression(long value, int size)
        {
            _value = value;
            Size = size;
        }

        public LiteralExpression(byte value) : this(value, 8) { }
        public LiteralExpression(short value) : this(value, 16) { }
        public LiteralExpression(int value) : this(value, 32) { }
        public LiteralExpression(long value) : this(value, 64) { }

        public override string ToString()
        {
            return _value.ToString();
        }
    }

    internal class LocalExpression : NullaryExpressionBase, IWritableMemory
    {
        public int Id { get; }
        public string Name { get; }
        public string OriginalName { get; }
        public int Offset { get; }
        public int Width { get; }

        public LocalExpression(int id, string name, string originalName, int offset, int width)
        {
            Id = id;
            Name = name;
            OriginalName = originalName;
            Offset = offset;
            Width = width;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Name))
            {
                return "local" + Id;
            }
            return Name;
        }
    }

    internal class AddressOfExpression : UnaryExpressionBase, IWritableMemory
    {
        public AddressOfExpression(IExpression child) : base(child) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new AddressOfExpression(children[0]);
        }

        public override string ToString()
        {
            return "[" + Child + "]";
        }
    }

    internal class AddExpression : BinaryExpressionBase
    {
        public AddExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new AddExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " + " + RightChild + ")";
        }
    }

    internal class SubtractExpression : BinaryExpressionBase
    {
        public SubtractExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new SubtractExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " - " + RightChild + ")";
        }
    }

    internal class BooleanNotExpression : UnaryExpressionBase
    {
        public BooleanNotExpression(IExpression child) : base(child) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new BooleanNotExpression(children[0]);
        }

        public override string ToString()
        {
            return "!" + Child;
        }
    }

    internal class BitwiseAndExpression : BinaryExpressionBase
    {
        public BitwiseAndExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new BitwiseAndExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " & " + RightChild + ")";
        }
    }

    internal class BitwiseOrExpression : BinaryExpressionBase
    {
        public BitwiseOrExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new BitwiseOrExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " | " + RightChild + ")";
        }
    }

    internal class LogicalLeftShiftExpression : BinaryExpressionBase
    {
        public LogicalLeftShiftExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new LogicalLeftShiftExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " << " + RightChild + ")";
        }
    }

    internal class LogicalRightShiftExpression : BinaryExpressionBase
    {
        public LogicalRightShiftExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new LogicalRightShiftExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " >> " + RightChild + ")";
        }
    }

    internal class ArithmeticRightShiftExpression : BinaryExpressionBase
    {
        public ArithmeticRightShiftExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new ArithmeticRightShiftExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " >> " + RightChild + ")";
        }
    }

    internal class RotateRightShiftExpression : BinaryExpressionBase
    {
        public RotateRightShiftExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new RotateRightShiftExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " >>> " + RightChild + ")";
        }
    }

    internal class EqualityExpression : BinaryExpressionBase
    {
        public EqualityExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new EqualityExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " == " + RightChild + ")";
        }
    }

    internal class InequalityExpression : BinaryExpressionBase
    {
        public InequalityExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new InequalityExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " != " + RightChild + ")";
        }
    }

    internal class GreaterThanExpression : BinaryExpressionBase
    {
        public GreaterThanExpression(IExpression leftChild, IExpression rightChild) : base(leftChild, rightChild) { }

        public override IExpression CreateFromChildren(ImmutableArray<IExpression> children)
        {
            return new GreaterThanExpression(children[0], children[1]);
        }

        public override string ToString()
        {
            return "(" + LeftChild + " > " + RightChild + ")";
        }
    }
}
