using System.Text;

namespace Build.DomainModel.MSBuild
{
	public abstract class AbstractMessage
		: Task
	{
		public AbstractMessage()
			: base(null)
		{}

		public override string ToString()
		{
			string token = GetType().Name;
			var builder = new StringBuilder();
			builder.AppendFormat("<{0} ", token);
			builder.AppendFormat("Text=\"{0}\" ", Text);
			if (Condition != null)
			{
				builder.AppendFormat("{0} ", Condition);
			}
			builder.AppendFormat("Importance=\"{0}\" ", Importance);
			builder.Append("/>");
			return builder.ToString();
		}

		public Importance Importance { get; set; }

		public string Text { get; set; }
	}
}