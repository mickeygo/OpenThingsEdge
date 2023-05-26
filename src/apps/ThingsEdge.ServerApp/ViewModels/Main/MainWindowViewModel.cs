namespace ThingsEdge.ServerApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavService _navService;
    private readonly AppConfig _appConfig;

    private bool _isInitialized = false;

    [ObservableProperty]
    private string _applicationTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<INavigationControl> _navigationItems = new();

    [ObservableProperty]
    private ObservableCollection<INavigationControl> _navigationFooter = new();

    [ObservableProperty]
    private ObservableCollection<Wpf.Ui.Controls.MenuItem> _trayMenuItems = new();

    public MainWindowViewModel(INavigationService navigationService, 
        INavService navService, 
        IOptions<AppConfig> appConfigOptions)
    {
        _navService = navService;
        _appConfig = appConfigOptions.Value;

        if (!_isInitialized)
        {
            InitializeViewModel();

            _isInitialized = true;
        }
    }

    private void InitializeViewModel()
    {
        ApplicationTitle = _appConfig.Title;

        // 导航设置
        NavigationItems = new ObservableCollection<INavigationControl>(_navService.NavigationItems);

        // foot 导航设置
        NavigationFooter = new ObservableCollection<INavigationControl>(_navService.NavigationFooter);

        TrayMenuItems = new ObservableCollection<Wpf.Ui.Controls.MenuItem>
        {
            new Wpf.Ui.Controls.MenuItem
            {
                Header = "Home",
                Tag = "tray_home",
            }
        };
    }
}
