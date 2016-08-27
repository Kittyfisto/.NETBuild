using System.Xml;
using Build.DomainModel.MSBuild;

namespace Build.Parser
{
	public static class XmlReaderExtensions
	{
		public static string ReadRequiredAttribute(this XmlReader reader, string attributeName)
		{
			var name = reader.GetAttribute("Name");
			return name;
		}

		public static Condition TryReadCondition(this XmlReader reader)
		{
			var conditionValue = reader.GetAttribute("Condition");
			Condition condition = conditionValue != null ? new Condition(conditionValue) : null;
			return condition;
		}
	}
}