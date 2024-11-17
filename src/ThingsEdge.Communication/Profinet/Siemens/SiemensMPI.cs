using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <summary>
/// 西门子的MPI协议信息，注意：未测试通过，无法使用<br />
/// Siemens MPI protocol information, note: it has not passed the test and cannot be used
/// </summary>
public class SiemensMPI : DeviceSerialPort
{
    private byte station = 2;

    private byte[] readConfirm = new byte[15]
    {
        104, 8, 8, 104, 130, 128, 92, 22, 2, 176,
        7, 0, 45, 22, 229
    };

    private byte[] writeConfirm = new byte[15]
    {
        104, 8, 8, 104, 130, 128, 124, 22, 2, 176,
        7, 0, 77, 22, 229
    };

    /// <summary>
    /// 西门子PLC的站号信息<br />
    /// Siemens PLC station number information
    /// </summary>
    public byte Station
    {
        get
        {
            return station;
        }
        set
        {
            station = value;
            readConfirm[4] = (byte)(value + 128);
            writeConfirm[4] = (byte)(value + 128);
            var num = 0;
            var num2 = 0;
            for (var i = 4; i < 12; i++)
            {
                num += readConfirm[i];
                num2 += writeConfirm[i];
            }
            readConfirm[12] = (byte)num;
            writeConfirm[12] = (byte)num2;
        }
    }

    /// <summary>
    /// 实例化一个西门子的MPI协议对象<br />
    /// Instantiate a Siemens MPI protocol object
    /// </summary>
    public SiemensMPI()
    {
        ByteTransform = new ReverseBytesTransform();
        WordLength = 2;
    }

