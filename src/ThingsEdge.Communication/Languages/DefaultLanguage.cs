namespace ThingsEdge.Communication.Languages;

/// <summary>
/// 系统的语言基类，默认也即是中文版本
/// </summary>
public class DefaultLanguage
{
    public virtual string ConnectTimeout => "连接 {0} 失败，超时时间为 {1}";

    public virtual string UnknownError => "未知错误";

    public virtual string ErrorCode => "错误代号";

    public virtual string TextDescription => "文本描述";

    public virtual string SuccessText => "成功";

    public virtual string TwoParametersLengthIsNotSame => "两个参数的个数不一致";

    public virtual string NotSupportedDataType => "输入的地址解析失败，可能是该地址格式不支持或是没有输入正确的地址，请重新输入";

    public virtual string NotSupportedFunction => "当前的功能逻辑不支持，或是当前的功能没有实现";

    public virtual string DataLengthIsNotEnough => "接收的数据长度不足，应该值:{0},实际值:{1}";

    public virtual string ReceiveDataTimeout => "接收数据超时：";

    public virtual string ReceiveDataLengthTooShort => "接收的数据长度太短：";

    public virtual string MessageTip => "消息提示：";

    public virtual string IpAddressError => "Ip地址输入异常，格式不正确";

    public virtual string Send => "发送";

    public virtual string Receive => "接收";

    public virtual string CheckDataTimeout => "等待检查数据时，发生了超时，超时时间为：";

    public virtual string WriteWarning => "是否真的确认写入操作？" + Environment.NewLine + "请检查确认当前的情况是否安全！";

    public virtual string AddressFormatWrong => "地址解析的过程中发生了异常: ";

    public virtual string UserCancelOperate => "用户取消了当前的操作";

    public virtual string TooManyLock => $"当前发生了太多的读写操作，导致锁大量累计(1000)，所以本次读写失败，请稍候重试。";

    public virtual string AddressOffsetEven => "地址偏移量必须为偶数";

    public virtual string SocketIOException => "套接字传送数据异常：";

    public virtual string SocketSendException => "同步数据发送异常：";

    public virtual string SocketHeadReceiveException => "指令头接收异常：";

    public virtual string SocketContentReceiveException => "内容数据接收异常：";

    public virtual string SocketContentRemoteReceiveException => "对方内容数据接收异常：";

    public virtual string SocketAcceptCallbackException => "异步接受传入的连接尝试";

    public virtual string SocketReAcceptCallbackException => "重新异步接受传入的连接尝试";

    public virtual string SocketSendAsyncException => "异步数据发送出错:";

    public virtual string SocketEndSendException => "异步数据结束挂起发送出错";

    public virtual string SocketReceiveException => "异步数据发送出错:";

    public virtual string SocketEndReceiveException => "异步数据结束接收指令头出错";

    public virtual string SocketRemoteCloseException => "远程主机强迫关闭了一个现有的连接";

    public virtual string CommandHeadCodeCheckFailed => "命令头校验失败";

    public virtual string CommandLengthCheckFailed => "命令长度检查失败";

    public virtual string NetClientAliasFailed => "客户端的别名接收失败：";

    public virtual string NetClientAccountTimeout => "等待账户验证超时：";

    public virtual string NetEngineStart => "启动引擎";

    public virtual string NetEngineClose => "关闭引擎";

    public virtual string NetClientBreak => "异常掉线";

    public virtual string NetClientFull => "服务器承载上限，收到超出的请求连接。";

    public virtual string NetClientLoginFailed => "客户端登录中错误：";

    public virtual string NetHeartCheckFailed => "心跳验证异常：";

    public virtual string DataSourceFormatError => "数据源格式不正确";

    public virtual string ServerFileCheckFailed => "服务器确认文件失败，请重新上传";

    public virtual string ReConnectServerSuccess => "重连服务器成功";

    public virtual string ReConnectServerAfterTenSeconds => "在10秒后重新连接服务器";

    public virtual string ConnectingServer => "正在连接服务器...";

    public virtual string ConnectServerSuccess => "连接服务器成功";

    public virtual string GetClientIpAddressFailed => "客户端IP地址获取失败";

    public virtual string ConnectionIsNotAvailable => "当前的连接不可用";

    public virtual string DataTransformError => "数据转换失败，源数据：";

    public virtual string RemoteClosedConnection => "远程关闭了连接";

