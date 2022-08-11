
using System.Diagnostics;

string processToRun = args[0];
string stdoutText = args[1];

var psi = new ProcessStartInfo(processToRun)
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
