# measure-startup

C# console app to launch a process, wait for stdout, report a time.

Based on a [Powershell script for Android][android], but instead runs
a process on your system.

To use this tool to measure the time `dotnet` takes to print `help`,
you could do:

```bash
dotnet run -- dotnet help
```

Everything after `--` are arguments passed to `measure-startup`.

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

On Windows, build the app for `Release` mode, such as:

```bash
dotnet publish -f net6.0-windows10.0.19041.0 -c Release -bl -p:PublishReadyToRun=true
```

You could then measure the startup time via:

```bash
dotnet run -c Release -- C:\src\YourApp\bin\Release\net6.0-windows10.0.19041.0\win10-x64\publish\YourApp.exe appeared
```

This launches `YourApp.exe` recording the time it takes for `appeared`
to be printed:

```
0:00:01.2314455
Dropping first run...
0:00:01.399508
0:00:01.3208399
0:00:01.3836848
0:00:01.3858833
0:00:01.4415977
Average(ms): 1386.3027399999999
Std Err(ms): 19.39629404511595
Std Dev(ms): 43.371431996453644
```

[android]: https://github.com/jonathanpeppers/maui-profiling/blob/main/scripts/profile.ps1
