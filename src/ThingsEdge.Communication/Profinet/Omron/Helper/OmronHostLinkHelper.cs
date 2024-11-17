using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// 欧姆龙的OmronHostLink相关辅助方法
/// </summary>
public class OmronHostLinkHelper
{
    /// <summary>
    /// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容
    /// </summary>
    /// <param name="send">发送的报文信息</param>
    /// <param name="response">来自欧姆龙返回的数据内容</param>
    /// <returns>带有是否成功的结果对象</returns>
    public static OperateResult<byte[]> ResponseValidAnalysis(byte[] send, byte[] response)
    {
        if (response.Length >= 27)
        {
            try
            {
                var @string = Encoding.ASCII.GetString(send, 14, 4);
                var string2 = Encoding.ASCII.GetString(response, 15, 4);
                if (string2 != @string)
                {
                    return new OperateResult<byte[]>("Send Command [" + @string + "] not the same as receive command [" + string2 + "] source:[" + SoftBasic.GetAsciiStringRender(response) + "]");
                }
                int num;
                try
                {
                    num = Convert.ToInt32(Encoding.ASCII.GetString(response, 19, 4), 16);
                }
                catch (Exception ex)
                {
                    return new OperateResult<byte[]>("Get error code failed: " + ex.Message + Environment.NewLine + "Source Data: " + SoftBasic.GetAsciiStringRender(response));
                }
                var array = new byte[0];
                if (response.Length > 27)
                {
                    array = SoftBasic.HexStringToBytes(Encoding.ASCII.GetString(response, 23, response.Length - 27));
                }
                if (num > 0)
                {
                    return new OperateResult<byte[]>
                    {
                        ErrorCode = num,
                        Content = array,
                        Message = GetErrorText(num)
                    };
                }
                if (Encoding.ASCII.GetString(response, 15, 4) == "0104")
                {
                    var array2 = array.Length != 0 ? new byte[array.Length * 2 / 3] : new byte[0];
                    for (var i = 0; i < array.Length / 3; i++)
                    {
                        array2[i * 2] = array[i * 3 + 1];
                        array2[i * 2 + 1] = array[i * 3 + 2];
                    }
                    array = array2;
                }
                return OperateResult.CreateSuccessResult(array);
            }
            catch (Exception ex2)
            {
                return new OperateResult<byte[]>("ResponseValidAnalysis failed: " + ex2.Message + " Source: " + response.ToHexString(' '));
            }
        }
        return new OperateResult<byte[]>(StringResources.Language.OmronReceiveDataError + " Source Data: " + response.ToHexString(' '));
    }

