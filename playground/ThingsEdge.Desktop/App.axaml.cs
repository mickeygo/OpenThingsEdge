using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Hosting;
using Serilog;
using ThingsEdge.Desktop.ViewModels;
using ThingsEdge.Desktop.Views;

namespace ThingsEdge.Desktop;

public partial class App : Application
{
    private Mutex? _mutex;

    private readonly IHost _host = Host.CreateDefaultBuilder()
        .UseSerilog((hostingContext, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
        ).Build();

    /// <summary>
    /// 获取当前的 Application 应用程序实例。
    /// </summary>
    public static new App? Current => Application.Current as App;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 只允许开启一个应用（若是相同程序不同应用，需修改 name）
        _mutex = new(true, "ThingsEdge.Desktop", out var createdNew);
        if (!createdNew)
        {
            Log.Information("应用程序已启动，不能同时开启多个。");
            Environment.Exit(0);
            return;
        }

        Log.Information("应用程序启动");

        _host.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 中止应用。
    /// </summary>
    public async Task ShutdownAsync()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0

        Log.Error(e.Exception, "DispatcherUnhandledException");
    }
}