    public virtual string ModbusTcpFunctionCodeNotSupport => "不支持的功能码";

    public virtual string ModbusTcpFunctionCodeOverBound => "读取的数据越界";

    public virtual string ModbusTcpFunctionCodeQuantityOver => "读取长度超过最大值";

    public virtual string ModbusTcpFunctionCodeReadWriteException => "读写异常";

    public virtual string ModbusTcpReadCoilException => "读取线圈异常";

    public virtual string ModbusTcpWriteCoilException => "写入线圈异常";

    public virtual string ModbusTcpReadRegisterException => "读取寄存器异常";

    public virtual string ModbusTcpWriteRegisterException => "写入寄存器异常";

    public virtual string ModbusAddressMustMoreThanOne => "地址值在起始地址为1的情况下，必须大于1";

    public virtual string ModbusAsciiFormatCheckFailed => "Modbus的ascii指令检查失败，不是modbus-ascii报文";

    public virtual string ModbusCRCCheckFailed => "Modbus的CRC校验检查失败";

    public virtual string ModbusLRCCheckFailed => "Modbus的LRC校验检查失败";

    public virtual string ModbusMatchFailed => "不是标准的modbus协议";

    public virtual string ModbusBitIndexOverstep => "位访问的索引越界，应该在0-15之间";

    public virtual string MelsecPleaseReferToManualDocument => "请查看三菱的通讯手册来查看报警的具体信息";

    public virtual string MelsecReadBitInfo => "读取位变量数组只能针对位软元件，如果读取字软元件，请调用Read方法";

    public virtual string MelsecCurrentTypeNotSupportedWordOperate => "当前的类型不支持字读写";

    public virtual string MelsecCurrentTypeNotSupportedBitOperate => "当前的类型不支持位读写";

    public virtual string MelsecFxReceiveZero => "接收的数据长度为0";

    public virtual string MelsecFxAckNagative => "PLC反馈的数据无效";

    public virtual string MelsecFxAckWrong => "PLC反馈信号错误：";

    public virtual string MelsecFxCrcCheckFailed => "PLC反馈报文的和校验失败！";

    public virtual string MelsecError02 => "“读/写”(入/出)软元件的指定范围不正确。";

    public virtual string MelsecError51 => "在使用随机访问缓冲存储器的通讯时，由外部设备指定的起始地址设置在 0-6143 的范围之外。解决方法:检查及纠正指定的起始地址。";

    public virtual string MelsecError52 => "1. 在使用随机访问缓冲存储器的通讯时，由外部设备指定的起始地址+数据字数的计数(读时取决于设置)超出了 0-6143 的范围。\r\n2. 指定字数计数(文本)的数据不能用一个帧发送。(数据长度数值和通讯的文本总数不在允许的范围之内。)";

    public virtual string MelsecError54 => "当通过 GX Developer 在[操作设置]-[通讯数据代码]中选择“ASCII码通讯”时，则接收来自外部设备的、不能转换为二进制代码的ASCII 码。";

    public virtual string MelsecError55 => "当不能通过 GX Developer(无检查标记)来设置[操作设置]-[无法在运行时间内写入]时，如 PLCCPU 处于运行状态，则外部设备请求写入数据。 ";

    public virtual string MelsecError56 => "从外部进行的软元件指定不正确。";

    public virtual string MelsecError58 => "1. 由外部设备指定的命令起始地址(起始软元件号和起始步号)可设置在指定范围外。\r\n2. 为扩展文件寄存器指定的块号不存在。\r\n3. 不能指定文件寄存器(R)。\r\n4. 为位软元件的命令指定字软元件。\r\n5. 位软元件的起始号由某一个数值指定，此数值不是字软元件命令中16 的倍数。";

    public virtual string MelsecError59 => "不能指定扩展文件的寄存器。";

    public virtual string MelsecError430D => "请求数据异常，因此无法对用户认证启用中的CPU模块执行操作，请将用户认证功能设置为禁用。";

    public virtual string MelsecErrorC04D => "在以太网模块通过自动开放 UDP端口通讯或无序固定缓冲存储器通讯接收的信息中，应用领域中指定的数据长度不正确。";

    public virtual string MelsecErrorC050 => "当在以太网模块中进行 ASCII 代码通讯的操作设置时，接收不能转化为二进制代码的 ASCII 代码数据。";

    public virtual string MelsecErrorC051_54 => "读/写点的数目在允许范围之外。";

