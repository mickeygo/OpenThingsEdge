namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// 在线数据信息
/// </summary>
public class OnlineData
{
    /// <summary>
    /// equipment model type
    /// </summary>
    public string ModelType { get; set; }

    /// <summary>
    /// software revision
    /// </summary>
    public string SoftVersion { get; set; }

    /// <summary>
    /// 指定类型及其版本号来实例化一个对象
    /// </summary>
    /// <param name="model">类型信息</param>
    /// <param name="version">版本号</param>
    public OnlineData(string model, string version)
    {
        ModelType = model;
        SoftVersion = version;
    }

    /// <summary>
    /// 赋值操作，可以直接赋值 <see cref="OnlineData" /> 数据
    /// </summary>
    /// <param name="value"><see cref="SecsValue" /> 数值</param>
    /// <returns>等值的消息对象</returns>
    public static implicit operator OnlineData(SecsValue value)
    {
        TypeHelper.TypeListCheck(value);
        if (value.Value is SecsValue[] array)
        {
            return new OnlineData(array[0].Value.ToString(), array[1].Value.ToString());
        }
        return null;
    }

    /// <summary>
    /// 也可以赋值给<see cref="SecsValue" /> 数据
    /// </summary>
    /// <param name="value"><see cref="SecsValue" /> 对象</param>
    /// <returns>等值的消息对象</returns>
    public static implicit operator SecsValue(OnlineData value)
    {
        return new SecsValue(new object[2] { value.ModelType, value.SoftVersion });
    }
}
