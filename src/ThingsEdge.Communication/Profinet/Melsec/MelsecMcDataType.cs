namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC的数据类型，此处包含了几个常用的类型<br />
/// Data types of Mitsubishi PLC, here contains several commonly used types
/// </summary>
public class MelsecMcDataType
{
    /// <summary>
    /// X输入继电器
    /// </summary>
    public static readonly MelsecMcDataType X = new MelsecMcDataType(156, 1, "X*", 16);

    /// <summary>
    /// Y输出继电器
    /// </summary>
    public static readonly MelsecMcDataType Y = new MelsecMcDataType(157, 1, "Y*", 16);

    /// <summary>
    /// M内部继电器
    /// </summary>
    public static readonly MelsecMcDataType M = new MelsecMcDataType(144, 1, "M*", 10);

    /// <summary>
    /// SM特殊继电器
    /// </summary>
    public static readonly MelsecMcDataType SM = new MelsecMcDataType(145, 1, "SM", 10);

    /// <summary>
    /// S步进继电器
    /// </summary>
    public static readonly MelsecMcDataType S = new MelsecMcDataType(152, 1, "S*", 10);

    /// <summary>
    /// L锁存继电器
    /// </summary>
    public static readonly MelsecMcDataType L = new MelsecMcDataType(146, 1, "L*", 10);

    /// <summary>
    /// F报警器
    /// </summary>
    public static readonly MelsecMcDataType F = new MelsecMcDataType(147, 1, "F*", 10);

    /// <summary>
    /// V边沿继电器
    /// </summary>
    public static readonly MelsecMcDataType V = new MelsecMcDataType(148, 1, "V*", 10);

    /// <summary>
    /// B链接继电器
    /// </summary>
    public static readonly MelsecMcDataType B = new MelsecMcDataType(160, 1, "B*", 16);

    /// <summary>
    /// SB特殊链接继电器
    /// </summary>
    public static readonly MelsecMcDataType SB = new MelsecMcDataType(161, 1, "SB", 16);

    /// <summary>
    /// DX直接访问输入
    /// </summary>
    public static readonly MelsecMcDataType DX = new MelsecMcDataType(162, 1, "DX", 16);

    /// <summary>
    /// DY直接访问输出
    /// </summary>
    public static readonly MelsecMcDataType DY = new MelsecMcDataType(163, 1, "DY", 16);

    /// <summary>
    /// D数据寄存器
    /// </summary>
    public static readonly MelsecMcDataType D = new MelsecMcDataType(168, 0, "D*", 10);

    /// <summary>
    /// 特殊链接存储器
    /// </summary>
    public static readonly MelsecMcDataType SD = new MelsecMcDataType(169, 0, "SD", 10);

    /// <summary>
    /// W链接寄存器
    /// </summary>
    public static readonly MelsecMcDataType W = new MelsecMcDataType(180, 0, "W*", 16);

    /// <summary>
    /// SW特殊链接寄存器
    /// </summary>
    public static readonly MelsecMcDataType SW = new MelsecMcDataType(181, 0, "SW", 16);

    /// <summary>
    /// R文件寄存器
    /// </summary>
    public static readonly MelsecMcDataType R = new MelsecMcDataType(175, 0, "R*", 10);

    /// <summary>
    /// 变址寄存器
    /// </summary>
    public static readonly MelsecMcDataType Z = new MelsecMcDataType(204, 0, "Z*", 10);

    /// <summary>
    /// 文件寄存器ZR区
    /// </summary>
    public static readonly MelsecMcDataType ZR = new MelsecMcDataType(176, 0, "ZR", 10);

    /// <summary>
    /// 定时器的当前值
    /// </summary>
    public static readonly MelsecMcDataType TN = new MelsecMcDataType(194, 0, "TN", 10);

    /// <summary>
    /// 定时器的触点
    /// </summary>
    public static readonly MelsecMcDataType TS = new MelsecMcDataType(193, 1, "TS", 10);

