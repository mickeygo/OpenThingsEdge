namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// 变量名称类对象
/// </summary>
public class VariableName
{
    /// <summary>
    /// 变量的ID信息
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// 变量的名称信息
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 变量的单位信息
    /// </summary>
    public string Units { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// 赋值操作，可以直接赋值 <see cref="T:HslCommunication.Secs.Types.OnlineData" /> 数据
    /// </summary>
    /// <param name="value"><see cref="T:HslCommunication.Secs.Types.SecsValue" /> 数值</param>
    /// <returns>等值的消息对象</returns>
    public static implicit operator VariableName(SecsValue value)
    {
        TypeHelper.TypeListCheck(value);
        if (value.Value is SecsValue[] array)
        {
            var variableName = new VariableName();
            variableName.ID = Convert.ToInt64(array[0].Value);
            variableName.Name = array[1].Value as string;
            variableName.Units = array[2].Value as string;
            return variableName;
        }
        return null;
    }

    /// <summary>
    /// 也可以赋值给<see cref="T:HslCommunication.Secs.Types.SecsValue" /> 数据
    /// </summary>
    /// <param name="value"><see cref="T:HslCommunication.Secs.Types.SecsValue" /> 对象</param>
    /// <returns>等值的消息对象</returns>
    public static implicit operator SecsValue(VariableName value)
    {
        return new SecsValue(new object[3] { value.ID, value.Name, value.Units });
    }
}
