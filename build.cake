var target		= Argument("target", "Default");
var slnDir		= System.IO.Directory.GetCurrentDirectory();
var projDir		= System.IO.Path.Combine(slnDir, "Benchmark.App");

Task("Create-Benchmark-Files")
	.Does(() => {
		Random rng = new Random();
		var smallDummy = System.IO.Path.Combine(projDir, "smalldummy");
		var largeDummy = System.IO.Path.Combine(projDir, "largedummy");

		var smallSizeInMB = 2;
		var largeSizeInMB = 100;

		byte[] data = new byte[smallSizeInMB * 1024 * 1024];
		rng.NextBytes(data);
		System.IO.File.WriteAllBytes(smallDummy, data);

		data = new byte[largeSizeInMB * 1024 * 1024];
		rng.NextBytes(data);
		System.IO.File.WriteAllBytes(largeDummy, data);
	});

Task("Restore")
	.Does(() =>
	{
		DotNetCoreRestore();	  
	});

Task("Build")
	.IsDependentOn("Restore")
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