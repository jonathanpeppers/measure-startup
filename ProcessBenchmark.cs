using BenchmarkDotNet.Attributes;
using System.Diagnostics;

namespace measure_startup;

public class ProcessBenchmark
{
    public static string FileName { get; set; } = "";

    public static string Arguments { get; set; } = "";

    public static string StdOutText { get; set; } = "";

    readonly ProcessStartInfo psi;

    public ProcessBenchmark()
    {
        if (string.IsNullOrEmpty(FileName))
            throw new ArgumentNullException(nameof(FileName));
        if (string.IsNullOrEmpty(StdOutText))
            throw new ArgumentNullException(nameof(StdOutText));
        if (!File.Exists(FileName))
            throw new FileNotFoundException($"'{FileName}' did not exist!");

        psi = new ProcessStartInfo(FileName, Arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
    }

    [Benchmark]
    public void Run()
    {
        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };

        var resetEvent = new ManualResetEvent(false);
        bool matched = false;

        p.ErrorDataReceived += OnDataReceived;
        p.OutputDataReceived += OnDataReceived;
        p.Exited += OnExited;
        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();

        resetEvent.WaitOne();
        if (!matched)
        {
            throw new Exception($"Exited before '{StdOutText}' was printed.");
        }

        if (!p.HasExited && !p.CloseMainWindow())
        {
            p.Kill();
        }

        void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data) && e.Data.IndexOf(StdOutText, StringComparison.OrdinalIgnoreCase) != -1)
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
}
