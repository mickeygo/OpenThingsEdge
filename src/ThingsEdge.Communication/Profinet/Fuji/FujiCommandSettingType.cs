using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Fuji;

/// <summary>
/// 基于Command-Setting-Type通信模式的协议实现。
/// </summary>
/// <remarks>
/// 本类实例化对象之后，还需要设置 <see cref="DataSwap" /> 属性，根据实际情况来设置。
/// </remarks>
public class FujiCommandSettingType : DeviceTcpNet
{
    /// <summary>
    /// 获取当前的对象是否进行数据交换操作，将根据PLC的实际值来设定。
    /// </summary>
    public bool DataSwap { get; }

    /// <summary>
    /// 使用指定的IP地址和端口号来实例化一个对象
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号信息</param>
    /// <param name="dataSwap">当前的对象是否进行数据交换操作，将根据PLC的实际值来设定</param>
    public FujiCommandSettingType(string ipAddress, int port, bool dataSwap = false) : base(ipAddress, port)
    {
        DataSwap = dataSwap;

        WordLength = 2;
        ByteTransform = dataSwap ? new RegularByteTransform() : new ReverseBytesTransform();
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new FujiCommandSettingTypeMessage();
    }

    /// <inheritdoc />
    protected override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        return UnpackResponseContentHelper(response);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var bulid = BuildReadCommand(address, length);
        if (!bulid.IsSuccess)
        {
            return bulid;
        }
        return await ReadFromCoreServerAsync(bulid.Content).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        var bulid = BuildWriteCommand(address, data);
        if (!bulid.IsSuccess)
        {
            return bulid;
        }
        return await ReadFromCoreServerAsync(bulid.Content).ConfigureAwait(false);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        // TODO: [NotImplemented] FujiCommandSettingType -> ReadBoolAsync

        throw new NotImplementedException();
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] FujiCommandSettingType -> WriteAsync

        throw new NotImplementedException();
    }

    /// <summary>
    /// 读取指定地址的byte数据，地址格式 S100。
    /// </summary>
    /// <param name="address">起始地址，格式为S100 </param>
    /// <returns>是否读取成功的结果对象</returns>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }

    /// <summary>
    /// 向PLC中写入byte数据，返回值说明。
    /// </summary>
    /// <param name="address">起始地址，格式为 S100</param>
    /// <param name="value">byte数据</param>
    /// <returns>是否写入成功的结果对象 </returns>
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <summary>
    /// 构建读取的报文指令
    /// </summary>
    /// <param name="address">PLC的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <returns>报文构建的结果对象</returns>
    private static OperateResult<byte[]> BuildReadCommand(string address, ushort length)
    {
        var operateResult = FujiCommandSettingTypeAddress.ParseFrom(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        return OperateResult.CreateSuccessResult(new byte[9]
        {
            0,
            0,
            0,
            operateResult.Content.DataCode,
            4,
            BitConverter.GetBytes(operateResult.Content.AddressStart)[0],
            BitConverter.GetBytes(operateResult.Content.AddressStart)[1],
            BitConverter.GetBytes(operateResult.Content.Length)[0],
            BitConverter.GetBytes(operateResult.Content.Length)[1]
        });
    }

    /// <summary>
    /// 构建写入原始报文数据的请求信息
    /// </summary>
    /// <param name="address">地址数据</param>
    /// <param name="value">原始报文的数据</param>
    /// <returns>原始的写入报文数据</returns>
    private static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value)
    {
        var operateResult = FujiCommandSettingTypeAddress.ParseFrom(address, 0);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[9 + value.Length];
        array[0] = 1;
        array[1] = 0;
        array[2] = 0;
        array[3] = operateResult.Content.DataCode;
        array[4] = (byte)(4 + value.Length);
        array[5] = BitConverter.GetBytes(operateResult.Content.AddressStart)[0];
        array[6] = BitConverter.GetBytes(operateResult.Content.AddressStart)[1];
        array[7] = BitConverter.GetBytes(operateResult.Content.Length)[0];
        array[8] = BitConverter.GetBytes(operateResult.Content.Length)[0];
        value.CopyTo(array, 9);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 根据错误信息获取相关数据信息
    /// </summary>
    /// <param name="error">错误代号</param>
    /// <returns>实际的错误描述信息</returns>
    private static string GetErrorText(int error)
    {
        return error switch
        {
            18 => "Write of data to the program area",
            32 => "Non-existing CMND code",
            33 => "Input data is not in the order of data corresponding to CMND",
            34 => "Operation only from the loader is effective. Operation from any other node is disabled",
            36 => "A non-existing module has been specified",
            50 => "An address out of the memory size has been specified",
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 根据PLC返回的数据，解析出实际的数据内容
    /// </summary>
    /// <param name="response">PLC返回的数据</param>
    /// <returns>结果数据信息</returns>
    private static OperateResult<byte[]> UnpackResponseContentHelper(byte[] response)
    {
        try
        {
            if (response[1] != 0)
            {
                return new OperateResult<byte[]>(GetErrorText(response[1]));
            }
            if (response[0] == 1)
            {
                return OperateResult.CreateSuccessResult(Array.Empty<byte>());
            }
            if (response.Length < 10)
            {
                return new OperateResult<byte[]>(StringResources.Language.ReceiveDataLengthTooShort + "10, Source: " + response.ToHexString(' '));
            }
            return OperateResult.CreateSuccessResult(response.RemoveBegin(10));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("UnpackResponseContentHelper failed: " + ex.Message + " Source: " + response.ToHexString(' '));
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"FujiCommandSettingType[{IpAddress}:{Port}]";
    }
}
