using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace Bat.Reset.NotesExperiment;

internal static class Program
{
    public static string DataPath { get; private set; } = "";
    public static bool Offscreen { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        Offscreen=args.Contains("--offscreen");
        // A separate identity and data root: this experiment never opens timer.db.
        var dataArg = Array.IndexOf(args, "--data-dir");
        DataPath = Path.GetFullPath(dataArg >= 0 && args.Length > dataArg + 1
            ? args[dataArg + 1]
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BAT.Reset.NotesExperiment"));
        Directory.CreateDirectory(DataPath);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => File.WriteAllText(Path.Combine(DataPath, "error.log"), e.ExceptionObject.ToString());
        AppBuilder.Configure<ExperimentApp>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
    }
}

internal sealed class ExperimentApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow(new NoteStore(Program.DataPath));
        base.OnFrameworkInitializationCompleted();
    }
}
