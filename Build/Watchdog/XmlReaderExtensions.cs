using System.Xml;
using Build.DomainModel.MSBuild;

namespace Build.Watchdog
{
	public static class XmlReaderExtensions
	{
		public static Condition TryReadCondition(this XmlReader reader)
		{
			var conditionValue = reader.GetAttribute("Condition");
			Condition condition = conditionValue != null ? new Condition(conditionValue) : null;
			return condition;
		}
	}
}