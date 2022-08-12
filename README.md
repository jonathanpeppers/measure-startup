# measure-startup

C# console app to launch a process, wait for stdout, report a time.

Based on a [Powershell script for Android][android], but instead runs
a process on your system.

To use this tool to measure the time `dotnet` takes to print `help`,
you could build and run this tool by doing:

```bash
$ git clone https://github.com/jonathanpeppers/measure-startup.git
$ dotnet run -- dotnet help
```

Everything after `--` are arguments passed to `measure-startup`.
I may eventually make this a .NET global tool to make this easier.

## .NET MAUI

To measure a .NET MAUI app, first add somewhere in your app, something
like:

```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    Dispatcher.Dispatch(() => Console.WriteLine("appeared"));
}
```

In an app using `BlazorWebView`, you might consider logging this message
when the web view finishes loading.

On Windows, build the app for `Release` mode, such as:

```bash
$ dotnet publish -f net6.0-windows10.0.19041.0 -c Release -p:PublishReadyToRun=true
```

You could then measure the startup time via:

```bash
$ dotnet run -c Release -- C:\src\YourApp\bin\Release\net6.0-windows10.0.19041.0\win10-x64\publish\YourApp.exe appeared
```

This launches `YourApp.exe` recording the time it takes for `appeared`
to be printed to stdout:

```
0:00:05.4562167
Dropping first run...
0:00:01.3820061
0:00:01.3802031
0:00:01.3769286
0:00:01.3594958
0:00:01.3526491
Average(ms): 1370.2565399999999
Std Err(ms): 5.946923866050407
Std Dev(ms): 13.297726021504564
```

On macOS, build the app for either both architectures:

```xml
<RuntimeIdentifiers>maccatalyst-x64;maccatalyst-arm64</RuntimeIdentifiers>
```

Or build the app for `Release` mode for your machine's architecture, for an M1:

```bash
$ dotnet publish -f net6.0-maccatalyst -c Release -r maccatalyst-arm64
```

You could then measure the startup time via:

```dotnetcli
$ dotnet run -- ~/src/YourApp/bin/Release/net6.0-maccatalyst/maccatalyst-arm64/YourApp.app/Contents/MacOS/YourApp appeared 
```

This launches `YourApp` recording the time it takes for `appeared`
to be printed to stdout:

```dotnetcli
0:00:03.1464144
Dropping first run...
0:00:01.6133342
0:00:01.5960076
0:00:01.5935203
0:00:01.6674307
0:00:01.6061827
Average(ms): 1615.2951
Std Err(ms): 13.511386533624139
Std Dev(ms): 30.212378759458822
```

[android]: https://github.com/jonathanpeppers/maui-profiling/blob/main/scripts/profile.ps1
