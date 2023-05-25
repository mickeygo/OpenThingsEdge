namespace ThingsEdge.ServerApp.ViewModels;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private int _counter = 0;

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }
}
