namespace ThingsEdge.ServerApp.ViewModels;

public partial class DataViewModel : AbstractObservableNavViewModel
{
    [ObservableProperty]
    private IEnumerable<DataColor> _colors = null!;

    protected override void Initialize()
    {
        var random = new Random();
        var colorCollection = new List<DataColor>();

        for (int i = 0; i < 8192; i++)
            colorCollection.Add(new DataColor
            {
                Color = new SolidColorBrush(Color.FromArgb(
                    (byte)200,
                    (byte)random.Next(0, 250),
                    (byte)random.Next(0, 250),
                    (byte)random.Next(0, 250)))
            });

        Colors = colorCollection;
    }
}
