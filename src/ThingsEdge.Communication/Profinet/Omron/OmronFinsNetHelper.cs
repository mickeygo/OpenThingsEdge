using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Profinet.Omron.Helper;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// Omron PLC的FINS协议相关的辅助类，主要是一些地址解析，读写的指令生成。
/// </summary>
public static class OmronFinsNetHelper
{
    /// <summary>
    /// 同时读取多个地址的命令报文信息
    /// </summary>
    /// <param name="address">多个地址</param>
    /// <param name="plcType">PLC的类型信息</param>
    /// <returns>命令报文信息</returns>
    public static OperateResult<List<byte[]>> BuildReadCommand(string[] address, OmronPlcType plcType)
    {
        var list = new List<byte[]>();
        var list2 = SoftBasic.ArraySplitByLength(address, 89);
        for (var i = 0; i < list2.Count; i++)
        {
            var array = list2[i];
            var array2 = new byte[2 + 4 * array.Length];
            array2[0] = 1;
            array2[1] = 4;
            for (var j = 0; j < array.Length; j++)
            {
                var operateResult = OmronFinsAddress.ParseFrom(array[j], 1, plcType);
                if (!operateResult.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
                }
                array2[2 + 4 * j] = operateResult.Content.WordCode;
                array2[3 + 4 * j] = (byte)(operateResult.Content.AddressStart / 16 / 256);
                array2[4 + 4 * j] = (byte)(operateResult.Content.AddressStart / 16 % 256);
                array2[5 + 4 * j] = (byte)(operateResult.Content.AddressStart % 16);
            }
            list.Add(array2);
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
    /// </summary>
    /// <param name="plcType">PLC的类型信息</param>
    /// <param name="address">地址，具体格式请参照示例说明</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="isBit">是否使用位读取</param>
    /// <param name="splitLength">读取的长度切割，默认500</param>
    /// <returns>带有成功标识的Fins核心报文</returns>
    public static OperateResult<List<byte[]>> BuildReadCommand(OmronPlcType plcType, string address, ushort length, bool isBit, int splitLength = 500)
    {
        var operateResult = OmronFinsAddress.ParseFrom(address, length, plcType);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var list = new List<byte[]>();
        var array = SoftBasic.SplitIntegerToArray(length, isBit ? 1998 : splitLength);
        for (var i = 0; i < array.Length; i++)
        {
            list.Add(BuildReadCommand(operateResult.Content, (ushort)array[i], isBit));
            operateResult.Content.AddressStart += isBit ? array[i] : array[i] * 16;
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
    /// </summary>
    /// <param name="address">地址，具体格式请参照示例说明</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="isBit">是否使用位读取</param>
    /// <returns>带有成功标识的Fins核心报文</returns>
    public static byte[] BuildReadCommand(OmronFinsAddress address, ushort length, bool isBit)
    {
        var array = new byte[8] { 1, 1, 0, 0, 0, 0, 0, 0 };
        if (isBit)
        {
            array[2] = address.BitCode;
        }
        else
        {
            array[2] = address.WordCode;
        }
        array[3] = (byte)(address.AddressStart / 16 / 256);
        array[4] = (byte)(address.AddressStart / 16 % 256);
        array[5] = (byte)(address.AddressStart % 16);
        array[6] = (byte)(length / 256);
        array[7] = (byte)(length % 256);
        return array;
    }

    /// <summary>
    /// 根据写入的地址，数据，是否位写入生成Fins协议的核心报文。
    /// </summary>
    /// <param name="plcType">PLC的类型信息</param>
    /// <param name="address">地址内容，具体格式请参照示例说明</param>
    /// <param name="value">实际的数据</param>
    /// <param name="isBit">是否位数据</param>
    /// <returns>带有成功标识的Fins核心报文</returns>
    public static OperateResult<byte[]> BuildWriteWordCommand(OmronPlcType plcType, string address, byte[] value, bool isBit)
    {
        var operateResult = OmronFinsAddress.ParseFrom(address, 0, plcType);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[8 + value.Length];
        array[0] = 1;
        array[1] = 2;
        array[2] = isBit ? operateResult.Content.BitCode : operateResult.Content.WordCode;
        array[3] = (byte)(operateResult.Content.AddressStart / 16 / 256);
        array[4] = (byte)(operateResult.Content.AddressStart / 16 % 256);
        array[5] = (byte)(operateResult.Content.AddressStart % 16);
        if (isBit)
        {
            array[6] = (byte)(value.Length / 256);
            array[7] = (byte)(value.Length % 256);
        }
        else
        {
            array[6] = (byte)(value.Length / 2 / 256);
            array[7] = (byte)(value.Length / 2 % 256);
        }
        value.CopyTo(array, 8);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容。
    /// </summary>
    /// <param name="response">来自欧姆龙返回的数据内容</param>
    /// <returns>带有是否成功的结果对象</returns>
    public static OperateResult<byte[]> ResponseValidAnalysis(byte[] response)
    {
        if (response.Length >= 16)
        {
            var num = BitConverter.ToInt32(
            [
                response[15],
                response[14],
                response[13],
                response[12]
            ], 0);
            if (num > 0)
            {
                return new OperateResult<byte[]>(num, GetStatusDescription(num));
            }
            return UdpResponseValidAnalysis(response.RemoveBegin(16));
        }
        return new OperateResult<byte[]>(StringResources.Language.OmronReceiveDataError);
    }

    /// <summary>
    /// 验证欧姆龙的Fins-Udp返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容。
    /// </summary>
    /// <param name="response">来自欧姆龙返回的数据内容</param>
    /// <returns>带有是否成功的结果对象</returns>
    public static OperateResult<byte[]> UdpResponseValidAnalysis(byte[] response)
    {
        if (response.Length >= 14)
        {
            var num = response[12] * 256 + response[13];
            if (response[12].GetBoolByIndex(7))
            {
                var mainCode = response[12] & 0x7F;
                var subCode = response[13] & 0x3F;
                return new OperateResult<byte[]>(num, GetEndCodeDescription(mainCode, subCode));
            }
            if (response[10] == 1 & response[11] == 1
                || response[10] == 1 & response[11] == 4
                || response[10] == 2 & response[11] == 1
                || response[10] == 3 & response[11] == 6
                || response[10] == 5 & response[11] == 1
                || response[10] == 5 & response[11] == 2
                || response[10] == 6 & response[11] == 1
                || response[10] == 6 & response[11] == 32
                || response[10] == 7 & response[11] == 1
                || response[10] == 9 & response[11] == 32
                || response[10] == 33 & response[11] == 2
                || response[10] == 34 & response[11] == 2)
            {
                try
                {
                    var array = new byte[response.Length - 14];
                    if (array.Length != 0)
                    {
                        Array.Copy(response, 14, array, 0, array.Length);
                    }
                    var operateResult = OperateResult.CreateSuccessResult(array);
                    if (array.Length == 0)
                    {
                        operateResult.IsSuccess = false;
                    }
                    operateResult.ErrorCode = num;
                    operateResult.Message = GetStatusDescription(num) + " Received:" + SoftBasic.ByteToHexString(response, ' ');
                    if (response[10] == 1 & response[11] == 4)
                    {
                        var array2 = array.Length != 0 ? new byte[array.Length * 2 / 3] : [];
                        for (var i = 0; i < array.Length / 3; i++)
                        {
                            array2[i * 2] = array[i * 3 + 1];
                            array2[i * 2 + 1] = array[i * 3 + 2];
                        }
                        operateResult.Content = array2;
                    }
                    return operateResult;
                }
                catch (Exception ex)
                {
                    return new OperateResult<byte[]>("UdpResponseValidAnalysis failed: " + ex.Message + Environment.NewLine + "Content: " + response.ToHexString(' '));
                }
            }
            var operateResult2 = OperateResult.CreateSuccessResult(Array.Empty<byte>());
            operateResult2.ErrorCode = num;
            operateResult2.Message = GetStatusDescription(num) + " Received:" + SoftBasic.ByteToHexString(response, ' ');
            return operateResult2;
        }
        return new OperateResult<byte[]>(StringResources.Language.OmronReceiveDataError);
    }

    private static string GetEndCodeDescription(int mainCode, int subCode)
    {
        switch (mainCode)
        {
            case 0:
                switch (subCode)
                {
                    case 0:
                        return "Normal completion";
                    case 1:
                        return "Data link status: Service was canceled";
                }
                break;
            case 1:
                switch (subCode)
                {
                    case 1:
                        return "Local node is not participating in the network.";
                    case 2:
                        return "Token does not arrive. [Set the local node to within the maximum node address.]";
                    case 3:
                        return "Send was not possible during the specified number of retries.";
                    case 4:
                        return "Cannot send because maximum number of event frames exceeded.";
                    case 5:
                        return "Node address setting error occurred";
                    case 6:
                        return "The same node address has been set twice in the same network";
                }
                break;
            case 2:
                switch (subCode)
                {
                    case 1:
                        return "The destination node is not in the network";
                    case 2:
                        return "There is no Unit with the specified unit address.";
                    case 3:
                        return "The third node does not exist";
                    case 4:
                        return "The destination node is busy";
                    case 5:
                        return "The message was destroyed by noise.";
                }
                break;
            case 3:
                switch (subCode)
                {
                    case 1:
                        return "An error occurred in the communications controller";
                    case 2:
                        return "A CPU error occurred in the destination CPU Unit.";
                    case 3:
                        return "A response was not returned because an error occurred in the Board";
                    case 4:
                        return "The unit number was set incorrectly";
                }
                break;
            case 4:
                switch (subCode)
                {
                    case 1:
                        return "The Unit/Board does not support the specified command code";
                    case 2:
                        return "The command cannot be executed because the model or version is incorrect";
                }
                break;
            case 5:
                switch (subCode)
                {
                    case 1:
                        return "The destination network or node address is not set in the routing tables";
                    case 2:
                        return "Relaying is not possible because there are no routing tables";
                    case 3:
                        return "There is an error in the routing tables";
                    case 4:
                        return "An attempt was made to send to a network that was over 3 networks away";
                }
                break;
            case 16:
                switch (subCode)
                {
                    case 1:
                        return "The command is longer than the maximum permissible length";
                    case 2:
                        return "The command is shorter than the minimum permissible length";
                    case 3:
                        return "The designated number of elements differs from the number of write data items";
                    case 4:
                        return "An incorrect format was used";
                    case 5:
                        return "Either the relay table in the local node or the local network table in the relay node is incorrect.";
                }
                break;
            case 17:
                switch (subCode)
                {
                    case 1:
                        return "The specified word does not exist in the memory area or there is no EM Area";
                    case 2:
                        return "The access size specification is incorrect or an odd word address is specified";
                    case 3:
                        return "The start address in command process is beyond the accessible area";
                    case 4:
                        return "The end address in command process is beyond the accessible area";
                    case 11:
                        return "The response format is longer than the maximum permissible length.";
                }
                break;
            case 32:
                switch (subCode)
                {
                    case 2:
                        return "The program area is protected";
                    case 4:
                        return "The search data does not exist.";
                    case 5:
                        return "A non-existing program number has been specified";
                }
                break;
            case 33:
                switch (subCode)
                {
                    case 1:
                        return "The specified area is read-only.";
                    case 2:
                        return "The program area is protected.";
                }
                break;
        }
        return StringResources.Language.UnknownError;
    }

    /// <summary>
    /// 根据欧姆龙返回的错误码，获取错误信息的字符串描述文本。
    /// </summary>
    /// <param name="err">错误码</param>
    /// <returns>文本描述</returns>
    public static string GetStatusDescription(int err)
    {
        return err switch
        {
            0 => StringResources.Language.OmronStatus0,
            1 => StringResources.Language.OmronStatus1,
            2 => StringResources.Language.OmronStatus2,
            3 => StringResources.Language.OmronStatus3,
            32 => StringResources.Language.OmronStatus20,
            33 => StringResources.Language.OmronStatus21,
            34 => StringResources.Language.OmronStatus22,
            35 => StringResources.Language.OmronStatus23,
            36 => StringResources.Language.OmronStatus24,
            37 => StringResources.Language.OmronStatus25,
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 从欧姆龙PLC中读取想要的数据，返回读取结果，读取长度的单位为字，地址格式为"D100","C100","W100","H100","A100。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <param name="address">读取地址，格式为"D100","C100","W100","H100","A100"</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="splits">分割信息</param>
    /// <returns>带成功标志的结果数据对象</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(IOmronFins omron, string address, ushort length, int splits)
    {
        var command = BuildReadCommand(omron.PlcType, address, length, isBit: false, splits);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }

        var contentArray = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await omron.ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            contentArray.AddRange(read.Content);
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    /// <summary>
    /// 向PLC写入数据，数据格式为原始的字节类型，地址格式为"D100","C100","W100","H100","A100"。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <param name="address">初始地址</param>
    /// <param name="value">原始的字节数据</param>
    /// <returns>结果</returns>
    public static async Task<OperateResult> WriteAsync(IOmronFins omron, string address, byte[] value)
    {
        var command = BuildWriteWordCommand(omron.PlcType, address, value, isBit: false);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await omron.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从欧姆龙PLC中读取多个地址的数据，返回读取结果，每个地址按照字为单位读取，地址格式为"D100","C100","W100","H100","A100"
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <param name="address">从欧姆龙PLC中读取多个地址的数据，返回读取结果，每个地址按照字为单位读取，地址格式为"D100","C100","W100","H100","A100"</param>
    /// <returns>带成功标志的结果数据对象</returns>
    public static async Task<OperateResult<byte[]>> ReadAsync(IOmronFins omron, string[] address)
    {
        var command = BuildReadCommand(address, omron.PlcType);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        return await omron.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
    }

    /// <summary>
    /// 从欧姆龙PLC中批量读取位软元件，地址格式为"D100.0","C100.0","W100.0","H100.0","A100.0"。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <param name="address">读取地址，格式为"D100","C100","W100","H100","A100"</param>
    /// <param name="length">读取的长度</param>
    /// <param name="splits">分割信息</param>
    /// <returns>带成功标志的结果数据对象</returns>
    public static async Task<OperateResult<bool[]>> ReadBoolAsync(IOmronFins omron, string address, ushort length, int splits)
    {
        var analysis = OmronFinsAddress.ParseFrom(address, length, omron.PlcType);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }
        var len = 0;
        var contentArray = new List<bool>();
        while (len < length)
        {
            var cmd = BuildReadCommand(analysis.Content, (ushort)(length - len), isBit: true);
            var read = await omron.ReadFromCoreServerAsync(cmd).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            if (read.Content.Length == 0)
            {
                return new OperateResult<bool[]>(read.ErrorCode, read.Message);
            }
            var array = read.Content.Select((m) => m != 0);
            contentArray.AddRange(array);
            len += array.Count();
            analysis.Content.AddressStart += array.Count();
        }
        return OperateResult.CreateSuccessResult(contentArray.ToArray());
    }

    /// <summary>
    /// 向PLC中位软元件写入bool数组，返回是否写入成功，比如你写入D100,values[0]对应D100.0，地址格式为"D100.0","C100.0","W100.0","H100.0","A100.0"。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <param name="address">要写入的数据地址</param>
    /// <param name="values">要写入的实际数据，可以指定任意的长度</param>
    /// <returns>返回写入结果</returns>
    public static async Task<OperateResult> WriteAsync(IOmronFins omron, string address, bool[] values)
    {
        if (omron.PlcType == OmronPlcType.CV && (address.StartsWithAndNumber("CIO") || address.StartsWithAndNumber("C")))
        {
            return await ReadWriteNetHelper.WriteBoolWithWordAsync(omron, address, values, 16, reverseWord: true).ConfigureAwait(continueOnCapturedContext: false);
        }

        var command = BuildWriteWordCommand(omron.PlcType, address, values.Select((m) => (byte)(m ? 1 : 0)).ToArray(), isBit: true);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await omron.ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 将CPU单元的操作模式更改为RUN，从而使PLC能够执行其程序。
    /// </summary>
    /// <remarks>
    /// 当执行RUN时，CPU单元将开始运行。 在执行RUN之前，您必须确认系统的安全性。 启用“禁止覆盖受保护程序”设置时，无法执行此命令。
    /// </remarks>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <returns>是否启动成功</returns>
    public static async Task<OperateResult> RunAsync(IReadWriteDevice omron)
    {
        return await omron.ReadFromCoreServerAsync([4, 1, 255, 255, 4]).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 将CPU单元的操作模式更改为PROGRAM，停止程序执行。
    /// </summary>
    /// <remarks>
    /// 当执行STOP时，CPU单元将停止操作。 在执行STOP之前，您必须确认系统的安全性。
    /// </remarks>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <returns>是否停止成功</returns>
    public static async Task<OperateResult> StopAsync(IReadWriteDevice omron)
    {
        return await omron.ReadFromCoreServerAsync([4, 2, 255, 255]).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取CPU的一些数据信息，主要包含型号，版本，一些数据块的大小。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <returns>是否读取成功</returns>
    public static async Task<OperateResult<OmronCpuUnitData>> ReadCpuUnitDataAsync(IReadWriteDevice omron)
    {
        return (await omron.ReadFromCoreServerAsync([5, 1, 0]).ConfigureAwait(continueOnCapturedContext: false))
            .Then((m) => OperateResult.CreateSuccessResult(new OmronCpuUnitData(m)));
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取CPU单元的一些操作状态数据，主要包含运行状态，工作模式，错误信息等。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <returns>是否读取成功</returns>
    public static async Task<OperateResult<OmronCpuUnitStatus>> ReadCpuUnitStatusAsync(IReadWriteDevice omron)
    {
        return (await omron.ReadFromCoreServerAsync([6, 1]).ConfigureAwait(continueOnCapturedContext: false))
            .Then((m) => OperateResult.CreateSuccessResult(new OmronCpuUnitStatus(m)));
    }

    /// <summary>
    /// 读取CPU的时间信息。
    /// </summary>
    /// <param name="omron">PLC设备的连接对象</param>
    /// <returns>是否读取成功</returns>
    public static async Task<OperateResult<DateTime>> ReadCpuTimeAsync(IReadWriteDevice omron)
    {
        return (await omron.ReadFromCoreServerAsync([7, 1]).ConfigureAwait(continueOnCapturedContext: false))
            .Then(CreatePlcTime);
    }

    private static OperateResult<DateTime> CreatePlcTime(byte[] buffer)
    {
        try
        {
            var text = buffer.ToHexString();
            var year = Convert.ToInt32(string.Concat(DateTime.Now.Year.ToString().AsSpan(0, 2), text.AsSpan(0, 2)));
            return OperateResult.CreateSuccessResult(new DateTime(
                year,
                Convert.ToInt32(text.Substring(2, 2)),
                Convert.ToInt32(text.Substring(4, 2)),
                Convert.ToInt32(text.Substring(6, 2)),
                Convert.ToInt32(text.Substring(8, 2)),
                Convert.ToInt32(text.Substring(10, 2))
                ));
        }
        catch (Exception ex)
        {
            return new OperateResult<DateTime>("Prase Time failed: " + ex.Message + Environment.NewLine + "Source: " + buffer.ToHexString(' '));
        }
    }
}
