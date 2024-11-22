namespace ThingsEdge.Communication.Common.Extensions;

internal static class RandomExtensions
{
    /// <summary>
    /// 本通讯项目的随机数信息。
    /// </summary>
    public static Random Random { get; } = new();

    /// <summary>
    /// 根据指定的字节长度信息，获取到随机的字节信息。
    /// </summary>
    /// <param name="length">字节的长度信息</param>
    /// <returns>原始字节数组</returns>
    public static byte[] GetBytes(int length)
    {
        var array = new byte[length];
        Random.NextBytes(array);
        return array;
    }
}
