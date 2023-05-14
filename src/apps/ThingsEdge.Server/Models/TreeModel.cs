namespace ThingsEdge.Server.Models;

public sealed class TreeModel
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Category { get; set; }

    public List<TreeModel>? Children { get; set; }
}
