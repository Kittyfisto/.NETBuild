namespace Build.DomainModel.MSBuild
{
	public interface INode
	{
		/// <summary>
		///     The condition that is to be evaluated in order to find out if this node
		///     applies, or not.
		/// </summary>
		string Condition { get; }
	}
}