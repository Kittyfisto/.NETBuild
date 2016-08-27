namespace Build.DomainModel.MSBuild
{
	public static class Targets
	{
		/// <summary>
		/// 
		/// </summary>
		public const string BeforeBuild = "BeforeBuild";

		/// <summary>
		/// 
		/// </summary>
		public const string CoreBuild = "CoreBuild";

		/// <summary>
		/// 
		/// </summary>
		public const string AfterBuild = "AfterBuild";

		/// <summary>
		///     Same as "BeforeBuild;CoreBuild;AfterBuild";
		/// </summary>
		public const string Build = "Build";

		/// <summary>
		/// 
		/// </summary>
		public const string BeforeClean = "BeforeClean";

		/// <summary>
		/// 
		/// </summary>
		public const string CoreClean = "CoreClean";

		/// <summary>
		/// 
		/// </summary>
		public const string AfterClean = "AfterClean";

		/// <summary>
		///     Same as "BeforeClean;CoreClean;AfterClean".
		/// </summary>
		public const string Clean = "Clean";

		/// <summary>
		///     Same as "Clean;Build".
		/// </summary>
		public const string Avenge = "Avenge";
	}
}