    /// <summary>
    /// 根据错误信息获取当前的文本描述信息
    /// </summary>
    /// <param name="error">错误代号</param>
    /// <returns>文本消息</returns>
    public static string GetErrorText(int error)
    {
        switch (error)
        {
            case 1:
                return "Service was canceled.";
            case 257:
                return "Local node is not participating in the network.";
            case 258:
                return "Token does not arrive.";
            case 259:
                return "Send was not possible during the specified number of retries.";
            case 260:
                return "Cannot send because maximum number of event frames exceeded.";
            case 261:
                return "Node address setting error occurred.";
            case 262:
                return "The same node address has been set twice in the same network.";
            case 513:
                return "The destination node is not in the network.";
            case 514:
                return "There is no Unit with the specified unit address.";
            case 515:
                return "The third node does not exist.";
            case 516:
                return "The destination node is busy.";
            case 517:
                return "The message was destroyed by noise";
            case 769:
                return "An error occurred in the communications controller.";
            case 770:
                return "A CPU error occurred in the destination CPU Unit.";
            case 771:
                return "A response was not returned because an error occurred in the Board.";
            case 772:
                return "The unit number was set incorrectly";
            case 1025:
                return "The Unit/Board does not support the specified command code.";
            case 1026:
                return "The command cannot be executed because the model or version is incorrect";
            case 1281:
                return "The destination network or node address is not set in the routing tables.";
            case 1282:
                return "Relaying is not possible because there are no routing tables";
            case 1283:
                return "There is an error in the routing tables.";
            case 1284:
                return "An attempt was made to send to a network that was over 3 networks away";
            case 4097:
                return "The command is longer than the maximum permissible length.";
            case 4098:
                return "The command is shorter than the minimum permissible length.";
            case 4099:
                return "The designated number of elements differs from the number of write data items.";
            case 4100:
                return "An incorrect format was used.";
            case 4101:
                return "Either the relay table in the local node or the local network table in the relay node is incorrect.";
            case 4353:
                return "The specified word does not exist in the memory area or there is no EM Area.";
            case 4354:
                return "The access size specification is incorrect or an odd word address is specified.";
            case 4355:
                return "The start address in command process is beyond the accessible area";
            case 4356:
                return "The end address in command process is beyond the accessible area.";
            case 4358:
                return "FFFF hex was not specified.";
            case 4361:
                return "A large–small relationship in the elements in the command data is incorrect.";
            case 4363:
                return "The response format is longer than the maximum permissible length.";
            case 4364:
                return "There is an error in one of the parameter settings.";
            case 8194:
                return "The program area is protected.";
            case 8195:
                return "A table has not been registered.";
            case 8196:
                return "The search data does not exist.";
            case 8197:
                return "A non-existing program number has been specified.";
            case 8198:
                return "The file does not exist at the specified file device.";
            case 8199:
                return "A data being compared is not the same.";
            case 8449:
                return "The specified area is read-only.";
            case 8450:
                return "The program area is protected.";
            case 8451:
                return "The file cannot be created because the limit has been exceeded.";
            case 8453:
                return "A non-existing program number has been specified.";
            case 8454:
                return "The file does not exist at the specified file device.";
            case 8455:
                return "A file with the same name already exists in the specified file device.";
            case 8456:
                return "The change cannot be made because doing so would create a problem.";
            case 8705:
            case 8706:
            case 8712:
                return "The mode is incorrect.";
            case 8707:
                return "The PLC is in PROGRAM mode.";
            case 8708:
                return "The PLC is in DEBUG mode.";
            case 8709:
                return "The PLC is in MONITOR mode.";
            case 8710:
                return "The PLC is in RUN mode.";
            case 8711:
                return "The specified node is not the polling node.";
            case 8961:
                return "The specified memory does not exist as a file device.";
            case 8962:
                return "There is no file memory.";
            case 8963:
                return "There is no clock.";
            case 9217:
                return "The data link tables have not been registered or they contain an error.";
            default:
                return StringResources.Language.UnknownError;
        }
    }

