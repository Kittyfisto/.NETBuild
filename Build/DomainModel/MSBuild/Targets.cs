namespace Build.DomainModel.MSBuild
{
	public static class Targets
	{
		public static readonly Target CoreBuild;
		public static readonly Target CopyAppConfigFile;

		static Targets()
		{
			/*CopyAppConfigFile = new Target
				{
					Name = "_CopyAppConfigFile",
					Inputs = "@(AppConfigWithTargetPath)",
					Outputs = "@(AppConfigWithTargetPath->'$(OutDir)%(TargetPath)')",
					Tasks =
						{
							new Copy
								{
									SourceFiles = "@(AppConfigWithTargetPath)",
									DestinationFiles = "@(AppConfigWithTargetPath->'$(OutDir)%(TargetPath)')"
								}
						}
				};*/
		}

		/// <summary>
		/// 
		/// </summary>
		public const string BeforeBuild = "BeforeBuild";

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