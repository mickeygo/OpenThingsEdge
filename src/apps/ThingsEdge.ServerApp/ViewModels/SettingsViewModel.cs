namespace ThingsEdge.ServerApp.ViewModels;

public partial class SettingsViewModel : AbstractObservableNavViewModel
{
    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private ThemeType _currentTheme = ThemeType.Unknown;

    protected override void Initialize()
    {
        CurrentTheme = Theme.GetAppTheme();
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    }

    [RelayCommand]
    private void OnChangeTheme(string parameter)
    {
        switch (parameter)
        {
            case "theme_light":
                if (CurrentTheme == ThemeType.Light)
                    break;

                Theme.Apply(ThemeType.Light);
                CurrentTheme = ThemeType.Light;

                break;

            default:
                if (CurrentTheme == ThemeType.Dark)
                    break;

                Theme.Apply(ThemeType.Dark);
                CurrentTheme = ThemeType.Dark;

                break;
        }
    }
}