    public virtual string MelsecErrorC055 => "文件数据读/写点的数目在允许范围之外。";

    public virtual string MelsecErrorC056 => "读/写请求超过了最大地址。";

    public virtual string MelsecErrorC057 => "请求数据的长度与字符区域(部分文本)的数据计数不匹配。";

    public virtual string MelsecErrorC058 => "在经过 ASCII 二进制转换后，请求数据的长度与字符区域( 部分文本)的数据计数不相符。";

    public virtual string MelsecErrorC059 => "命令和子命令的指定不正确。";

    public virtual string MelsecErrorC05A_B => "以太网模块不能对指定软元件进行读出和写入";

    public virtual string MelsecErrorC05C => "请求内容不正确。 ( 以位为单元请求读 / 写至字软元件。)";

    public virtual string MelsecErrorC05D => "不执行监视注册。";

    public virtual string MelsecErrorC05E => "以太网模块和 PLC CPU 之间的通讯时问超过了 CPU 监视定时器的时间。";

    public virtual string MelsecErrorC05F => "目标 PLC 上不能执行请求。";

    public virtual string MelsecErrorC060 => "请求内容不正确。 ( 对位软元件等指定了不正确的数据。) ";

    public virtual string MelsecErrorC061 => "请求数据的长度与字符区域(部分文本)中的数据数目不相符。 ";

    public virtual string MelsecErrorC062 => "禁止在线更正时，通过 MC 协议远程 I/O 站执行( QnA兼容 3E 帧或4E 帧)写入操作。";

    public virtual string MelsecErrorC070 => "不能为目标站指定软元件存储器的范围";

    public virtual string MelsecErrorC072 => "请求内容不正确。 ( 以位为单元请求调写至字软元件。) ";

    public virtual string MelsecErrorC074 => "目标 PLC 不执行请求。需要纠正网络号和 PC 号。";

    public virtual string MelsecFxLinksError02 => "和校验错误，接收数据中的和校验码与从已经接收到的数据中生成的和校验不一致";

    public virtual string MelsecFxLinksError03 => "通信协议不正常，通信中使用的控制顺序与用参数设定的控制顺序不同。 或是一部分与指定的控制顺序不同。 或是控制顺序中指定的指令不存在。";

    public virtual string MelsecFxLinksError06 => "字符A,B,C区中错误" + Environment.NewLine + "1. 用参数设定的控制顺序不同。" + Environment.NewLine + "2.指定了对象可编程控制器中不存在的软元件编号。" + Environment.NewLine + "3. 在指定字符数目( 5个字符，或7个字符 )中没有指定软元件编号。";

    public virtual string MelsecFxLinksError07 => "在软元件中写入的数据不是16进制的ASCII码。";

    public virtual string MelsecFxLinksError0A => "不存在该PC号的站点。";

    public virtual string MelsecFxLinksError10 => "不存在该PC号的站点。";

    public virtual string MelsecFxLinksError18 => "不能执行远程RUN/SOP。在可编程控制器的硬件中决定了 RUN 或是 STOP。 (比如使用了RUN/STOP开关等)";

    public virtual string SiemensDBAddressNotAllowedLargerThan255 => "DB块数据无法大于255";

    public virtual string SiemensReadLengthMustBeEvenNumber => "读取的数据长度必须为偶数";

    public virtual string SiemensWriteError => "写入数据异常，代号为：";

    public virtual string SiemensReadLengthCannotLargerThan19 => "读取的数组数量不允许大于19";

    public virtual string SiemensDataLengthCheckFailed => "数据块长度校验失败，请检查是否开启put/get以及关闭db块优化";

    public virtual string SiemensFWError => "发生了异常，具体信息查找Fetch/Write协议文档";

    public virtual string SiemensReadLengthOverPlcAssign => "读取的数据范围超出了PLC的设定";

    public virtual string SiemensError000A => "尝试读取不存在的DB块数据";

    public virtual string SiemensError0006 => "当前操作的数据类型不支持";

    public virtual string OmronAddressMustBeZeroToFifteen => "输入的位地址只能在0-15之间";

    public virtual string OmronReceiveDataError => "数据接收异常";

    public virtual string OmronStatus0 => "通讯正常";

    public virtual string OmronStatus1 => "消息头不是FINS";

    public virtual string OmronStatus2 => "数据长度太长";

    public virtual string OmronStatus3 => "该命令不支持";

    public virtual string OmronStatus20 => "超过连接上限";