    /// <summary>
    /// 定时器的线圈
    /// </summary>
    public static readonly MelsecMcDataType TC = new MelsecMcDataType(192, 1, "TC", 10);

    /// <summary>
    /// 累计定时器的触点
    /// </summary>
    public static readonly MelsecMcDataType SS = new MelsecMcDataType(199, 1, "SS", 10);

    /// <summary>
    /// 累计定时器的线圈
    /// </summary>
    public static readonly MelsecMcDataType SC = new MelsecMcDataType(198, 1, "SC", 10);

    /// <summary>
    /// 累计定时器的当前值
    /// </summary>
    public static readonly MelsecMcDataType SN = new MelsecMcDataType(200, 0, "SN", 10);

    /// <summary>
    /// 计数器的当前值
    /// </summary>
    public static readonly MelsecMcDataType CN = new MelsecMcDataType(197, 0, "CN", 10);

    /// <summary>
    /// 计数器的触点
    /// </summary>
    public static readonly MelsecMcDataType CS = new MelsecMcDataType(196, 1, "CS", 10);

    /// <summary>
    /// 计数器的线圈
    /// </summary>
    public static readonly MelsecMcDataType CC = new MelsecMcDataType(195, 1, "CC", 10);

    /// <summary>
    /// X输入继电器
    /// </summary>
    public static readonly MelsecMcDataType R_X = new MelsecMcDataType(156, 1, "X***", 16);

    /// <summary>
    /// Y输入继电器
    /// </summary>
    public static readonly MelsecMcDataType R_Y = new MelsecMcDataType(157, 1, "Y***", 16);

    /// <summary>
    /// M内部继电器
    /// </summary>
    public static readonly MelsecMcDataType R_M = new MelsecMcDataType(144, 1, "M***", 10);

    /// <summary>
    /// 特殊继电器
    /// </summary>
    public static readonly MelsecMcDataType R_SM = new MelsecMcDataType(145, 1, "SM**", 10);

    /// <summary>
    /// 锁存继电器
    /// </summary>
    public static readonly MelsecMcDataType R_L = new MelsecMcDataType(146, 1, "L***", 10);

    /// <summary>
    /// 报警器
    /// </summary>
    public static readonly MelsecMcDataType R_F = new MelsecMcDataType(147, 1, "F***", 10);

    /// <summary>
    /// 变址继电器
    /// </summary>
    public static readonly MelsecMcDataType R_V = new MelsecMcDataType(148, 1, "V***", 10);

    /// <summary>
    /// S步进继电器
    /// </summary>
    public static readonly MelsecMcDataType R_S = new MelsecMcDataType(152, 1, "S***", 10);

    /// <summary>
    /// 链接继电器
    /// </summary>
    public static readonly MelsecMcDataType R_B = new MelsecMcDataType(160, 1, "B***", 16);

    /// <summary>
    /// 特殊链接继电器
    /// </summary>
    public static readonly MelsecMcDataType R_SB = new MelsecMcDataType(161, 1, "SB**", 16);

    /// <summary>
    /// 直接访问输入继电器
    /// </summary>
    public static readonly MelsecMcDataType R_DX = new MelsecMcDataType(162, 1, "DX**", 16);

    /// <summary>
    /// 直接访问输出继电器
    /// </summary>
    public static readonly MelsecMcDataType R_DY = new MelsecMcDataType(163, 1, "DY**", 16);

    /// <summary>
    /// 数据寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_D = new MelsecMcDataType(168, 0, "D***", 10);

    /// <summary>
    /// 特殊数据寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_SD = new MelsecMcDataType(169, 0, "SD**", 10);

    /// <summary>
    /// 链接寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_W = new MelsecMcDataType(180, 0, "W***", 16);

    /// <summary>
    /// 特殊链接寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_SW = new MelsecMcDataType(181, 0, "SW**", 16);

    /// <summary>
    /// 文件寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_R = new MelsecMcDataType(175, 0, "R***", 10);

