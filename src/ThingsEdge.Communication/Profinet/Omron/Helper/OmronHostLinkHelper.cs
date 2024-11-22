using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// 欧姆龙的OmronHostLink相关辅助方法
/// </summary>
public static class OmronHostLinkHelper
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
                    return new OperateResult<byte[]>($"Send Command [{@string}] not the same as receive command [{string2}] source:[{response.ToAsciiString()}]");
                }

                int num;
                try
                {
                    num = Convert.ToInt32(Encoding.ASCII.GetString(response, 19, 4), 16);
                }
                catch (Exception ex)
                {
                    return new OperateResult<byte[]>($"Get error code failed: {ex.Message}{Environment.NewLine}Source Data: {response.ToAsciiString()}");
                }

                byte[] array = [];
                if (response.Length > 27)
                {
                    array = Encoding.ASCII.GetString(response, 23, response.Length - 27).ToHexBytes();
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
                    var array2 = array.Length != 0 ? new byte[array.Length * 2 / 3] : [];
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

    /// <remarks>
    /// 如果需要需要额外指定站号的话，在第一个地址里，使用 s=2;D100 这种携带地址的功能
    /// </remarks>
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
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i])).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            contentArray.AddRange(read.Content);
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    public static async Task<OperateResult> WriteAsync(IHostLink hostLink, string address, byte[] value)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, value, isBit: false);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    public static async Task<OperateResult<byte[]>> ReadAsync(IHostLink hostLink, string[] address)
    {
        var station = hostLink.UnitNumber;
        if (address.Length != 0)
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
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i])).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            contentArray.AddRange(read.Content);
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

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
            var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content[i])).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            contentArray.AddRange(read.Content.Select((m) => m != 0));
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    public static async Task<OperateResult> WriteAsync(IHostLink hostLink, string address, bool[] values)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", hostLink.UnitNumber);
        var command = OmronFinsNetHelper.BuildWriteWordCommand(hostLink.PlcType, address, values.Select((m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
        if (!command.IsSuccess)
        {
            return command;
        }

        var read = await hostLink.ReadFromCoreServerAsync(PackCommand(hostLink, station, command.Content)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 根据错误信息获取当前的文本描述信息
    /// </summary>
    /// <param name="error">错误代号</param>
    /// <returns>文本消息</returns>
    private static string GetErrorText(int error)
    {
        return error switch
        {
            1 => "Service was canceled.",
            257 => "Local node is not participating in the network.",
            258 => "Token does not arrive.",
            259 => "Send was not possible during the specified number of retries.",
            260 => "Cannot send because maximum number of event frames exceeded.",
            261 => "Node address setting error occurred.",
            262 => "The same node address has been set twice in the same network.",
            513 => "The destination node is not in the network.",
            514 => "There is no Unit with the specified unit address.",
            515 => "The third node does not exist.",
            516 => "The destination node is busy.",
            517 => "The message was destroyed by noise",
            769 => "An error occurred in the communications controller.",
            770 => "A CPU error occurred in the destination CPU Unit.",
            771 => "A response was not returned because an error occurred in the Board.",
            772 => "The unit number was set incorrectly",
            1025 => "The Unit/Board does not support the specified command code.",
            1026 => "The command cannot be executed because the model or version is incorrect",
            1281 => "The destination network or node address is not set in the routing tables.",
            1282 => "Relaying is not possible because there are no routing tables",
            1283 => "There is an error in the routing tables.",
            1284 => "An attempt was made to send to a network that was over 3 networks away",
            4097 => "The command is longer than the maximum permissible length.",
            4098 => "The command is shorter than the minimum permissible length.",
            4099 => "The designated number of elements differs from the number of write data items.",
            4100 => "An incorrect format was used.",
            4101 => "Either the relay table in the local node or the local network table in the relay node is incorrect.",
            4353 => "The specified word does not exist in the memory area or there is no EM Area.",
            4354 => "The access size specification is incorrect or an odd word address is specified.",
            4355 => "The start address in command process is beyond the accessible area",
            4356 => "The end address in command process is beyond the accessible area.",
            4358 => "FFFF hex was not specified.",
            4361 => "A large–small relationship in the elements in the command data is incorrect.",
            4363 => "The response format is longer than the maximum permissible length.",
            4364 => "There is an error in one of the parameter settings.",
            8194 => "The program area is protected.",
            8195 => "A table has not been registered.",
            8196 => "The search data does not exist.",
            8197 => "A non-existing program number has been specified.",
            8198 => "The file does not exist at the specified file device.",
            8199 => "A data being compared is not the same.",
            8449 => "The specified area is read-only.",
            8450 => "The program area is protected.",
            8451 => "The file cannot be created because the limit has been exceeded.",
            8453 => "A non-existing program number has been specified.",
            8454 => "The file does not exist at the specified file device.",
            8455 => "A file with the same name already exists in the specified file device.",
            8456 => "The change cannot be made because doing so would create a problem.",
            8705 or 8706 or 8712 => "The mode is incorrect.",
            8707 => "The PLC is in PROGRAM mode.",
            8708 => "The PLC is in DEBUG mode.",
            8709 => "The PLC is in MONITOR mode.",
            8710 => "The PLC is in RUN mode.",
            8711 => "The specified node is not the polling node.",
            8961 => "The specified memory does not exist as a file device.",
            8962 => "There is no file memory.",
            8963 => "There is no clock.",
            9217 => "The data link tables have not been registered or they contain an error.",
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 将 fins 命令的报文打包成 HostLink 格式的报文信息，打包之后的结果可以直接发送给PLC。
    /// </summary>
    /// <param name="hostLink">HostLink协议的plc通信对象</param>
    /// <param name="station">站号信息</param>
    /// <param name="cmd">fins命令</param>
    /// <returns>可发送PLC的完整的报文信息</returns>
    private static byte[] PackCommand(IHostLink hostLink, byte station, byte[] cmd)
    {
        var cmd2 = cmd.ToAsciiBytes();
        var array = new byte[18 + cmd2.Length];
        array[0] = 64;
        array[1] = ByteExtensions.BuildAsciiBytesFrom(station)[0];
        array[2] = ByteExtensions.BuildAsciiBytesFrom(station)[1];
        array[3] = 70;
        array[4] = 65;
        array[5] = hostLink.ResponseWaitTime;
        array[6] = ByteExtensions.BuildAsciiBytesFrom(hostLink.ICF)[0];
        array[7] = ByteExtensions.BuildAsciiBytesFrom(hostLink.ICF)[1];
        array[8] = ByteExtensions.BuildAsciiBytesFrom(hostLink.DA2)[0];
        array[9] = ByteExtensions.BuildAsciiBytesFrom(hostLink.DA2)[1];
        array[10] = ByteExtensions.BuildAsciiBytesFrom(hostLink.SA2)[0];
        array[11] = ByteExtensions.BuildAsciiBytesFrom(hostLink.SA2)[1];
        array[12] = ByteExtensions.BuildAsciiBytesFrom(hostLink.SID)[0];
        array[13] = ByteExtensions.BuildAsciiBytesFrom(hostLink.SID)[1];
        array[^2] = 42;
        array[^1] = 13;
        cmd2.CopyTo(array, 14);
        int num = array[0];
        for (var i = 1; i < array.Length - 4; i++)
        {
            num ^= array[i];
        }
        array[^4] = ByteExtensions.BuildAsciiBytesFrom((byte)num)[0];
        array[^3] = ByteExtensions.BuildAsciiBytesFrom((byte)num)[1];
        return array;
    }
}
