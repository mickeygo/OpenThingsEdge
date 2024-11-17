namespace ThingsEdge.Communication.Secs.Types;

internal class TypeHelper
{
    /// <summary>
    /// 检查类型 <see cref="T:HslCommunication.Secs.Types.SecsValue" /> 的类型是否为 <see cref="F:HslCommunication.Secs.Types.SecsItemType.List" />，如果不是，就抛出异常
    /// </summary>
    /// <param name="secsItem">等待转换的对象</param>
    /// <exception cref="T:System.InvalidCastException">转换异常</exception>
    public static void TypeListCheck(SecsValue secsItem)
    {
        if (secsItem.ItemType != 0)
        {
            throw new InvalidCastException($"Current type must be List, but now is {secsItem.ItemType} {Environment.NewLine} Source: {secsItem.ToXElement()}");
        }
    }
}
