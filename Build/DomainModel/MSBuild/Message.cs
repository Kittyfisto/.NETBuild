using System.Text;

namespace Build.DomainModel.MSBuild
{
	public sealed class Message
		: Node
	{
		public enum Type
		{
			Message,
			Warning,
			Error
		}

		private readonly Importance _importance;
		private readonly string _text;
		private readonly Type _type;

		public Message(string text,
		               Type type,
		               Importance importance = Importance.Normal,
		               Condition condition = null)
			: base(condition)
		{
			_text = text;
			_type = type;
			_importance = importance;
		}

		public override string ToString()
		{
			string token;
			switch (_type)
			{
				case Type.Error:
					token = "Error";
					break;

				case Type.Message:
					token = "Message";
					break;

				case Type.Warning:
					token = "Warning";
					break;

				default:
					token = null;
					break;
			}

			var builder = new StringBuilder();
			builder.AppendFormat("<{0} ", token);
			builder.AppendFormat("Text=\"{0}\" ", _text);
			if (Condition != null)
			{
				builder.AppendFormat("{0} ", Condition);
			}
			builder.AppendFormat("Importance=\"{0}\" ", _importance);
			builder.Append("/>");
			return builder.ToString();
		}

		public Importance Importance
		{
			get { return _importance; }
		}

		public string Text
		{
			get { return _text; }
		}

		public Type MessageType
		{
			get { return _type; }
		}
	}
}