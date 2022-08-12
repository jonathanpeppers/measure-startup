
using System.CommandLine;
using System.Diagnostics;

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
	iterationsOption,
};
command.Name = "measure-startup";
command.Description = "Measures startup of a process and reports a time.";
command.SetHandler(OnCommand, fileOption, argsOption, textOption, iterationsOption);
return command.Invoke(args);

void OnCommand(string fileName, string args, string stdoutText, int iterations = 5)
{
	var times = new List<TimeSpan>();

	// Do an extra iteration, first time is ignored
	for (int i = 0; i <= iterations; i++)
	{
		var psi = new ProcessStartInfo(fileName, args)
		{
			RedirectStandardError = true,
			RedirectStandardOutput = true,
		};
		using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };

		var resetEvent = new ManualResetEvent(false);
		bool matched = false;

		p.ErrorDataReceived += OnDataReceived;
		p.OutputDataReceived += OnDataReceived;
		p.Exited += OnExited;

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		p.Start();
		p.BeginErrorReadLine();
		p.BeginOutputReadLine();

		resetEvent.WaitOne();
		if (!matched)
		{
			throw new Exception($"Exited before '{stdoutText}' was printed.");
		}
		stopwatch.Stop();
		Console.WriteLine($"{stopwatch.Elapsed:g}");
		if (i == 0)
		{
			Console.WriteLine("Dropping first run...");
		}
		else
		{
			times.Add(stopwatch.Elapsed);
		}

		void OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data) && e.Data.IndexOf(stdoutText, StringComparison.OrdinalIgnoreCase) != -1)
			{
				matched = true;
				resetEvent.Set();
			}
		}

		void OnExited(object? sender, EventArgs e)
		{
			// Sleep because this fires before OnDataReceived
			Thread.Sleep(1000);
			resetEvent.Set();
		}
	}

	double sum = times.Sum(t => t.TotalMilliseconds);
	double mean = sum / times.Count;
	double variance = 0;
	if (times.Count != 1)
	{
		foreach (var time in times)
		{
			variance += (time.TotalMilliseconds - mean) * (time.TotalMilliseconds - mean) / (times.Count - 1);
		}
	}
	double stddev = Math.Sqrt(variance);
	double stderr = stddev / Math.Sqrt(times.Count);
	Console.WriteLine($"Average(ms): {mean}");
	Console.WriteLine($"Std Err(ms): {stderr}");
	Console.WriteLine($"Std Dev(ms): {stddev}");
}