    public virtual string OmronStatus21 => "指定的节点已经处于连接中";

    public virtual string OmronStatus22 => "尝试去连接一个受保护的网络节点，该节点还未配置到PLC中";

    public virtual string OmronStatus23 => "当前客户端的网络节点超过正常范围";

    public virtual string OmronStatus24 => "当前客户端的网络节点已经被使用";

    public virtual string OmronStatus25 => "所有的网络节点已经被使用";

    public virtual string AllenBradley04 => "它没有正确生成或匹配标记不存在。";

    public virtual string AllenBradley05 => "引用的特定项（通常是实例）无法找到。";

    public virtual string AllenBradley06 => "请求的数据量不适合响应缓冲区。 发生了部分数据传输。";

    public virtual string AllenBradley0A => "尝试处理其中一个属性时发生错误。";

    public virtual string AllenBradley0C => "尝试读写操作时发生错误，在程序载入过程中读写将会触发错误。";

    public virtual string AllenBradley13 => "命令中没有提供足够的命令数据/参数来执行所请求的服务。";

    public virtual string AllenBradley1C => "与属性计数相比，提供的属性数量不足。";

    public virtual string AllenBradley1E => "此服务中的服务请求出错。";

    public virtual string AllenBradley20 => "命令中参数的数据类型与实际参数的数据类型不一致。";

    public virtual string AllenBradley26 => "IOI字长与处理的IOI数量不匹配。";

    public virtual string AllenBradleySessionStatus00 => "成功";

    public virtual string AllenBradleySessionStatus01 => "发件人发出无效或不受支持的封装命令。";

    public virtual string AllenBradleySessionStatus02 => "接收器中的内存资源不足以处理命令。 这不是一个应用程序错误。 相反，只有在封装层无法获得所需内存资源的情况下才会导致此问题。";

    public virtual string AllenBradleySessionStatus03 => "封装消息的数据部分中的数据形成不良或不正确。";

    public virtual string AllenBradleySessionStatus64 => "向目标发送封装消息时，始发者使用了无效的会话句柄。";

    public virtual string AllenBradleySessionStatus65 => "目标收到一个无效长度的信息。";

    public virtual string AllenBradleySessionStatus69 => "不支持的封装协议修订。";

    public virtual string PanasonicReceiveLengthMustLargerThan9 => "接收数据长度必须大于9";

    public virtual string PanasonicAddressParameterCannotBeNull => "地址参数不允许为空";

    public virtual string PanasonicAddressBitStartMulti16 => "位写入的起始地址需要为16的倍数，示例：R0.0, R2.0, L3.0, Y4.0";

    public virtual string PanasonicBoolLengthMulti16 => "批量bool写入的数据长度需要为16的倍数，否则无法写入";

    public virtual string PanasonicMewStatus20 => "错误未知";

    public virtual string PanasonicMewStatus21 => "NACK错误，远程单元无法被正确识别，或者发生了数据错误。";

    public virtual string PanasonicMewStatus22 => "WACK 错误:用于远程单元的接收缓冲区已满。";

    public virtual string PanasonicMewStatus23 => "多重端口错误:远程单元编号(01 至 16)设置与本地单元重复。";

    public virtual string PanasonicMewStatus24 => "传输格式错误:试图发送不符合传输格式的数据，或者某一帧数据溢出或发生了数据错误。";

    public virtual string PanasonicMewStatus25 => "硬件错误:传输系统硬件停止操作。";

    public virtual string PanasonicMewStatus26 => "单元号错误:远程单元的编号设置超出 01 至 63 的范围。";

    public virtual string PanasonicMewStatus27 => "不支持错误:接收方数据帧溢出. 试图在不同的模块之间发送不同帧长度的数据。";

    public virtual string PanasonicMewStatus28 => "无应答错误:远程单元不存在. (超时)。";

    public virtual string PanasonicMewStatus29 => "缓冲区关闭错误:试图发送或接收处于关闭状态的缓冲区。";

    public virtual string PanasonicMewStatus30 => "超时错误:持续处于传输禁止状态。";

    public virtual string PanasonicMewStatus40 => "BCC 错误:在指令数据中发生传输错误。";

    public virtual string PanasonicMewStatus41 => "格式错误:所发送的指令信息不符合传输格式。";

    public virtual string PanasonicMewStatus42 => "不支持错误:发送了一个未被支持的指令。向未被支持的目标站发送了指令。";