    /// <summary>
    /// 与PLC进行握手<br />
    /// Handshake with PLC
    /// </summary>
    /// <returns>是否握手成功</returns>
    public OperateResult Handle()
    {
        while (true)
        {
            var operateResult = CommunicationPipe.ReceiveMessage(null, null);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }
            if (operateResult.Content[0] == 220 && operateResult.Content[1] == 2 && operateResult.Content[2] == 2)
            {
                var operateResult2 = CommunicationPipe.Send(new byte[3] { 220, 0, 0 });
                if (!operateResult2.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(operateResult2);
                }
            }
            else if (operateResult.Content[0] == 220 && operateResult.Content[1] == 0 && operateResult.Content[2] == 2)
            {
                break;
            }
        }
        var operateResult3 = CommunicationPipe.Send(new byte[3] { 220, 2, 0 });
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从西门子的PLC中读取数据信息，地址为"M100","AI100","I0","Q0","V100","S100"等，详细请参照API文档<br />
    /// Read data information from Siemens PLC, the address is "M100", "AI100", "I0", "Q0", "V100", "S100", etc., please refer to the API documentation
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>带返回结果的结果对象</returns>
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        var operateResult = BuildReadCommand(station, address, length, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        if (IsClearCacheBeforeRead)
        {
            ((PipeSerialPort)CommunicationPipe).ClearSerialCache();
        }
        var operateResult2 = CommunicationPipe.Send(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        var operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        if (operateResult3.Content[14] != 229)
        {
            return new OperateResult<byte[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult3.Content));
        }
        operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        if (operateResult3.Content[19] != 0)
        {
            return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult3.Content[19]);
        }
        operateResult2 = CommunicationPipe.Send(readConfirm);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        var array = new byte[length];
        if (operateResult3.Content[25] == byte.MaxValue && operateResult3.Content[26] == 4)
        {
            Array.Copy(operateResult3.Content, 29, array, 0, length);
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 从西门子的PLC中读取bool数据信息，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等，详细请参照API文档<br />
    /// Read the bool data information from Siemens PLC. The addresses are "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc. For details, please Refer to API documentation
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="length">数据长度</param>
    /// <returns>带返回结果的结果对象</returns>
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        var operateResult = BuildReadCommand(station, address, length, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var operateResult2 = CommunicationPipe.Send(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        var operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult3);
        }
        if (operateResult3.Content[14] != 229)
        {
            return new OperateResult<bool[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult3.Content));
        }
        operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult3);
        }
        if (operateResult3.Content[19] != 0)
        {
            return new OperateResult<bool[]>("PLC Receive Check Failed:" + operateResult3.Content[19]);
        }
        operateResult2 = CommunicationPipe.Send(readConfirm);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        var array = new byte[operateResult3.Content.Length - 31];
        if (operateResult3.Content[21] == byte.MaxValue && operateResult3.Content[22] == 3)
        {
            Array.Copy(operateResult3.Content, 28, array, 0, array.Length);
        }
        return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(array, length));
    }

    /// <summary>
    /// 将字节数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等，详细请参照API文档<br />
    /// Write byte data to Siemens PLC, the address is "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc. Refer to API documentation
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="value">数据长度</param>
    /// <returns>带返回结果的结果对象</returns>
    public override OperateResult Write(string address, byte[] value)
    {
        var operateResult = BuildWriteCommand(station, address, value);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        if (IsClearCacheBeforeRead)
        {
            ((PipeSerialPort)CommunicationPipe).ClearSerialCache();
        }
        var operateResult2 = CommunicationPipe.Send(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        var operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        if (operateResult3.Content[14] != 229)
        {
            return new OperateResult<byte[]>("PLC Receive Check Failed:" + SoftBasic.ByteToHexString(operateResult3.Content));
        }
        operateResult3 = CommunicationPipe.ReceiveMessage(null, null);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        if (operateResult3.Content[25] != byte.MaxValue)
        {
            return new OperateResult<byte[]>("PLC Receive Check Failed:" + operateResult3.Content[25]);
        }
        operateResult2 = CommunicationPipe.Send(writeConfirm);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从西门子的PLC中读取byte数据信息，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等，详细请参照API文档<br />
    /// Read byte data information from Siemens PLC. The addresses are "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc. For details, please Refer to API documentation
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <returns>带返回结果的结果对象</returns>
    public OperateResult<byte> ReadByte(string address)
    {
        return ByteTransformHelper.GetResultFromArray(Read(address, 1));
    }

    /// <summary>
    /// 将byte数据写入到西门子PLC中，地址为"M100.0","AI100.1","I0.3","Q0.6","V100.4","S100"等，详细请参照API文档<br />
    /// Write byte data to Siemens PLC, the address is "M100.0", "AI100.1", "I0.3", "Q0.6", "V100.4", "S100", etc. API documentation
    /// </summary>
    /// <param name="address">西门子的地址数据信息</param>
    /// <param name="value">数据长度</param>
    /// <returns>带返回结果的结果对象</returns>
    public OperateResult Write(string address, byte value)
    {
        return Write(address, new byte[1] { value });
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SiemensMPI[{PortName}:{BaudRate}]";
    }

    /// <summary>
    /// 生成一个读取字数据指令头的通用方法<br />
    /// A general method for generating a command header to read a Word data
    /// </summary>
    /// <param name="station">设备的站号信息 -&gt; Station number information for the device</param>
    /// <param name="address">起始地址，例如M100，I0，Q0，V100 -&gt;
    /// Start address, such as M100,I0,Q0,V100</param>
    /// <param name="length">读取数据长度 -&gt; Read Data length</param>
    /// <param name="isBit">是否为位读取</param>
    /// <returns>包含结果对象的报文 -&gt; Message containing the result object</returns>
    public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length, bool isBit)
    {
        var operateResult = S7AddressData.ParseFrom(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var array = new byte[38];
        array[0] = 104;
        array[1] = BitConverter.GetBytes(array.Length - 6)[0];
        array[2] = BitConverter.GetBytes(array.Length - 6)[0];
        array[3] = 104;
        array[4] = (byte)(station + 128);
        array[5] = 128;
        array[6] = 124;
        array[7] = 22;
        array[8] = 1;
        array[9] = 241;
        array[10] = 0;
        array[11] = 50;
        array[12] = 1;
        array[13] = 0;
        array[14] = 0;
        array[15] = 51;
        array[16] = 2;
        array[17] = 0;
        array[18] = 14;
        array[19] = 0;
        array[20] = 0;
        array[21] = 4;
        array[22] = 1;
        array[23] = 18;
        array[24] = 10;
        array[25] = 16;
        array[26] = (byte)(isBit ? 1 : 2);
        array[27] = BitConverter.GetBytes(length)[1];
        array[28] = BitConverter.GetBytes(length)[0];
        array[29] = BitConverter.GetBytes(operateResult.Content.DbBlock)[1];
        array[30] = BitConverter.GetBytes(operateResult.Content.DbBlock)[0];
        array[31] = operateResult.Content.DataCode;
        array[32] = BitConverter.GetBytes(operateResult.Content.AddressStart)[2];
        array[33] = BitConverter.GetBytes(operateResult.Content.AddressStart)[1];
        array[34] = BitConverter.GetBytes(operateResult.Content.AddressStart)[0];
        var num = 0;
        for (var i = 4; i < 35; i++)
        {
            num += array[i];
        }
        array[35] = BitConverter.GetBytes(num)[0];
        array[36] = 22;
        array[37] = 229;
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 生成一个写入PLC数据信息的报文内容<br />
    /// Generate a message content to write PLC data information
    /// </summary>
    /// <param name="station">PLC的站号</param>
    /// <param name="address">地址</param>
    /// <param name="values">数据值</param>
    /// <returns>是否写入成功</returns>
    public static OperateResult<byte[]> BuildWriteCommand(byte station, string address, byte[] values)
    {
        var operateResult = S7AddressData.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var num = values.Length;
        var array = new byte[42 + values.Length];
        array[0] = 104;
        array[1] = BitConverter.GetBytes(array.Length - 6)[0];
        array[2] = BitConverter.GetBytes(array.Length - 6)[0];
        array[3] = 104;
        array[4] = (byte)(station + 128);
        array[5] = 128;
        array[6] = 92;
        array[7] = 22;
        array[8] = 2;
        array[9] = 241;
        array[10] = 0;
        array[11] = 50;
        array[12] = 1;
        array[13] = 0;
        array[14] = 0;
        array[15] = 67;
        array[16] = 2;
        array[17] = 0;
        array[18] = 14;
        array[19] = 0;
        array[20] = (byte)(values.Length + 4);
        array[21] = 5;
        array[22] = 1;
        array[23] = 18;
        array[24] = 10;
        array[25] = 16;
        array[26] = 2;
        array[27] = BitConverter.GetBytes(num)[0];
        array[28] = BitConverter.GetBytes(num)[1];
        array[29] = BitConverter.GetBytes(operateResult.Content.DbBlock)[0];
        array[30] = BitConverter.GetBytes(operateResult.Content.DbBlock)[1];
        array[31] = operateResult.Content.DataCode;
        array[32] = BitConverter.GetBytes(operateResult.Content.AddressStart)[2];
        array[33] = BitConverter.GetBytes(operateResult.Content.AddressStart)[1];
        array[34] = BitConverter.GetBytes(operateResult.Content.AddressStart)[0];
        array[35] = 0;
        array[36] = 4;
        array[37] = BitConverter.GetBytes(num * 8)[1];
        array[38] = BitConverter.GetBytes(num * 8)[0];
        values.CopyTo(array, 39);
        var num2 = 0;
        for (var i = 4; i < array.Length - 3; i++)
        {
            num2 += array[i];
        }
        array[array.Length - 3] = BitConverter.GetBytes(num2)[0];
        array[array.Length - 2] = 22;
        array[array.Length - 1] = 229;
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 根据错误信息，获取到文本信息
    /// </summary>
    /// <param name="code">状态</param>
    /// <returns>消息文本</returns>
    public static string GetMsgFromStatus(byte code)
    {
        return code switch
        {
            byte.MaxValue => "No error",
            1 => "Hardware fault",
            3 => "Illegal object access",
            5 => "Invalid address(incorrent variable address)",
            6 => "Data type is not supported",
            10 => "Object does not exist or length error",
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 根据错误信息，获取到文本信息
    /// </summary>
    /// <param name="errorClass">错误类型</param>
    /// <param name="errorCode">错误代码</param>
    /// <returns>错误信息</returns>
    public static string GetMsgFromStatus(byte errorClass, byte errorCode)
    {
        if (errorClass == 128 && errorCode == 1)
        {
            return "Switch\u2002in\u2002wrong\u2002position\u2002for\u2002requested\u2002operation";
        }
        if (errorClass == 129 && errorCode == 4)
        {
            return "Miscellaneous\u2002structure\u2002error\u2002in\u2002command.\u2002\u2002Command is not supportedby CPU";
        }
        if (errorClass == 132 && errorCode == 4)
        {
            return "CPU is busy processing an upload or download CPU cannot process command because of system fault condition";
        }
        if (errorClass == 133 && errorCode == 0)
        {
            return "Length fields are not correct or do not agree with the amount of data received";
        }
        switch (errorClass)
        {
            case 210:
                return "Error in upload or download command";
            case 214:
                return "Protection error(password)";
            case 220:
                if (errorCode == 1)
                {
                    return "Error in time-of-day clock data";
                }
                break;
        }
        return StringResources.Language.UnknownError;
    }
}