    /// <summary>
    /// 变址寄存器
    /// </summary>
    public static readonly MelsecMcDataType R_Z = new MelsecMcDataType(204, 0, "Z***", 10);

    /// <summary>
    /// 长累计定时器触点
    /// </summary>
    public static readonly MelsecMcDataType R_LSTS = new MelsecMcDataType(89, 1, "LSTS", 10);

    /// <summary>
    /// 长累计定时器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_LSTC = new MelsecMcDataType(88, 1, "LSTC", 10);

    /// <summary>
    /// 长累计定时器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_LSTN = new MelsecMcDataType(90, 0, "LSTN", 10);

    /// <summary>
    /// 累计定时器触点
    /// </summary>
    public static readonly MelsecMcDataType R_STS = new MelsecMcDataType(199, 1, "STS*", 10);

    /// <summary>
    /// 累计定时器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_STC = new MelsecMcDataType(198, 1, "STC*", 10);

    /// <summary>
    /// 累计定时器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_STN = new MelsecMcDataType(200, 0, "STN*", 10);

    /// <summary>
    /// 长定时器触点
    /// </summary>
    public static readonly MelsecMcDataType R_LTS = new MelsecMcDataType(81, 1, "LTS*", 10);

    /// <summary>
    /// 长定时器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_LTC = new MelsecMcDataType(80, 1, "LTC*", 10);

    /// <summary>
    /// 长定时器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_LTN = new MelsecMcDataType(82, 0, "LTN*", 10);

    /// <summary>
    /// 定时器触点
    /// </summary>
    public static readonly MelsecMcDataType R_TS = new MelsecMcDataType(193, 1, "TS**", 10);

    /// <summary>
    /// 定时器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_TC = new MelsecMcDataType(192, 1, "TC**", 10);

    /// <summary>
    /// 定时器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_TN = new MelsecMcDataType(194, 0, "TN**", 10);

    /// <summary>
    /// 长计数器触点
    /// </summary>
    public static readonly MelsecMcDataType R_LCS = new MelsecMcDataType(85, 1, "LCS*", 10);

    /// <summary>
    /// 长计数器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_LCC = new MelsecMcDataType(84, 1, "LCC*", 10);

    /// <summary>
    /// 长计数器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_LCN = new MelsecMcDataType(86, 0, "LCN*", 10);

    /// <summary>
    /// 计数器触点
    /// </summary>
    public static readonly MelsecMcDataType R_CS = new MelsecMcDataType(196, 1, "CS**", 10);

    /// <summary>
    /// 计数器线圈
    /// </summary>
    public static readonly MelsecMcDataType R_CC = new MelsecMcDataType(195, 1, "CC**", 10);

    /// <summary>
    /// 计数器当前值
    /// </summary>
    public static readonly MelsecMcDataType R_CN = new MelsecMcDataType(197, 0, "CN**", 10);

    /// <summary>
    /// X输入继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_X = new MelsecMcDataType(156, 1, "X*", 16);

    /// <summary>
    /// Y输出继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_Y = new MelsecMcDataType(157, 1, "Y*", 16);

    /// <summary>
    /// 链接继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_B = new MelsecMcDataType(160, 1, "B*", 16);

    /// <summary>
    /// 内部辅助继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_M = new MelsecMcDataType(144, 1, "M*", 10);

    /// <summary>
    /// 锁存继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_L = new MelsecMcDataType(146, 1, "L*", 10);

    /// <summary>
    /// 控制继电器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_SM = new MelsecMcDataType(145, 1, "SM", 10);

    /// <summary>
    /// 控制存储器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_SD = new MelsecMcDataType(169, 0, "SD", 10);

    /// <summary>
    /// 数据存储器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_D = new MelsecMcDataType(168, 0, "D*", 10);

    /// <summary>
    /// 文件寄存器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_R = new MelsecMcDataType(175, 0, "R*", 10);

