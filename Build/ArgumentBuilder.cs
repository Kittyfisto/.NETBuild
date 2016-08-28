using System.Text;

namespace Build
{
	public sealed class ArgumentBuilder
	{
		private readonly StringBuilder _builder;

		public ArgumentBuilder()
		{
			_builder = new StringBuilder();
		}

		public int Length
		{
			get { return _builder.Length; }
		}

		public void Flag(string flag)
		{
			_builder.Append('/');
			_builder.Append(flag);
			_builder.Append(' ');
		}

		public void Add(string value)
		{
			AppendWithSpaces(value);
			_builder.Append(' ');
		}

		public void Add(string parameter, string value)
		{
			_builder.Append('/');
			_builder.Append(parameter);
			_builder.Append(':');

			AppendWithSpaces(value);

			_builder.Append(' ');
		}

		private void AppendWithSpaces(string value)
		{
			if (value.Contains(" "))
			{
				_builder.Append('"');
				_builder.Append(value);
				_builder.Append('"');
			}
			else
			{
				_builder.Append(value);
			}
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		public void Clear()
		{
			_builder.Clear();
		}
	}
}