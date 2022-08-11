
using System.Diagnostics;

string processToRun = "dotnet";
string stdoutText = "asdf";

var psi = new ProcessStartInfo(processToRun)
{
	RedirectStandardError = true,
	RedirectStandardOutput = true,
};
using var p = new Process { StartInfo = psi };

bool exited = false;
var resetEvent = new ManualResetEvent(false);
p.ErrorDataReceived += OnDataReceived;
p.OutputDataReceived += OnDataReceived;
p.Exited += OnExited;

var stopwatch = new Stopwatch();
stopwatch.Start();
p.Start();
p.BeginErrorReadLine();
p.BeginOutputReadLine();

resetEvent.WaitOne();
if (exited)
{
	throw new Exception($"Exited before '{stdoutText}' was printed.");
}
stopwatch.Stop();

Console.WriteLine(stopwatch.Elapsed.ToString());

void OnDataReceived(object sender, DataReceivedEventArgs e)
{
	if (!string.IsNullOrEmpty(e.Data) && e.Data.IndexOf(stdoutText, StringComparison.OrdinalIgnoreCase) != -1)
	{
		resetEvent.Set();
	}
}

void OnExited(object? sender, EventArgs e)
{
	exited = true;
	resetEvent.Set();
}