    /// <summary>
    /// 文件寄存器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_ZR = new MelsecMcDataType(176, 0, "ZR", 16);

    /// <summary>
    /// 链路寄存器
    /// </summary>
    public static readonly MelsecMcDataType Keyence_W = new MelsecMcDataType(180, 0, "W*", 16);

    /// <summary>
    /// 计时器（当前值）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_TN = new MelsecMcDataType(194, 0, "TN", 10);

    /// <summary>
    /// 计时器（接点）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_TS = new MelsecMcDataType(193, 1, "TS", 10);

    /// <summary>
    /// 计时器（线圈）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_TC = new MelsecMcDataType(192, 1, "TC", 10);

    /// <summary>
    /// 计数器（当前值）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_CN = new MelsecMcDataType(197, 0, "CN", 10);

    /// <summary>
    /// 计数器（接点）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_CS = new MelsecMcDataType(196, 1, "CS", 10);

    /// <summary>
    /// 计数器（线圈）
    /// </summary>
    public static readonly MelsecMcDataType Keyence_CC = new MelsecMcDataType(195, 1, "CC", 10);

    /// <summary>
    /// 输入继电器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_X = new MelsecMcDataType(156, 1, "X*", 10);

    /// <summary>
    /// 输出继电器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_Y = new MelsecMcDataType(157, 1, "Y*", 10);

    /// <summary>
    /// 链接继电器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_L = new MelsecMcDataType(160, 1, "L*", 10);

    /// <summary>
    /// 内部继电器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_R = new MelsecMcDataType(144, 1, "R*", 10);

    /// <summary>
    /// 数据存储器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_DT = new MelsecMcDataType(168, 0, "D*", 10);

    /// <summary>
    /// 链接存储器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_LD = new MelsecMcDataType(180, 0, "W*", 10);

    /// <summary>
    /// 计时器（当前值）
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_TN = new MelsecMcDataType(194, 0, "TN", 10);

    /// <summary>
    /// 计时器（接点）
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_TS = new MelsecMcDataType(193, 1, "TS", 10);

    /// <summary>
    /// 计数器（当前值）
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_CN = new MelsecMcDataType(197, 0, "CN", 10);

    /// <summary>
    /// 计数器（接点）
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_CS = new MelsecMcDataType(196, 1, "CS", 10);

    /// <summary>
    /// 特殊链接继电器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_SM = new MelsecMcDataType(145, 1, "SM", 10);

    /// <summary>
    /// 特殊链接存储器
    /// </summary>
    public static readonly MelsecMcDataType Panasonic_SD = new MelsecMcDataType(169, 0, "SD", 10);

    /// <summary>
    /// 类型的代号值
    /// </summary>
    public ushort DataCode { get; private set; } = 0;


    /// <summary>
    /// 数据的类型，0代表按字，1代表按位
    /// </summary>
    public byte DataType { get; private set; } = 0;


    /// <summary>
    /// 当以ASCII格式通讯时的类型描述
    /// </summary>
    public string AsciiCode { get; private set; }

    /// <summary>
    /// 指示地址是10进制，还是16进制的
    /// </summary>
    public int FromBase { get; private set; }

    /// <summary>
    /// 实例化一个三菱数据类型对象，如果您清楚类型代号，可以根据值进行扩展<br />
    /// Instantiate a Mitsubishi data type object, if you know the type code, you can expand according to the value
    /// </summary>
    /// <param name="code">数据类型的代号</param>
    /// <param name="type">0或1，默认为0，0代表按字，1代表按位</param>
    /// <param name="asciiCode">ASCII格式的类型信息</param>
    /// <param name="fromBase">指示地址的多少进制的，10或是16</param>
    public MelsecMcDataType(ushort code, byte type, string asciiCode, int fromBase)
    {
        DataCode = code;
        AsciiCode = asciiCode;
        FromBase = fromBase;
        if (type < 2)
        {
            DataType = type;
        }
    }
}