    /// <summary>
    /// 将 fins 命令的报文打包成 HostLink 格式的报文信息，打包之后的结果可以直接发送给PLC<br />
    /// Pack the message of the fins command into the message information in the HostLink format, and the packaged result can be sent directly to the PLC
    /// </summary>
    /// <param name="hostLink">HostLink协议的plc通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="cmd">fins命令</param>
    /// <returns>可发送PLC的完整的报文信息</returns>
    public static byte[] PackCommand(IHostLink hostLink, byte station, byte[] cmd)
    {
        cmd = SoftBasic.BytesToAsciiBytes(cmd);
        var array = new byte[18 + cmd.Length];
        array[0] = 64;
        array[1] = SoftBasic.BuildAsciiBytesFrom(station)[0];
        array[2] = SoftBasic.BuildAsciiBytesFrom(station)[1];
        array[3] = 70;
        array[4] = 65;
        array[5] = hostLink.ResponseWaitTime;
        array[6] = SoftBasic.BuildAsciiBytesFrom(hostLink.ICF)[0];
        array[7] = SoftBasic.BuildAsciiBytesFrom(hostLink.ICF)[1];
        array[8] = SoftBasic.BuildAsciiBytesFrom(hostLink.DA2)[0];
        array[9] = SoftBasic.BuildAsciiBytesFrom(hostLink.DA2)[1];
        array[10] = SoftBasic.BuildAsciiBytesFrom(hostLink.SA2)[0];
        array[11] = SoftBasic.BuildAsciiBytesFrom(hostLink.SA2)[1];
        array[12] = SoftBasic.BuildAsciiBytesFrom(hostLink.SID)[0];
        array[13] = SoftBasic.BuildAsciiBytesFrom(hostLink.SID)[1];
        array[array.Length - 2] = 42;
        array[array.Length - 1] = 13;
        cmd.CopyTo(array, 14);
        int num = array[0];
        for (var i = 1; i < array.Length - 4; i++)
        {
            num ^= array[i];
        }
        array[array.Length - 4] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
        array[array.Length - 3] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
        return array;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    public static OperateResult<byte[]> Read(IHostLink hostLink, string address, ushort length)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var operateResult = OmronFinsNetHelper.BuildReadCommand(hostLink.PlcType, address, length, isBit: false, hostLink.ReadSplits);
        if (!operateResult.IsSuccess)
        {
            return operateResult.ConvertFailed<byte[]>();
        }
        var list = new List<byte>();
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = hostLink.ReadFromCoreServer(PackCommand(hostLink, station, operateResult.Content[i]));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult2);
            }
            list.AddRange(operateResult2.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNetHelper.Read(HslCommunication.Profinet.Omron.Helper.IOmronFins,System.String[])" />
    /// <remarks>
    /// 如果需要需要额外指定站号的话，在第一个地址里，使用 s=2;D100 这种携带地址的功能
    /// </remarks>
    public static OperateResult<byte[]> Read(IHostLink hostLink, string[] address)
    {
        var station = hostLink.UnitNumber;
        if (address != null && address.Length != 0)
        {
            station = (byte)CommHelper.ExtractParameter(ref address[0], "s", hostLink.UnitNumber);
        }
        var operateResult = OmronFinsNetHelper.BuildReadCommand(address, hostLink.PlcType);
        if (!operateResult.IsSuccess)
        {
            return operateResult.ConvertFailed<byte[]>();
        }
        var list = new List<byte>();
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = hostLink.ReadFromCoreServer(PackCommand(hostLink, station, operateResult.Content[i]));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult2);
            }
            list.AddRange(operateResult2.Content);
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    public static OperateResult Write(IHostLink hostLink, string address, byte[] value)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var operateResult = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, value, isBit: false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = hostLink.ReadFromCoreServer(PackCommand(hostLink, station, operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Read(System.String,System.UInt16)" />
    public static async Task<OperateResult<byte[]>> ReadAsync(IHostLink hostLink, string address, ushort length)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildReadCommand(hostLink.PlcType, address, length, isBit: false, hostLink.ReadSplits);
        if (!command.IsSuccess)
        {
            return command.ConvertFailed<byte[]>();
        }
        var contentArray = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i]));
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            contentArray.AddRange(read.Content);
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Byte[])" />
    public static async Task<OperateResult> WriteAsync(IHostLink hostLink, string address, byte[] value)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, value, isBit: false);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.Helper.OmronHostLinkHelper.Read(HslCommunication.Profinet.Omron.Helper.IHostLink,System.String[])" />
    public static async Task<OperateResult<byte[]>> ReadAsync(IHostLink hostLink, string[] address)
    {
        var station = hostLink.UnitNumber;
        if (address != null && address.Length != 0)
        {
            station = (byte)CommHelper.ExtractParameter(ref address[0], "s", hostLink.UnitNumber);
        }
        var command = OmronFinsNetHelper.BuildReadCommand(address, hostLink.PlcType);
        if (!command.IsSuccess)
        {
            return command.ConvertFailed<byte[]>();
        }
        var contentArray = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i]));
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            contentArray.AddRange(read.Content);
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    public static OperateResult<bool[]> ReadBool(IHostLink hostLink, string address, ushort length)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var operateResult = OmronFinsNetHelper.BuildReadCommand(hostLink.PlcType, address, length, isBit: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult);
        }
        var list = new List<bool>();
        for (var i = 0; i < operateResult.Content.Count; i++)
        {
            OperateResult<byte[]> operateResult2 = hostLink.ReadFromCoreServer(PackCommand(hostLink, station, operateResult.Content[i]));
            if (!operateResult2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult2);
            }
            if (operateResult2.Content.Length == 0)
            {
                return new OperateResult<bool[]>("Data is empty.");
            }
            list.AddRange(operateResult2.Content.Select((m) => m != 0));
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    public static OperateResult Write(IHostLink hostLink, string address, bool[] values)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var operateResult = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, values.Select((m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        OperateResult<byte[]> operateResult2 = hostLink.ReadFromCoreServer(PackCommand(hostLink, station, operateResult.Content));
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.ReadBool(System.String,System.UInt16)" />
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IHostLink hostLink, string address, ushort length)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildReadCommand(hostLink.PlcType, address, length, isBit: true);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var contentArray = new List<bool>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i]));
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            contentArray.AddRange(read.Content.Select((m) => m != 0));
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronFinsNet.Write(System.String,System.Boolean[])" />
    public static async Task<OperateResult> WriteAsync(IHostLink hostLink, string address, bool[] values)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, values.Select((m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content));
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }
}
