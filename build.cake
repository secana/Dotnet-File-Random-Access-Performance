var target		= Argument("target", "Default");
var slnDir		= System.IO.Directory.GetCurrentDirectory();
var projDir		= System.IO.Path.Combine(slnDir, "Benchmark.App");

Task("Build")
	.Does(() =>
	{
		var solution = GetFiles("./*.sln").ElementAt(0);
		Information("Build solution: {0}", solution);

		var settings = new DotNetCoreBuildSettings
		{
			Configuration = "Release"
		};

		DotNetCoreBuild(solution.FullPath, settings);
	});

Task("Run-Benchmark")
	.IsDependentOn("Build")
	.Does(() => {
		var settings = new DotNetCoreRunSettings
		{
			Configuration = "Release"
		};

		DotNetCoreRun("./Benchmark.App", "--args", settings);
	});

Task("Default")
	.IsDependentOn("Run-Benchmark");

RunTarget(target);