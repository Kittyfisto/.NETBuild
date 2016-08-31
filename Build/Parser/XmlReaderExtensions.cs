using System.Xml;

namespace Build.Parser
{
	public static class XmlReaderExtensions
	{
		public static string ReadRequiredAttribute(this XmlReader reader, string attributeName)
		{
			string name = reader.GetAttribute("Name");
			return name;
		}

		public static string TryReadCondition(this XmlReader reader)
		{
			string conditionValue = reader.GetAttribute("Condition");
			return conditionValue;
		}
	}
}