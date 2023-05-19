namespace ThingsEdge.Application.Dtos;

public sealed class EquipmentCodeInput
{
    /// <summary>
    /// 产线
    /// </summary>
    [NotNull]
    public string? Line { get; set; }

    /// <summary>
    /// 设备代码集合
    /// </summary>
    [NotNull]
    public List<string>? EquipmentCodes { get; set; }
}
