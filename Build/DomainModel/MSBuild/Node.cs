namespace Build.DomainModel.MSBuild
{
	public abstract class Node
		: INode
	{
		private Condition _condition;

		protected Node(Condition condition)
		{
			_condition = condition;
		}

		public Condition Condition
		{
			get { return _condition; }
			set { _condition = value; }
		}
	}
}