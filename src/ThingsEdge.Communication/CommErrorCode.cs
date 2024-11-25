using System.ComponentModel;

namespace ThingsEdge.Communication;

/// <summary>
/// 通信库错误代码，错误代码区间为 [800, 999]。
/// </summary>
public enum CommErrorCode
{
    [Description("错误")]
    Error = 800,

    [Description("连接失败")]
    ConnectedFailed,

    [Description("当前的连接不可用")]
    ConnectionIsNotAvailable,

    [Description("当前的功能逻辑不支持，或是当前的功能没有实现")]
    NotSupportedFunction,

    [Description("两个参数的个数不一致")]
    TwoParametersLengthIsNotSame,

    [Description("不支持的数据类型错误")]
    NotSupportedDataType,

    [Description("Token 检测失败")]
    TokenCheckFailed,

    CommandLengthCheckFailed,

    UnpackResponseContentError,

    [Description("连接超时")]
    ConnectTimeout,

    [Description("接收数据超时")]
    ReceiveDataTimeout,

    [Description("等待检查数据时发生了超时")]
    CheckDataTimeout,

    [Description("心跳验证超时")]
    NetHeartCheckTimeout,

    [Description("接收的数据长度太短")]
    ReceiveDataLengthTooShort,

    [Description("远程连接已关闭")]
    RemoteClosedConnection,

    [Description("Ip地址格式不正确")]
    IpAddressError,

    [Description("服务器承载上限，收到超出的请求连接")]
    NetClientFull,

    [Description("异常掉线")]
    NetClientBreak,

    [Description("数据转换失败")]
    DataTransformError,

    [Description("命令头校验失败")]
    CommandHeadCodeCheckFailed,

    [Description("套接字传送数据异常")]
    SocketIOException,

    [Description("套接字连接异常")]
    SocketConnectException,

    [Description("套接字连接超时异常")]
    SocketConnectTimeoutException,

    [Description("套接字异常")]
    SocketException,

    [Description("套接字不可用")]
    SocketUnavailable,

    [Description("同步数据发送异常")]
    SocketSendException,

    [Description("指令头接收异常")]
    SocketHeadReceiveException,

    [Description("内容数据接收异常")]
    SocketContentReceiveException,

    [Description("对方内容数据接收异常")]
    SocketContentRemoteReceiveException,

    [Description("异步接受传入的连接尝试")]
    SocketAcceptCallbackException,

    [Description("重新异步接受传入的连接尝试")]
    SocketReAcceptCallbackException,

    [Description("异步数据发送出错")]
    SocketSendAsyncException,

    [Description("异步数据结束挂起发送出错")]
    SocketEndSendException,

    [Description("异步数据接收出错")]
    SocketReceiveException,

    [Description("异步数据结束接收指令头出错")]
    SocketEndReceiveException,

    [Description("远程主机强迫关闭了一个现有的连接")]
    SocketRemoteCloseException,

    [Description("你的主机中的软件中止了一个已建立的连接")]
    SocketConnectionAborted,

    [Description("打开串口出现异常")]
    OpenSerialPortException,

    [Description("关闭串口出现异常")]
    CloseSerialPortException,

    SerialPortSendException,

    SerialPortReceiveException,

    [Description("DB块数据无法大于255")]
    SiemensDBAddressNotAllowedLargerThan255,

    [Description("读取的数据长度必须为偶数")]
    SiemensReadLengthMustBeEvenNumber,

    [Description("写入数据异常")]
    SiemensWriteError,

    [Description("西门子PLC启动异常")]
    SiemensStartPLCError,

    [Description("西门子PLC停止异常")]
    SiemensStopPLCError,

    [Description("读取的数组数量不允许大于19")]
    SiemensReadLengthCannotLargerThan19,

    [Description("数据块长度校验失败，请检查是否开启put/get以及关闭db块优化")]
    SiemensDataLengthCheckFailed,

    [Description("值在 PLC 中不是 String 类型")]
    SiemensValueOfPlcIsNotStringType,

