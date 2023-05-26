namespace ThingsEdge.ServerApp.ViewModels;

public partial class DashboardViewModel : AbstractObservableNavViewModel
{
    [ObservableProperty]
    private int _counter = 0;

    [RelayCommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }
}