    public virtual string PanasonicMewStatus43 => "处理步骤错误:在处于传输请求信息挂起时,发送了其他指令。";

    public virtual string PanasonicMewStatus50 => "链接设置错误:设置了实际不存在的链接编号。";

    public virtual string PanasonicMewStatus51 => "同时操作错误:当向其他单元发出指令时,本地单元的传输缓冲区已满。";

    public virtual string PanasonicMewStatus52 => "传输禁止错误:无法向其他单元传输。";

    public virtual string PanasonicMewStatus53 => "忙错误:在接收到指令时,正在处理其他指令。";

    public virtual string PanasonicMewStatus60 => "参数错误:在指令中包含有无法使用的代码,或者代码没有附带区域指定参数(X, Y, D), 等以外。";

    public virtual string PanasonicMewStatus61 => "数据错误:触点编号,区域编号,数据代码格式(BCD,hex,等)上溢出, 下溢出以及区域指定错误。";

    public virtual string PanasonicMewStatus62 => "寄存器错误:过多记录数据在未记录状态下的操作（监控记录、跟踪记录等。)。";

    public virtual string PanasonicMewStatus63 => "PLC模式错误:当一条指令发出时，运行模式不能够对指令进行处理。";

    public virtual string PanasonicMewStatus64 => "外部记录不良错误: 1.硬件不良。内置 ROM（FROM） /主存储器/SD 存储卡中可能存在异常。\r\n2.ROM 传输时指定内容超出规定容量。\r\n3.发生了读取/写入错误。";

    public virtual string PanasonicMewStatus65 => "保护错误:在存储保护状态下执行写操作到程序区域或系统寄存器。";

    public virtual string PanasonicMewStatus66 => "地址错误:地址（程序地址、绝对地址等）数据编码形式（BCD、hex 等）、上溢、下溢或指定范围错误。";

    public virtual string PanasonicMewStatus67 => "丢失数据错误:要读的数据不存在。（读取没有写入注释寄存区的数据。";

    public virtual string PanasonicMewStatus68 => "RUN中改写不可错误: 想要编辑在 RUN 中不能改写的指令语（ED,SUB,RET,INT,IRET,SSTP,STPE）。控制单元中未写入任何内容。";

    public virtual string PanasonicMewStatus71 => "排他控制错误: 执行了不能与处理中的指令同时处理的指令。";

    public virtual string PanasonicMewStatus78 => "无SD卡错误: 未安装 SD 卡。";

    public virtual string PanasonicMewStatus80 => "担保数据异常错误: 担保数据（CRC 代码）异常。";

    public virtual string PanasonicMewStatus81 => "无有效数据错误: 不存在有效的数据。";

    public virtual string PanasonicMewStatus90 => "记录跟踪中错误: 记录跟踪过程中，执行了不能处理的指令。";

    public virtual string PanasonicMewStatus92 => "不支持SD卡错误: 未使用 Panasonic 生产的业务用 SD 卡。";

    public virtual string PanasonicMc4031 => "地址超范围（起始设备＋写入点数）";

    public virtual string PanasonicMcC051 => "设备点数指定范围外";

    public virtual string PanasonicMcC056 => "起始设备指定范围外";

    public virtual string PanasonicMcC059 => "指令搜索 MC 协议指令表格中不存在与接收数据指令一致的指令时";

    public virtual string PanasonicMcC05B => "设备代码指定范围外";

    public virtual string PanasonicMcC05C => "从指令为位单位（0001）而设备代码是字设备时";

    public virtual string PanasonicMcC05F => "1. “网络编号”检查 \r\n2. “PC 编号”检查 \r\n3. “请求对象单元 IO 编号”检查 \r\n4. 接收写入数据数异常";

    public virtual string PanasonicMcC060 => "写入触点数据异常（0/1 以外）";

    public virtual string PanasonicMcC061 => "1. 接收数据数未达到允许起始符内容检查的最低接收字节数 \r\n2. 接收数据数未达到最低接收字节数";

    public virtual string FatekStatus02 => "不合法数值";

    public virtual string FatekStatus03 => "禁止写入";

    public virtual string FatekStatus04 => "不合法的命令码";

    public virtual string FatekStatus05 => "不能激活(下RUN命令但Ladder Checksum不合)";

    public virtual string FatekStatus06 => "不能激活(下RUN命令但PLC ID≠ Ladder ID)";

