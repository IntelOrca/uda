using System;
using System.Text;

namespace uda
{
	internal class CodeWriter
	{
		private readonly StringBuilder _stringBuilder = new StringBuilder();
		private int _indentLevel = 0;

		public bool UseTabIndents { get; set; }
		public int SpacesPerIndent { get; set; }

		public CodeWriter()
		{
			SpacesPerIndent = 4;
		}

		private void AppendIndent()
		{
			if (UseTabIndents)
				_stringBuilder.Append(new String('\t', _indentLevel));
			else
				_stringBuilder.Append(new String(' ', _indentLevel * SpacesPerIndent));
		}

		public void AppendLine(string formatLine, params object[] args)
		{
			AppendLine(String.Format(formatLine, args));
		}

		public void AppendLine(string line)
		{
			AppendIndent();
			_stringBuilder.Append(line);
			_stringBuilder.AppendLine();
		}

		public void AppendLine()
		{
			_stringBuilder.AppendLine();
		}

		public void BeginIndent()
		{
			_indentLevel++;
		}

		public void EndIndent()
		{
			if (_indentLevel == 0)
				throw new InvalidOperationException("Indent level is 0.");
			_indentLevel--;
		}

		public void Clear()
		{
			_stringBuilder.Clear();
		}

		public void Remove(int startIndex, int length)
		{
			_stringBuilder.Remove(startIndex, length);
		}

		public override string ToString()
		{
			return _stringBuilder.ToString();
		}
	}
}
