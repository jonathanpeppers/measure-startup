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

To measure a .NET MAUI app, first add a subscription to your main
`Page`'s `Loaded` event:

```csharp
Loaded += (sender, e) => Dispatcher.Dispatch(() => Console.WriteLine("loaded"));
```

`Dispatcher` is used to give the app one pass for rendering/layout. In
an app using `BlazorWebView`, you might consider logging this message
when the web view finishes loading.

On Windows, build the app for `Release` mode, such as:

```bash
$ dotnet publish -f net6.0-windows10.0.19041.0 -c Release -p:PublishReadyToRun=true
```

You could then measure the startup time via:

```bash
$ dotnet run -c Release -- C:\src\YourApp\bin\Release\net6.0-windows10.0.19041.0\win10-x64\publish\YourApp.exe loaded
```

This launches `YourApp.exe` recording the time it takes for `loaded`
to be printed to stdout:

```
0:00:07.0961628
Dropping first run...
0:00:01.4743315
0:00:01.4700848
0:00:01.4834235
0:00:01.4752893
0:00:01.4695317
Average(ms): 1474.53216
Std Err(ms): 2.4945246400065977
Std Dev(ms): 5.577926666602944
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
$ dotnet run -- ~/src/YourApp/bin/Release/net6.0-maccatalyst/maccatalyst-arm64/YourApp.app/Contents/MacOS/YourApp loaded 
```

This launches `YourApp` recording the time it takes for `loaded`
to be printed to stdout:

```dotnetcli
0:00:03.3880364
Dropping first run...
0:00:01.8178456
0:00:01.7757709
0:00:01.7641941
0:00:01.836607
0:00:01.7377252
Average(ms): 1786.4285599999998
Std Err(ms): 18.006777122311476
Std Dev(ms): 40.26437770117651
```

[android]: https://github.com/jonathanpeppers/maui-profiling/blob/main/scripts/profile.ps1