    public virtual string FatekStatus07 => "不能激活（下RUN命令但程序语法错误）";

    public virtual string FatekStatus09 => "不能激活（下RUN命令，但Ladder之程序指令PLC无法执行）";

    public virtual string FatekStatus10 => "不合法的地址";

    public virtual string FujiSpbStatus01 => "对ROM进行了写入";

    public virtual string FujiSpbStatus02 => "接收了未定义的命令或无法处理的命令";

    public virtual string FujiSpbStatus03 => "数据部分有矛盾（参数异常）";

    public virtual string FujiSpbStatus04 => "由于收到了其他编程器的传送联锁，因此无法处理";

    public virtual string FujiSpbStatus05 => "模块序号不正确";

    public virtual string FujiSpbStatus06 => "检索项目未找到";

    public virtual string FujiSpbStatus07 => "指定了超出模块范围的地址（写入时）";

    public virtual string FujiSpbStatus09 => "由于故障程序无法执行（RUN）";

    public virtual string FujiSpbStatus0C => "密码不一致";

    public virtual string KeyenceSR2000Error00 => "接收未定义的命令";

    public virtual string KeyenceSR2000Error01 => "命令格式不匹配。（参数的数量有误）";

    public virtual string KeyenceSR2000Error02 => "超出参数1的设置范围";

    public virtual string KeyenceSR2000Error03 => "超出参数2的设置范围";

    public virtual string KeyenceSR2000Error04 => "在HEX（十六进制）码中未设置参数2";

    public virtual string KeyenceSR2000Error05 => "参数2属于HEX（十六进制）码，但是超出了设置范围";

    public virtual string KeyenceSR2000Error10 => "预设数据内存在两个以上的！预设数据有误";

    public virtual string KeyenceSR2000Error11 => "区域指定数据有误";

    public virtual string KeyenceSR2000Error12 => "指定文件不存在";

    public virtual string KeyenceSR2000Error13 => "超出了%Tmm-LON, bb命令的mm的设置范围";

    public virtual string KeyenceSR2000Error14 => "用%Tmm-KEYENCE命令无法确认通信";

    public virtual string KeyenceSR2000Error20 => "在当前的模式下不允许执行此命令（执行错误）";

    public virtual string KeyenceSR2000Error21 => "缓冲区已满，不能执行命令";

    public virtual string KeyenceSR2000Error22 => "加载或保存参数时发生错误";

    public virtual string KeyenceSR2000Error23 => "由于正在连接 AutoID Netwoerk Navigator, 因此不能接收 RS-232C 发送的命令";

    public virtual string KeyenceSR2000Error99 => "如果觉得SR-2000系列有异常，请联系基恩士公司";

    public virtual string KeyenceNanoE0 => "1. 指定的软元件编号、存储体编号、单元编号、地址超出范围。" + Environment.NewLine + "2. 指定了程序不用的定时器、计数器、CTH 和 CTC 的编号。" + Environment.NewLine + "3. 未登录监控器，却要进行监控器读取。";

    public virtual string KeyenceNanoE1 => "1. 发送了CPU单元不支持的指令。" + Environment.NewLine + "2. 指定指令的方法出错。" + Environment.NewLine + "3. 确立通讯前，发送了 CR 以外的指令。";

    public virtual string KeyenceNanoE2 => "1. 在 CPU 单元没有存储程序的状态下， 发送了“M1（切换到 RUN 模式）”指令。" + Environment.NewLine + "2. 在 CPU 单元的 RUN/PROG 开关处于PROG 状态下，发送了“M1（切换到RUN 模式）”指令。";

    public virtual string KeyenceNanoE4 => "想要更改写入去能程序的定时器、计数器和 CTC 的设定值。";

    public virtual string KeyenceNanoE5 => "在尚未排除CPU单元错误的情况下， 发送了“M1( 切换到RUN模式)”指令。";

    public virtual string KeyenceNanoE6 => "读取“RDC”指令选定的软元件中。";

    public virtual string GeSRTPNotSupportBitReadWrite => "当前的地址数据不支持位单位的读写操作";

    public virtual string GeSRTPAddressCannotBeZero => "当前的地址起始地址不能为0，需要从1开始";

    public virtual string GeSRTPNotSupportByteReadWrite => "当前的地址数据不支持字节单位的读写操作，只能使用字单位读写";

    public virtual string GeSRTPWriteLengthMustBeEven => "当前的地址写入的数据长度必须是偶数";
}
