using System.IO;
using System.IO.Pipes;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quaply.Data;
using Quaply.Service;

namespace Quaply.Ui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string MutexName =
        "Quaply_SingleInstance_Mutex_2E0AF992-357D-4B7B-87E5-C6DC203AEA56";
    private const string PipeName =
        "Quaply_SingleInstance_Pipe_2E0AF992-357D-4B7B-87E5-C6DC203AEA56";
    private const string ActivateMessage = "activate";

    private readonly IHost? _host;
    private readonly Mutex _mutex;
    private readonly bool _isNewInstance;

    public App()
    {
        // Check the single-instance before building the DI/host
        // to avoid opening SQLite twice.
        _mutex = new Mutex(true, MutexName, out _isNewInstance);

        if (!_isNewInstance)
        {
            NotifyExistingInstance();
            // Do not build the host; exit immediately in OnStartup.
            _host = null;
            return;
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddData();
        builder.Services.AddService();
        builder.Services.AddUi();

        _host = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (!_isNewInstance || _host is null)
        {
            Shutdown();
            return;
        }

        await _host.StartAsync();

        MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        StartPipeServer(mainWindow);

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_isNewInstance && _host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();

        base.OnExit(e);
    }

    private static void NotifyExistingInstance()
    {
        try
        {
            using NamedPipeClientStream client = new(
                ".",
                PipeName,
                PipeDirection.Out
            );

            client.Connect(500);

            using StreamWriter writer = new(client);

            writer.WriteLine(ActivateMessage);
            writer.Flush();
        }
        catch
        {
            // The other instance may not be ready to listen yet; skip it.
        }
    }

    private static void StartPipeServer(MainWindow mainWindow)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    using NamedPipeServerStream server = new(
                        PipeName,
                        PipeDirection.In
                    );

                    await server.WaitForConnectionAsync();

                    using StreamReader reader = new(server);
                    string? message = await reader.ReadLineAsync();

                    if (message == ActivateMessage)
                    {
                        Current.Dispatcher.Invoke(() =>
                            ActivateWindow(mainWindow)
                        );
                    }
                }
                catch
                {
                    // The server encountered an error/closed; loop back
                    // and continue listening.
                }
            }
        });
    }

    private static void ActivateWindow(Window window)
    {
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }
}
