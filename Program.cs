
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using measure_startup;
using System.CommandLine;

// Uncommment to test the time "dotnet" takes to print "help"
//if (args.Length == 0)
//{
//	OnCommand("dotnet", "", "help");
//	return 0;
//}

if (args.Length == 2)
{
	OnCommand(args[0], "", args[1]);
	return 0;
}

var fileOption = new Option<string>(new[] { "-f", "--file-name" },
	"The path to the process to run. (required)");
var argsOption = new Option<string>(new[] { "-a", "--args" },
	"Any command-line arguments to pass.");
var textOption = new Option<string>(new[] { "-m", "--match" },
	"The text to match against. (required)");
var iterationsOption = new Option<int>(new[] { "-i", "--iterations" }, () => 5,
	"Number of iterations. (defaults to 5)");

var command = new RootCommand
{
	fileOption,
	argsOption,
	textOption,
};
command.Name = "measure-startup";
command.Description = "Measures startup of a process and reports a time.";
command.SetHandler(OnCommand, fileOption, argsOption, textOption, iterationsOption);
return command.Invoke(args);

void OnCommand(string fileName, string args, string stdoutText, int iterations = 5)
{
	ProcessBenchmark.FileName = fileName;
	ProcessBenchmark.Arguments = args;
	ProcessBenchmark.StdOutText = stdoutText;


	var job = JobMode<Job>.Default.WithToolchain(InProcessNoEmitToolchain.Instance)
		.WithWarmupCount(1)
		.WithIterationCount(iterations);

	var config =
		new ManualConfig
		{
			Options =
#if DEBUG
				ConfigOptions.KeepBenchmarkFiles | ConfigOptions.DisableOptimizationsValidator |
#endif
				ConfigOptions.DisableLogFile
		}
		.AddLogger(ConsoleLogger.Default)
		.AddColumnProvider(DefaultColumnProviders.Instance)
		.AddJob(job);

	var summary = BenchmarkRunner.Run<ProcessBenchmark>(config);
}
