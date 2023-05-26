namespace ThingsEdge.ServerApp.Services.Impl;

internal sealed class NavService : INavService
{
    private readonly Lazy<(IEnumerable<INavigationControl> items, IEnumerable<INavigationControl> footer)> _nav = new(BuildNavNavCollection);

    public IEnumerable<INavigationControl> NavigationItems => _nav.Value.items;

    public IEnumerable<INavigationControl> NavigationFooter => _nav.Value.footer;

    private static (IEnumerable<INavigationControl> items, IEnumerable<INavigationControl> footer) BuildNavNavCollection()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Assets", "nav", "nav.json");
        var navCollection = JsonSerializer.Deserialize<NavCollection>(File.ReadAllText(file))
            ?? throw new Exception("Please configure the navigation first!");
        
        var items = navCollection.Items.Select(s => new NavigationItem
        {
            Content = s.Content,
            PageTag = s.PageTag,
            Icon = s.Icon,
            PageType = Type.GetType($"ThingsEdge.ServerApp.Views.Pages.{s.PageType}, ThingsEdge.ServerApp", true, true),
        });

        var footer = navCollection.Footer.Select(s => new NavigationItem
        {
            Content = s.Content,
            PageTag = s.PageTag,
            Icon = s.Icon,
            PageType = Type.GetType($"ThingsEdge.ServerApp.Views.Pages.{s.PageType}, ThingsEdge.ServerApp", true, true),
        });

        return (items, footer);
    }
}
