using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Robot.YASKAWA.Helper;

namespace ThingsEdge.Communication.Robot.YASKAWA;

/// <summary>
/// 安川机器人的通信类，基于高速以太网的通信，基于UDP协议实现，默认端口10040，支持读写一些数据地址
/// </summary>
public class YRCHighEthernet : NetworkUdpBase
{
    private readonly IByteTransform _byteTransform = new RegularByteTransform();

    private readonly SoftIncrementCount _incrementCount = new(255L, 0L);

    private readonly byte _handle = 1;

    private Encoding _encoding = Encoding.ASCII;

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public YRCHighEthernet()
    {
    }

    /// <summary>
    /// 使用指定的IP地址和端口号信息来实例化一个对象
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="port">端口号信息</param>
    public YRCHighEthernet(string ipAddress, int port = 10040)
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <summary>
    /// 使用自定义的命令来读取机器人指定的数据信息，每个命令返回的数据格式互不相同，需要根据手册来自定义解析的。<br />
    /// Use custom commands to read the data information specified by the robot. 
    /// The data format returned by each command is different from each other, 
    /// and you need to customize the analysis according to the manual.
    /// </summary>
    /// <param name="command">命令编号，相当于CIP 通信协议的Class</param>
    /// <param name="dataAddress">数据队列编号，相当于CIP 通信协议的Instance</param>
    /// <param name="dataAttribute">单元编号，相当于CIP 通信协议的Attribute</param>
    /// <param name="dataHandle">处理(请求), 定义数据请求方法。</param>
    /// <param name="dataPart">附加数据信息</param>
    /// <returns>从机器人返回的设备数据，如果是写入状态，则 Content 为 NULL</returns>
    public OperateResult<byte[]> ReadCommand(ushort command, ushort dataAddress, byte dataAttribute, byte dataHandle, byte[] dataPart)
    {
        var send = YRCHighEthernetHelper.BuildCommand(_handle, (byte)_incrementCount.GetCurrentValue(), command, dataAddress, dataAttribute, dataHandle, dataPart);
        var operateResult = ReadFromCoreServer(send);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = YRCHighEthernetHelper.CheckResponseContent(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        if (operateResult.Content.Length > 32)
        {
            return OperateResult.CreateSuccessResult(operateResult.Content.RemoveBegin(32));
        }
        return OperateResult.CreateSuccessResult(new byte[0]);
    }

    /// <summary>
    /// 读取机器人的最新的报警列表信息，最多为四个报警
    /// </summary>
    /// <returns>报警列表信息</returns>
    public OperateResult<YRCAlarmItem[]> ReadAlarms()
    {
        var array = new YRCAlarmItem[4];
        for (var i = 0; i < array.Length; i++)
        {
            var operateResult = ReadCommand(112, (ushort)(i + 1), 0, 1, null);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<YRCAlarmItem[]>(operateResult);
            }
            if (operateResult.Content.Length != 0)
            {
                array[i] = new YRCAlarmItem(_byteTransform, operateResult.Content, _encoding);
            }
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 读取机器人的指定的报警信息，需要指定报警类型，及报警数量，其中length为1-100之间。
    /// </summary>
    /// <param name="alarmType">报警类型，1-100:重故障; 1001-1100: 轻故障; 2001-2100: 用户报警(系统); 3001-3100: 用户报警(用户); 4001-4100:在线报警</param>
    /// <param name="length">读取的报警的个数</param>
    /// <returns>报警列表信息</returns>
    public OperateResult<YRCAlarmItem[]> ReadHistoryAlarms(ushort alarmType, short length)
    {
        var array = new YRCAlarmItem[length];
        for (var i = 0; i < array.Length; i++)
        {
            var operateResult = ReadCommand(113, alarmType, 0, 1, null);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<YRCAlarmItem[]>(operateResult);
            }
            if (operateResult.Content.Length != 0)
            {
                array[i] = new YRCAlarmItem(_byteTransform, operateResult.Content, _encoding);
            }
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.ReadStats" />
    public OperateResult<bool[]> ReadStats()
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(114, 1, 0, 1, null), (m) => new byte[2]
        {
            (byte)_byteTransform.TransInt32(m, 0),
            (byte)_byteTransform.TransInt32(m, 4)
        }.ToBoolArray());
    }

    /// <summary>
    /// 读取当前的机器人的程序名称，行编号，步骤编号，速度超出值。需要指定当前的任务号，默认为1，表示主任务<br />
    /// Read the current robot's program name, line number, step number, and speed exceeding value. 
    /// Need to specify the current task number, the default is 1, which means the main task
    /// </summary>
    /// <param name="task">任务标识，1:主任务; 2-16分别表示子任务1-子任务15</param>
    /// <returns>读取的任务的结果信息</returns>
    public OperateResult<string[]> ReadJSeq(ushort task = 1)
    {
        var operateResult = ReadCommand(115, task, 0, 1, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(new string[4]
        {
            _encoding.GetString(operateResult.Content, 0, 32),
            _byteTransform.TransInt32(operateResult.Content, 32).ToString(),
            _byteTransform.TransInt32(operateResult.Content, 36).ToString(),
            _byteTransform.TransInt32(operateResult.Content, 40).ToString()
        });
    }

    /// <summary>
    /// 读取机器人的姿态信息，包括X,Y,Z,Rx,Ry,Rz,如果是七轴机器人，还包括Re
    /// </summary>
    /// <returns>姿态信息</returns>
    public OperateResult<string[]> ReadPose()
    {
        var operateResult = ReadCommand(117, 101, 0, 1, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string[]>(operateResult);
        }
        var array = new string[operateResult.Content.Length / 4 - 5];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = _byteTransform.TransInt32(operateResult.Content, 20 + i * 4).ToString();
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 读取力矩数据功能
    /// </summary>
    /// <returns>力矩信息</returns>
    public OperateResult<string[]> ReadTorqueData()
    {
        var operateResult = ReadCommand(119, 21, 0, 1, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string[]>(operateResult);
        }
        var array = new string[operateResult.Content.Length / 4];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = _byteTransform.TransInt32(operateResult.Content, i * 4).ToString();
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRCHighEthernet.ReadIO(System.UInt16,System.Int32)" />
    public OperateResult<byte> ReadIO(ushort address)
    {
        var operateResult = ReadCommand(120, address, 1, 14, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content[0]);
    }

    /// <summary>
    /// 写入IO的数据，只可写入网络输入信号，也即地址是 2701~2956
    /// </summary>
    /// <param name="address">网络输入信号，也即地址是 2701~2956</param>
    /// <param name="value">表示8个bool的字节数据</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteIO(ushort address, byte value)
    {
        return ReadCommand(120, address, 1, 16, new byte[1] { value });
    }

    /// <summary>
    /// 读取IO数据，需要指定IO的地址。
    /// </summary>
    /// <remarks>
    /// io地址如下：<br />
    /// 1~512: 机器人通用输入命令；1001~1512：机器人通用输出命令；2001~2512：外部输入信号；2701~2956：网络输入信号；
    /// 3001~3512：外部输出信号；3701~3956：网络输出信号；4001~4256：机器人专用输入信号；5001~5512：机器人专用输出信号；
    /// 6001~6064：接口面板输入信号；7001~7999：辅助继电器信号；8001~8512：机器人控制状态信号；8701~8720：模拟输入信号；
    /// </remarks>
    /// <param name="address">信号地址，详细参见注释</param>
    /// <param name="length">读取的数据长度信息</param>
    /// <returns>bool值</returns>
    public OperateResult<byte[]> ReadIO(ushort address, int length)
    {
        var operateResult = ReadCommand(768, address, 0, 51, _byteTransform.TransByte(length));
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var length2 = _byteTransform.TransInt32(operateResult.Content, 0);
        return OperateResult.CreateSuccessResult(operateResult.Content.SelectMiddle(4, length2));
    }

    /// <summary>
    /// 写入多个IO数据的命令，写入的字节长度需要是2的倍数
    /// </summary>
    /// <param name="address">网络输入信号，也即地址是 2701~2956</param>
    /// <param name="value">连续的字数据</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteIO(ushort address, byte[] value)
    {
        return ReadCommand(768, address, 0, 52, value);
    }

    /// <summary>
    /// 读取寄存器的数据，地址范围 0 ~ 999
    /// </summary>
    /// <param name="address">地址索引</param>
    /// <returns>读取结果数据</returns>
    public OperateResult<ushort> ReadRegisterVariable(ushort address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(121, address, 1, 14, null), (m) => _byteTransform.TransUInt16(m, 0));
    }

    /// <summary>
    /// 将数据写入到寄存器，支持写入的地址范围为 0 ~ 599
    /// </summary>
    /// <param name="address">地址索引</param>
    /// <param name="value">等待写入的值</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteRegisterVariable(ushort address, ushort value)
    {
        return ReadCommand(121, address, 1, 16, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 批量读取多个寄存器的数据，地址范围 0 ~ 999，指定读取的数据长度，最大不超过237 个
    /// </summary>
    /// <param name="address">地址索引</param>
    /// <param name="length">读取的数据长度，最大不超过237 个</param>
    /// <returns>读取结果内容</returns>
    public OperateResult<ushort[]> ReadRegisterVariable(ushort address, int length)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(769, address, 0, 51, _byteTransform.TransByte(length)), (m) => _byteTransform.TransUInt16(m, 0, length));
    }

    /// <summary>
    /// 写入多个数据到寄存器，地址范围 0 ~ 999，指定读取的数据长度，最大不超过237 个
    /// </summary>
    /// <param name="address">地址索引</param>
    /// <param name="value">等待写入的数据，最大不超过237 个长度</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteRegisterVariable(ushort address, ushort[] value)
    {
        return ReadCommand(769, address, 0, 52, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取字节型变量的数据，标准地址范围为 0 ~ 99
    /// </summary>
    /// <param name="address">标准地址范围为 0 ~ 99</param>
    /// <returns>读取的结果对象</returns>
    public OperateResult<byte> ReadByteVariable(ushort address)
    {
        return ByteTransformHelper.GetResultFromArray(ReadCommand(122, address, 1, 14, null));
    }

    /// <summary>
    /// 将数据写入到字节型变量的地址里去，标准地址范围为 0 ~ 99
    /// </summary>
    /// <param name="address">标准地址范围为 0 ~ 99</param>
    /// <param name="value">等待写入的值</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteByteVariable(ushort address, byte value)
    {
        return ReadCommand(122, address, 1, 16, new byte[1] { value });
    }

    /// <summary>
    /// 读取多个的字节型变量的数据，读取的最大个数为 474 个。
    /// </summary>
    /// <param name="address">标准地址范围为 0 ~ 99</param>
    /// <param name="length">读取的数据个数，读取的最大个数为 474 个</param>
    /// <returns>结果数据内容</returns>
    public OperateResult<byte[]> ReadByteVariable(ushort address, int length)
    {
        return ReadCommand(770, address, 0, 51, _byteTransform.TransByte(length));
    }

    /// <summary>
    /// 将多个字节的变量的数据写入到指定的地址，最大个数为 474 个，仅可指定2的倍数
    /// </summary>
    /// <param name="address">标准地址范围为 0 ~ 99</param>
    /// <param name="vaule">写入的值，最大个数为 474 个，仅可指定2的倍数</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteByteVariable(ushort address, byte[] vaule)
    {
        return ReadCommand(770, address, 0, 52, vaule);
    }

    /// <summary>
    /// 读取单个的整型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">0 ～ 99（ 标准设定时）</param>
    /// <returns>读取结果对象</returns>
    public OperateResult<short> ReadIntegerVariable(ushort address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(123, address, 1, 14, null), (m) => _byteTransform.TransInt16(m, 0));
    }

    /// <summary>
    /// 将单个的数据写入到整型变量去，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的值</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteIntegerVariable(ushort address, short value)
    {
        return ReadCommand(123, address, 1, 16, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取多个的整型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="length">读取的个数</param>
    /// <returns>读取结果对象</returns>
    public OperateResult<short[]> ReadIntegerVariable(ushort address, int length)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(771, address, 0, 51, _byteTransform.TransByte(length)), (m) => _byteTransform.TransInt16(m, 0, length));
    }

    /// <summary>
    /// 写入多个的整型变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的数据信息</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteIntegerVariable(ushort address, short[] value)
    {
        return ReadCommand(771, address, 0, 52, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取单个的双精度整型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">0 ～ 99（ 标准设定时）</param>
    /// <returns>读取结果对象</returns>
    public OperateResult<int> ReadDoubleIntegerVariable(ushort address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(124, address, 1, 14, null), (m) => _byteTransform.TransInt32(m, 0));
    }

    /// <summary>
    /// 将单个的数据写入到双精度整型变量去，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的值</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteDoubleIntegerVariable(ushort address, int value)
    {
        return ReadCommand(124, address, 1, 16, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取多个的双精度整型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="length">读取的个数，最大118个</param>
    /// <returns>读取结果对象</returns>
    public OperateResult<int[]> ReadDoubleIntegerVariable(ushort address, int length)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(772, address, 0, 51, _byteTransform.TransByte(length)), (m) => _byteTransform.TransInt32(m, 0, length));
    }

    /// <summary>
    /// 写入多个的双精度整型变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的数据信息，最大118个数据</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteDoubleIntegerVariable(ushort address, int[] value)
    {
        return ReadCommand(772, address, 0, 52, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取单个的实数型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <returns>读取结果内容</returns>
    public OperateResult<float> ReadRealVariable(ushort address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(125, address, 1, 14, null), (m) => _byteTransform.TransSingle(m, 0));
    }

    /// <summary>
    /// 将单个的数据写入到实数型变量去，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">写入的值</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteRealVariable(ushort address, float value)
    {
        return ReadCommand(125, address, 1, 16, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取多个的实数型变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="length">读取的个数，最大118个</param>
    /// <returns>读取的结果对象</returns>
    public OperateResult<float[]> ReadRealVariable(ushort address, int length)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(773, address, 0, 51, _byteTransform.TransByte(length)), (m) => _byteTransform.TransSingle(m, 0, length));
    }

    /// <summary>
    /// 写入多个的实数型的变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的数据信息，最大118个数据</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteRealVariable(ushort address, float[] value)
    {
        return ReadCommand(773, address, 0, 52, _byteTransform.TransByte(value));
    }

    /// <summary>
    /// 读取单个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <returns>读取的结果对象</returns>
    public OperateResult<string> ReadStringVariable(ushort address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(ReadCommand(126, address, 1, 14, null), (m) => _byteTransform.TransString(m, _encoding));
    }

    /// <summary>
    /// 写入单个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">写入的字符串数据</param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteStringVariable(ushort address, string value)
    {
        return ReadCommand(126, address, 1, 16, SoftBasic.ArrayExpandToLength(_encoding.GetBytes(value), 16));
    }

    /// <summary>
    /// 读取多个的字符串变量数据，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="length">读取的字符串个数，最大个数为 29 </param>
    /// <returns>读取的结果对象</returns>
    public OperateResult<string[]> ReadStringVariable(ushort address, int length)
    {
        var operateResult = ReadCommand(774, address, 0, 51, _byteTransform.TransByte(length));
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string[]>(operateResult);
        }
        var array = new string[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = _encoding.GetString(operateResult.Content, i * 16, 16);
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 写入多个的字符串变量数据到机器人，地址范围：0 ～ 99（ 标准设定时）
    /// </summary>
    /// <param name="address">地址范围：0 ～ 99（ 标准设定时）</param>
    /// <param name="value">等待写入的字符串数组，最大数组长度为 29 </param>
    /// <returns>是否写入成功</returns>
    public OperateResult WriteStringVariable(ushort address, string[] value)
    {
        var array = new byte[value.Length * 16];
        for (var i = 0; i < value.Length; i++)
        {
            _encoding.GetBytes(value[i]).CopyTo(array, i * 16);
        }
        return ReadCommand(774, address, 0, 52, array);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Hold(System.Boolean)" />
    [HslMqttApi(Description = "进行HOLD 的 ON/OFF 操作，状态参数 False: OFF，True: ON")]
    public OperateResult Hold(bool status)
    {
        return ReadCommand(131, 1, 1, 16, status ? _byteTransform.TransByte(1) : _byteTransform.TransByte(2));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Reset" />
    [HslMqttApi(Description = "对机械手的报警进行复位")]
    public OperateResult Reset()
    {
        return ReadCommand(130, 1, 1, 16, _byteTransform.TransByte(1));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Cancel" />
    [HslMqttApi(Description = "进行错误取消")]
    public OperateResult Cancel()
    {
        return ReadCommand(130, 2, 1, 16, _byteTransform.TransByte(1));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Svon(System.Boolean)" />
    [HslMqttApi(Description = "进行伺服电源的ON/OFF操作，状态参数 False: OFF，True: ON")]
    public OperateResult Svon(bool status)
    {
        return ReadCommand(131, 2, 1, 16, status ? _byteTransform.TransByte(1) : _byteTransform.TransByte(2));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.HLock(System.Boolean)" />
    [HslMqttApi(Description = "设定示教编程器和 I/O的操作信号的联锁。 状态参数 False: OFF，True: ON")]
    public OperateResult HLock(bool status)
    {
        return ReadCommand(131, 3, 1, 16, status ? _byteTransform.TransByte(1) : _byteTransform.TransByte(2));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Cycle(System.Int32)" />
    [HslMqttApi(Description = "选择循环。循环编号 1:步骤，2:1循环，3:连续自动")]
    public OperateResult Cycle(int number)
    {
        return ReadCommand(132, 2, 1, 16, _byteTransform.TransByte(number));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.MSDP(System.String)" />
    [HslMqttApi(Description = "接受消息数据时， 在YRC1000的示教编程器的远程画面下显示消息若。若不是远程画面时，强制切换到远程画面。显示MDSP命令的消息。")]
    public OperateResult MSDP(string message)
    {
        return ReadCommand(133, 1, 1, 16, string.IsNullOrEmpty(message) ? new byte[0] : _encoding.GetBytes(message));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.Start(System.String)" />
    [HslMqttApi(Description = "开始程序。操作时指定程序名时，此程序能附带对应主程序，则从该程序的开头开始执行。如果没有指定，则从前行开始执行")]
    public OperateResult Start()
    {
        return ReadCommand(134, 1, 1, 16, _byteTransform.TransByte(1));
    }

    /// <summary>
    /// 读取机器人的时间信息，根据地址来获取不同的时间，地址如下：<br />
    /// 1: 控制电源的接通时间<br />
    /// 10: 伺服电源接通时间(TOTAL)<br />
    /// 11~18: 伺服电源接通时间(R1~R8)<br />
    /// 21~44: 伺服电源接通时间(S1~S24)<br />
    /// 110: 再线时间（TOTAL）<br />
    /// 111~118: 再线时间（R1~R8）<br />
    /// 121~144: 再线时间 (S1~S24)<br />
    /// 210: 移动时间（TOTAL）<br />
    /// 211~218: 移动时间（R1~R8）<br />
    /// 221~244: 移动时间（S1~S24）<br />
    /// 301~308: 作业时间（用途1~用途8）
    /// </summary>
    /// <param name="address">时间的地址信息，具体参照方法的注释</param>
    /// <returns>读取的时间信息</returns>
    public OperateResult<DateTime> ReadManagementTime(ushort address)
    {
        var operateResult = ReadCommand(136, address, 1, 14, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(operateResult);
        }
        return OperateResult.CreateSuccessResult(Convert.ToDateTime(Encoding.ASCII.GetString(operateResult.Content, 0, 16)));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRCHighEthernet.ReadManagementTime(System.UInt16)" />
    public OperateResult<string> ReadManagementTimeSpan(ushort address)
    {
        var operateResult = ReadCommand(136, address, 2, 14, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult.Content, 0, 12));
    }

    /// <summary>
    /// 读取系统的参数信息，其中系统种类参数：<br />
    /// 11~18:机种信息R1~R8; <br />
    /// 21~44:机种信息S1~S24; <br />
    /// 101~108: 用途信息(用途1~用途8); <br />
    /// 返回数据信息为数组，分别为 [0]:系统软件版本；[1]:机种名称/用途名称；[2]:参数版本
    /// </summary>
    /// <param name="system">统种类参数：11~18:机种信息R1~R8; 21~44:机种信息S1~S24; 101~108: 用途信息(用途1~用途8);</param>
    /// <returns>返回数据信息为数组，分别为 [0]:系统软件版本；[1]:机种名称/用途名称；[2]:参数版本</returns>
    public OperateResult<string[]> ReadSystemInfo(ushort system)
    {
        var operateResult = ReadCommand(137, system, 0, 1, null);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(new string[3]
        {
            _encoding.GetString(operateResult.Content, 0, 24),
            _encoding.GetString(operateResult.Content, 24, 16),
            _encoding.GetString(operateResult.Content, 40, 8)
        });
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YASKAWA.YRC1000TcpNet.JSeq(System.String,System.Int32)" />
    [HslMqttApi(Description = "设定执行程序的名称和行编号。")]
    public OperateResult JSeq(string programName, int line)
    {
        var array = new byte[36];
        _encoding.GetBytes(programName).CopyTo(array, 0);
        _byteTransform.TransByte(line).CopyTo(array, 32);
        return ReadCommand(132, 2, 1, 16, array);
    }

    /// <summary>
    /// 指定坐标系的当前值读取。并且可以指定外部轴的有无。<br />
    /// The current value of the specified coordinate system is read. And you can specify the presence or absence of an external axis.
    /// </summary>
    /// <param name="coordinate">指定读取坐标 0:基座坐标，1:机器人坐标，2-65分别表示用户坐标1-64</param>
    /// <param name="hasExteralAxis">外部轴的有/无</param>
    /// <returns>坐标系当前值</returns>
    [HslMqttApi(Description = "指定坐标系的当前值读取。并且可以指定外部轴的有无。")]
    public OperateResult<YRCRobotData> ReadPOSC(int coordinate, bool hasExteralAxis)
    {
        return new OperateResult<YRCRobotData>();
    }
}