    [Description("写入的 String 长度超过了在 PLC 中定义的长度")]
    SiemensStringlengthIsToolongThanPlcDefined,

    [Description("发生了异常，具体信息查找Fetch/Write协议文档")]
    SiemensFWError,

    [Description("读取的数据范围超出了PLC的设定")]
    SiemensReadLengthOverPlcAssign,

    [Description("尝试读取不存在的DB块数据")]
    SiemensError000A,

    [Description("当前操作的数据类型不支持")]
    SiemensError0006,

    [Description("输入的位地址只能在0-15之间")]
    OmronAddressMustBeZeroToFifteen,

    [Description("数据接收异常")]
    OmronReceiveDataError,

    OmronAddressWrong,

    OmronForwardOpenFailed,

    [Description("通讯正常")]
    OmronStatus0,

    [Description("消息头不是FINS")]
    OmronStatus1,

    [Description("数据长度太长")]
    OmronStatus2,

    [Description("该命令不支持")]
    OmronStatus3,

    [Description("超过连接上限")]
    OmronStatus20,

    [Description("指定的节点已经处于连接中")]
    OmronStatus21,

    [Description("尝试去连接一个受保护的网络节点，该节点还未配置到PLC中")]
    OmronStatus22,

    [Description("当前客户端的网络节点已经被使用")]
    OmronStatus23,

    [Description("当前客户端的网络节点已经被使用")]
    OmronStatus24,

    [Description("所有的网络节点已经被使用")]
    OmronStatus25,

    [Description("查看三菱协议文档")]
    MelsecPleaseReferToManualDocument,

    [Description("当前的类型不支持字读写")]
    MelsecCurrentTypeNotSupportedWordOperate,

    [Description("当前的类型不支持位读写")]
    MelsecCurrentTypeNotSupportedBitOperate,

    [Description("接收的数据长度为0")]
    MelsecFxReceiveZero,

    [Description("PLC反馈的数据无效")]
    MelsecFxAckNagative,

    [Description("PLC反馈信号错误")]
    MelsecFxAckWrong,

    [Description("PLC反馈报文的和校验失败")]
    MelsecFxCrcCheckFailed,

    MelsecReadPlcTypeError,

    MelsecStartPLCFailed,

    MelsecStopPLCFailed,

    MelsecReadFailed,

    MelsecWriteFailed,

    MelsecError,

    [Description("读/写”(入/出)软元件的指定范围不正确")]
    MelsecError02,

    [Description("在使用随机访问缓冲存储器的通讯时，由外部设备指定的起始地址设置在 0-6143 的范围之外。解决方法:检查及纠正指定的起始地址")]
    MelsecError51,

    [Description("1. 在使用随机访问缓冲存储器的通讯时，由外部设备指定的起始地址+数据字数的计数(读时取决于设置)超出了 0-6143 的范围。\r\n2. 指定字数计数(文本)的数据不能用一个帧发送。(数据长度数值和通讯的文本总数不在允许的范围之内。)")]
    MelsecError52,

    [Description("当通过 GX Developer 在[操作设置]-[通讯数据代码]中选择“ASCII码通讯”时，则接收来自外部设备的、不能转换为二进制代码的ASCII 码")]
    MelsecError54,

    [Description("当不能通过 GX Developer(无检查标记)来设置[操作设置]-[无法在运行时间内写入]时，如 PLCCPU 处于运行状态，则外部设备请求写入数据")]
    MelsecError55,

    [Description("从外部进行的软元件指定不正确")]
    MelsecError56,

    [Description("1. 由外部设备指定的命令起始地址(起始软元件号和起始步号)可设置在指定范围外。\r\n2. 为扩展文件寄存器指定的块号不存在。\r\n3. 不能指定文件寄存器(R)。\r\n4. 为位软元件的命令指定字软元件。\r\n5. 位软元件的起始号由某一个数值指定，此数值不是字软元件命令中16 的倍数。")]
    MelsecError58,

