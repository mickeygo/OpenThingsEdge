namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 欧姆龙的Fins协议的数据类型。
/// </summary>
public class OmronFinsDataType
{
    /// <summary>
    /// DM Area
    /// </summary>
    public static readonly OmronFinsDataType DM = new(2, 130);

    /// <summary>
    /// CIO Area
    /// </summary>
    public static readonly OmronFinsDataType CIO = new(48, 176);

    /// <summary>
    /// Work Area
    /// </summary>
    public static readonly OmronFinsDataType WR = new(49, 177);

    /// <summary>
    /// Holding Bit Area
    /// </summary>
    public static readonly OmronFinsDataType HR = new(50, 178);

    /// <summary>
    /// Auxiliary Bit Area
    /// </summary>
    public static readonly OmronFinsDataType AR = new(51, 179);

    /// <summary>
    /// TIM Area
    /// </summary>
    public static readonly OmronFinsDataType TIM = new(9, 137);

    /// <summary>
    /// 进行位操作的指令
    /// </summary>
    public byte BitCode { get; private set; }

    /// <summary>
    /// 进行字操作的指令
    /// </summary>
    public byte WordCode { get; private set; }

    /// <summary>
    /// 实例化一个Fins的数据类型
    /// </summary>
    /// <param name="bitCode">进行位操作的指令</param>
    /// <param name="wordCode">进行字操作的指令</param>
    public OmronFinsDataType(byte bitCode, byte wordCode)
    {
        BitCode = bitCode;
        WordCode = wordCode;
    }
}
