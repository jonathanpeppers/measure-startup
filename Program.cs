
using System.CommandLine;
using System.Diagnostics;

if (args.Length == 2)
{
	OnCommand(args[0], "", args[1]);
	return 0;
}

var fileOption = new Option<string>(new[] { "-f", "--file-name" }, "The path to the process to run");
var argsOption = new Option<string>(new[] { "-a", "--args" }, "Any command-line arguments to pass");
var textOption = new Option<string>(new[] { "-m", "--match" }, "The text to match against.");
var command = new RootCommand
{
	fileOption,
	argsOption,
	textOption
};
command.Name = "measure-startup";
command.Description = "Measures startup of a process and reports a time";
command.SetHandler(OnCommand, fileOption, argsOption, textOption);
return command.Invoke(args);

void OnCommand(string fileName, string args, string stdoutText)
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

	Console.WriteLine(stopwatch.Elapsed.ToString());

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