    [Description("不能指定扩展文件的寄存器")]
    MelsecError59,

    [Description("在以太网模块通过自动开放 UDP端口通讯或无序固定缓冲存储器通讯接收的信息中，应用领域中指定的数据长度不正确")]
    MelsecErrorC04D,

    [Description("当在以太网模块中进行 ASCII 代码通讯的操作设置时，接收不能转化为二进制代码的 ASCII 代码数据")]
    MelsecErrorC050,

    [Description("读/写点的数目在允许范围之外")]
    MelsecErrorC051_54,

    [Description("文件数据读/写点的数目在允许范围之外")]
    MelsecErrorC055,

    [Description("读/写请求超过了最大地址")]
    MelsecErrorC056,

    [Description("请求数据的长度与字符区域(部分文本)的数据计数不匹配")]
    MelsecErrorC057,

    [Description("在经过 ASCII 二进制转换后，请求数据的长度与字符区域(部分文本)的数据计数不相符")]
    MelsecErrorC058,

    [Description("命令和子命令的指定不正确")]
    MelsecErrorC059,

    [Description("以太网模块不能对指定软元件进行读出和写入")]
    MelsecErrorC05A_B,

    [Description("请求内容不正确。 ( 以位为单元请求读 / 写至字软元件。)")]
    MelsecErrorC05C,

    [Description("不执行监视注册")]
    MelsecErrorC05D,

    [Description("以太网模块和 PLC CPU 之间的通讯时问超过了 CPU 监视定时器的时间")]
    MelsecErrorC05E,

    [Description("目标 PLC 上不能执行请求")]
    MelsecErrorC05F,

    [Description("请求内容不正确。(对位软元件等指定了不正确的数据) ")]
    MelsecErrorC060,

    [Description("请求数据的长度与字符区域(部分文本)中的数据数目不相符")]
    MelsecErrorC061,

    [Description("禁止在线更正时，通过 MC 协议远程 I/O 站执行( QnA兼容 3E 帧或4E 帧)写入操作")]
    MelsecErrorC062,

    [Description("不能为目标站指定软元件存储器的范围")]
    MelsecErrorC070,

    [Description("请求内容不正确。(以位为单元请求调写至字软元件)")]
    MelsecErrorC072,

    [Description("目标 PLC 不执行请求。需要纠正网络号和 PC 号")]
    MelsecErrorC074,

    [Description("它没有正确生成或匹配标记不存在")]
    AllenBradley04,

    [Description("引用的特定项（通常是实例）无法找到")]
    AllenBradley05,

    [Description("请求的数据量不适合响应缓冲区。 发生了部分数据传输")]
    AllenBradley06,

    [Description("尝试处理其中一个属性时发生错误")]
    AllenBradley0A,

    [Description("命令中没有提供足够的命令数据/参数来执行所请求的服务")]
    AllenBradley13,

    [Description("与属性计数相比，提供的属性数量不足")]
    AllenBradley1C,

    [Description("此服务中的服务请求出错")]
    AllenBradley1E,

    [Description("命令中参数的数据类型与实际参数的数据类型不一致")]
    AllenBradley20,

    [Description("IOI字长与处理的IOI数量不匹配")]
    AllenBradley26,

    [Description("成功")]
    AllenBradleySessionStatus00,

    [Description("发件人发出无效或不受支持的封装命令")]
    AllenBradleySessionStatus01,

    [Description("接收器中的内存资源不足以处理命令")]
    AllenBradleySessionStatus02,

    [Description("封装消息的数据部分中的数据形成不良或不正确")]
    AllenBradleySessionStatus03,

    [Description("向目标发送封装消息时，始发者使用了无效的会话句柄")]
    AllenBradleySessionStatus64,

    [Description("目标收到一个无效长度的信息")]
    AllenBradleySessionStatus65,

    [Description("不支持的封装协议修订")]
    AllenBradleySessionStatus69,

    ModbusAsciiFormatCheckFailed,

    ModbusLRCCheckFailed,

    ModbusTransAsciiPackError,
}
