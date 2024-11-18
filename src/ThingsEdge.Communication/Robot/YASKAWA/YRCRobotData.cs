using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Robot.YASKAWA;

/// <summary>
/// 安川机器人的数据信息，其中 Re只在YRC100中有效，没有外部轴的系统， 7-12外部轴的值设定为「0」
/// </summary>
public class YRCRobotData
{
    /// <summary>
    /// 动作速度（ 0.01 ～ 100.0％）
    /// </summary>
    /// <remarks>
    /// 在读取时没有任何的含义，仅在写入数据是有效，默认为 100.0%
    /// </remarks>
    public string SpeedPercent { get; set; }

    /// <summary>
    /// 参考系，0:基座坐标，1:机器人坐标，2-65分别表示用户坐标1-64
    /// </summary>
    public int Frame { get; set; }

    /// <summary>
    /// X 坐标值（ 单位mm、小数点第 3 位有效）
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y 坐标值（单位mm、小数点第 3 位有效）
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Z 坐标值（单位 mm、小数点第 3 位有效）
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// 手腕姿勢 Rx 坐标值（单位 °、小数点第 4 位有效）
    /// </summary>
    public float Rx { get; set; }

    /// <summary>
    /// 手腕姿勢 Ry 坐标值（单位 °、 小数点第 4 位有效）
    /// </summary>
    public float Ry { get; set; }

    /// <summary>
    /// 手腕姿勢 Rz 坐标值（单位 °、 小数点第 4 位有效）
    /// </summary>
    public float Rz { get; set; }

    /// <summary>
    /// 肘角姿势 Re，仅在七轴机器人的情况下有效
    /// </summary>
    public float Re { get; set; }

    /// <summary>
    /// 形态数据，各个索引含义为 [0] 0:F lip,1:N o Flip [1] 0:上方肘，1:下方肘 [2] 0:正面,1:背面 [3] 0:R＜180, 1:R≥180 [4] 0:T＜180, 1:T≥180 [5] 0:S＜180, 1:S≥180
    /// </summary>
    public bool[] Status { get; set; }

    /// <summary>
    /// 工具编号（ 0 ～ 63）
    /// </summary>
    public int ToolNumber { get; set; }

    /// <summary>
    /// 第 7 轴脉冲数（ 行走轴时、 单位mm）
    /// </summary>
    public int Axis7PulseNumber { get; set; }

    /// <summary>
    /// 第 8 轴脉冲数（ 行走轴时、 单位mm）
    /// </summary>
    public int Axis8PulseNumber { get; set; }

    /// <summary>
    /// 第 9 轴脉冲数（行走轴时、 单位mm）
    /// </summary>
    public int Axis9PulseNumber { get; set; }

    /// <summary>
    /// 第 10 轴脉冲数
    /// </summary>
    public int Axis10PulseNumber { get; set; }

    /// <summary>
    /// 第 11 轴脉冲数
    /// </summary>
    public int Axis11PulseNumber { get; set; }

    /// <summary>
    /// 第 12 轴脉冲数
    /// </summary>
    public int Axis12PulseNumber { get; set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public YRCRobotData()
    {
        SpeedPercent = "100.0%";
        Status = new bool[6];
    }

    /// <summary>
    /// 指定类型及字符串数据信息来实例化一个对象
    /// </summary>
    /// <param name="type">类型信息</param>
    /// <param name="value">字符串数据</param>
    public YRCRobotData(YRCType type, string value)
        : this()
    {
        Parse(type, value);
    }

    /// <summary>
    /// 将数据转换为写入命令的字符换，需要指定是否七轴机器人的信息
    /// </summary>
    /// <param name="type">机器人的型号信息</param>
    /// <returns>写入的数据信息</returns>
    public string ToWriteString(YRCType type)
    {
        if (type == YRCType.YRC100)
        {
            return $"{SpeedPercent},{Frame},{X},{Y},{Z},{Rx},{Ry},{Rz},{Re},{SoftBasic.BoolArrayToByte(Status)[0]}," + $"{ToolNumber},{Axis7PulseNumber},{Axis8PulseNumber},{Axis9PulseNumber},{Axis10PulseNumber},{Axis11PulseNumber},{Axis12PulseNumber}";
        }
        return $"{SpeedPercent},{Frame},{X},{Y},{Z},{Rx},{Ry},{Rz},{SoftBasic.BoolArrayToByte(Status)[0]}," + $"{ToolNumber},{Axis7PulseNumber},{Axis8PulseNumber},{Axis9PulseNumber},{Axis10PulseNumber},{Axis11PulseNumber},{Axis12PulseNumber}";
    }

    /// <summary>
    /// 从实际机器人读取到的数据解析出真实的机器人信息。
    /// </summary>
    /// <param name="type">机器人类型</param>
    /// <param name="value">值</param>
    public void Parse(YRCType type, string value)
    {
        var array = value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var num = 0;
        X = float.Parse(array[num++]);
        Y = float.Parse(array[num++]);
        Z = float.Parse(array[num++]);
        Rx = float.Parse(array[num++]);
        Ry = float.Parse(array[num++]);
        Rz = float.Parse(array[num++]);
        if (type == YRCType.YRC100)
        {
            Re = float.Parse(array[num++]);
        }
        Status = new byte[1] { byte.Parse(array[num++]) }.ToBoolArray().SelectBegin(6);
        ToolNumber = int.Parse(array[num++]);
        if (array.Length > num)
        {
            Axis7PulseNumber = int.Parse(array[num++]);
            Axis8PulseNumber = int.Parse(array[num++]);
            Axis9PulseNumber = int.Parse(array[num++]);
            Axis10PulseNumber = int.Parse(array[num++]);
            Axis11PulseNumber = int.Parse(array[num++]);
            Axis12PulseNumber = int.Parse(array[num++]);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{X},{Y},{Z},{Rx},{Ry},{Rz}]";
    }
}
