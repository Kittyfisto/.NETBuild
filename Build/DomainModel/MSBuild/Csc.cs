namespace Build.DomainModel.MSBuild
{
	public sealed class Csc
		: Task
	{
		public string HighEntropyVA { get; set; }
		public string AllowUnsafeBlocks { get; set; }

		public string BaseAddress { get; set; }

		public string CheckForOverflowUnderflow { get; set; }

		public string DebugType { get; set; }

		public string DefineConstants { get; set; }

		public string DelaySign { get; set; }

		public string DisabledWarnings { get; set; }

		public string EmitDebugInformation { get; set; }

		public string ErrorEndLocation { get; set; }

		public string ErrorReport { get; set; }

		public string FileAlignment { get; set; }

		public string MainEntryPoint { get; set; }

		public string ModuleAssemblyName { get; set; }

		public string NoConfig { get; set; }

		public string NoLogo { get; set; }

		public string Optimize { get; set; }

		public string OutputAssembly { get; set; }

		public string PdbFile { get; set; }

		public string Platform { get; set; }

		public string Prefer32Bit { get; set; }

		public string PreferredUILang { get; set; }

		public string TargetType { get; set; }

		public string TreatWarningsAsErrors { get; set; }

		public string Utf8Output { get; set; }

		public string WarningLevel { get; set; }

		public string WarningsAsErrors { get; set; }

		public string Win32Icon { get; set; }

		public string References { get; set; }

		public string Resources { get; set; }

		public string Sources { get; set; }

		public string SubsystemVersion { get; set; }
	}
}