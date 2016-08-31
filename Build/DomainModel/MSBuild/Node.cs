namespace Build.DomainModel.MSBuild
{
	public abstract class Node
		: INode
	{
		private string _condition;

		protected Node(string condition = null)
		{
			_condition = condition;
		}

		public string Condition
		{
			get { return _condition; }
			set { _condition = value; }
		}
	}
}