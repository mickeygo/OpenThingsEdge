using System.Collections.Concurrent;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Beckhoff.Helper;
using ThingsEdge.Communication.Reflection;

namespace ThingsEdge.Communication.Profinet.Beckhoff;

/// <summary>
/// 倍福的ADS协议，支持读取倍福的地址数据，关于端口号的选择，TwinCAT2，端口号801；TwinCAT3，端口号为851，NETID可以选择手动输入，自动输入方式，具体参考API文档的示例代码。
/// </summary>
/// <remarks>
/// 支持的地址格式分四种，第一种是绝对的地址表示，比如M100，I100，Q100；
/// 第二种是字符串地址，采用s=aaaa;的表示方式；
/// 第三种是绝对内存地址采用i=1000000;的表示方式，第四种是自定义的index group, IG=0xF020;0 的地址。
/// </remarks>
public class BeckhoffAdsNet : DeviceTcpNet
{
    private readonly byte[] _targetAMSNetId = new byte[8];
    private readonly byte[] _sourceAMSNetId = new byte[8];
    private string _senderAMSNetId = string.Empty;
    private string _targetAmsNetID = string.Empty;
    private readonly ConcurrentDictionary<string, uint> _tagCaches = [];

    private readonly SoftIncrementCount _incrementCount = new(2147483647L, 1L);

    public override string IpAddress
    {
        get => base.IpAddress;
        set
        {
            base.IpAddress = value;
            var array = base.IpAddress.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < array.Length; i++)
            {
                _targetAMSNetId[i] = byte.Parse(array[i]);
            }
        }
    }

    /// <summary>
    /// 是否使用标签的名称缓存功能，默认为 <c>False</c>。
    /// </summary>
    public bool UseTagCache { get; set; }

    /// <summary>
    /// 是否使用服务器自动的NETID信息，默认手动设置。
    /// </summary>
    public bool UseAutoAmsNetID { get; set; }

    /// <summary>
    /// 获取或设置Ams的端口号信息，TwinCAT2，端口号801,811,821,831；TwinCAT3，端口号为851,852,853。
    /// </summary>
    public int AmsPort
    {
        get => BitConverter.ToUInt16(_targetAMSNetId, 6);
        set
        {
            _targetAMSNetId[6] = BitConverter.GetBytes(value)[0];
            _targetAMSNetId[7] = BitConverter.GetBytes(value)[1];
        }
    }

    /// <summary>
    /// 通过指定的ip地址以及端口号实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号</param>
    public BeckhoffAdsNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;

        WordLength = 2;
        _targetAMSNetId[4] = 1;
        _targetAMSNetId[5] = 1;
        _targetAMSNetId[6] = 83;
        _targetAMSNetId[7] = 3;
        _sourceAMSNetId[4] = 1;
        _sourceAMSNetId[5] = 1;
        ByteTransform = new RegularByteTransform();
        CommunicationPipe.UseServerActivePush = true;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new AdsNetMessage();
    }

    /// <summary>
    /// 目标的地址，举例 192.168.0.1.1.1；也可以是带端口号 192.168.0.1.1.1:801。
    /// </summary>
    /// <remarks>
    /// Port：1: AMS Router; 2: AMS Debugger; 800: Ring 0 TC2 PLC; 801: TC2 PLC Runtime System 1; 811: TC2 PLC Runtime System 2; 
    /// 821: TC2 PLC Runtime System 3; 831: TC2 PLC Runtime System 4; 850: Ring 0 TC3 PLC; 851: TC3 PLC Runtime System 1；
    /// 852: TC3 PLC Runtime System 2; 853: TC3 PLC Runtime System 3; 854: TC3 PLC Runtime System 4; ...
    /// </remarks>
    /// <param name="amsNetId">AMSNet Id地址</param>
    public void SetTargetAMSNetId(string amsNetId)
    {
        if (!string.IsNullOrEmpty(amsNetId))
        {
            AdsHelper.StrToAMSNetId(amsNetId).CopyTo(_targetAMSNetId, 0);
            _targetAmsNetID = amsNetId;
        }
    }

    /// <summary>
    /// 设置原目标地址 举例 192.168.0.100.1.1；也可以是带端口号 192.168.0.100.1.1:34567
    /// </summary>
    /// <param name="amsNetId">原地址</param>
    public void SetSenderAMSNetId(string amsNetId)
    {
        if (!string.IsNullOrEmpty(amsNetId))
        {
            AdsHelper.StrToAMSNetId(amsNetId).CopyTo(_sourceAMSNetId, 0);
            _senderAMSNetId = amsNetId;
        }
    }

    /// <summary>
    /// 获取当前发送的AMS的网络ID信息
    /// </summary>
    /// <returns>AMS发送信息</returns>
    public string GetSenderAMSNetId()
    {
        return AdsHelper.GetAmsNetIdString(_sourceAMSNetId, 0);
    }

    /// <summary>
    /// 获取当前目标的AMS网络的ID信息
    /// </summary>
    /// <returns>AMS目标信息</returns>
    public string GetTargetAMSNetId()
    {
        return AdsHelper.GetAmsNetIdString(_targetAMSNetId, 0);
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        var value = (uint)_incrementCount.GetCurrentValue();
        _targetAMSNetId.CopyTo(command, 6);
        _sourceAMSNetId.CopyTo(command, 14);
        command[34] = BitConverter.GetBytes(value)[0];
        command[35] = BitConverter.GetBytes(value)[1];
        command[36] = BitConverter.GetBytes(value)[2];
        command[37] = BitConverter.GetBytes(value)[3];
        return base.PackCommandWithHeader(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        if (response.Length >= 38)
        {
            var num = ByteTransform.TransUInt16(response, 22);
            OperateResult operateResult = AdsHelper.CheckResponse(response);
            if (!operateResult.IsSuccess)
            {
                if (operateResult.ErrorCode == 1809 && (num == 2 || num == 3))
                {
                    _tagCaches.Clear();
                }
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }

            try
            {
                switch (num)
                {
                    case 1:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(42));
                    case 2:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(46));
                    case 3:
                        return OperateResult.CreateSuccessResult(Array.Empty<byte>());
                    case 4:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(42));
                    case 5:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(42));
                    case 6:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(42));
                    case 7:
                        return OperateResult.CreateSuccessResult(Array.Empty<byte>());
                    case 9:
                        return OperateResult.CreateSuccessResult(response.RemoveBegin(46));
                }
            }
            catch (Exception ex)
            {
                return new OperateResult<byte[]>("UnpackResponseContent failed: " + ex.Message + Environment.NewLine + "Source: " + response.ToHexString(' '));
            }
        }
        return base.UnpackResponseContent(send, response);
    }

    /// <inheritdoc />
    protected override void ExtraAfterReadFromCoreServer(OperateResult read)
    {
        if (!read.IsSuccess && read.ErrorCode < 0 && UseTagCache)
        {
            _tagCaches.Clear();
        }
        base.ExtraAfterReadFromCoreServer(read);
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        if (string.IsNullOrEmpty(_senderAMSNetId) && string.IsNullOrEmpty(_targetAmsNetID))
        {
            UseAutoAmsNetID = true;
        }
        if (UseAutoAmsNetID)
        {
            var read1 = await GetLocalNetIdAsync().ConfigureAwait(false);
            if (!read1.IsSuccess)
            {
                return read1;
            }
            if (read1.Content.Length >= 12)
            {
                Array.Copy(read1.Content, 6, _targetAMSNetId, 0, 6);
            }
            var send2 = await CommunicationPipe.SendAsync(AdsHelper.PackAmsTcpHelper(AmsTcpHeaderFlags.PortConnect, new byte[2])).ConfigureAwait(continueOnCapturedContext: false);
            if (!send2.IsSuccess)
            {
                return send2;
            }
            var read2 = await CommunicationPipe.ReceiveMessageAsync(GetNewNetMessage(), null, useActivePush: false).ConfigureAwait(continueOnCapturedContext: false);
            if (!read2.IsSuccess)
            {
                return read2;
            }
            if (read2.Content.Length >= 14)
            {
                Array.Copy(read2.Content, 6, _sourceAMSNetId, 0, 8);
            }
        }
        else if (string.IsNullOrEmpty(_senderAMSNetId))
        {
            var communicationPipe = CommunicationPipe;
            if (communicationPipe is PipeTcpNet pipe)
            {
                var iPEndPoint = (IPEndPoint)pipe.Socket.LocalEndPoint;
                _sourceAMSNetId[6] = BitConverter.GetBytes(iPEndPoint.Port)[0];
                _sourceAMSNetId[7] = BitConverter.GetBytes(iPEndPoint.Port)[1];
                iPEndPoint.Address.GetAddressBytes().CopyTo(_sourceAMSNetId, 0);
            }
        }
        if (UseTagCache)
        {
            _tagCaches.Clear();
        }
        return await base.InitializationOnConnectAsync().ConfigureAwait(false);
    }

    private async Task<OperateResult<byte[]>> GetLocalNetIdAsync()
    {
        var pipeTcpNet = CommunicationPipe.GetType() != typeof(PipeSslNet)
            ? new PipeTcpNet(IpAddress, Port)
            {
                ConnectTimeOut = ConnectTimeOut,
                ReceiveTimeOut = ReceiveTimeOut
            }
            : new PipeSslNet(IpAddress, Port, serverMode: false)
            {
                ConnectTimeOut = ConnectTimeOut,
                ReceiveTimeOut = ReceiveTimeOut
            };
        var opSocket = await pipeTcpNet.OpenCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (!opSocket.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(opSocket);
        }
        var send = await pipeTcpNet.SendAsync(AdsHelper.PackAmsTcpHelper(AmsTcpHeaderFlags.GetLocalNetId, new byte[4])).ConfigureAwait(continueOnCapturedContext: false);
        if (!send.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(send);
        }

        var read = await pipeTcpNet.ReceiveMessageAsync(GetNewNetMessage(), null).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        await pipeTcpNet.CloseCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
        return read;
    }

    /// <inheritdoc />
    protected override bool DecideWhetherQAMessage(CommunicationPipe pipe, OperateResult<byte[]> receive)
    {
        if (!receive.IsSuccess)
        {
            if (UseTagCache)
            {
                _tagCaches.Clear();
            }
            return false;
        }
        var content = receive.Content!;
        if (content.Length >= 2 && BitConverter.ToUInt16(content, 0) == 0)
        {
            if (content.Length >= 24)
            {
                var num = ByteTransform.TransUInt16(content, 22);
                if (num == 8)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根据当前标签的地址获取到内存偏移地址。
    /// </summary>
    /// <param name="address">带标签的地址信息，例如s=A,那么标签就是A</param>
    /// <returns>内存偏移地址</returns>
    public async Task<OperateResult<uint>> ReadValueHandleAsync(string address)
    {
        if (!address.StartsWith("s="))
        {
            return new OperateResult<uint>("When read valueHandle, address must startwith 's=', forexample: s=MAIN.A");
        }
        var build = AdsHelper.BuildReadWriteCommand(address, 4, isBit: false, AdsHelper.StrToAdsBytes(address.Substring(2)));
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<uint>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<uint>(read);
        }
        return OperateResult.CreateSuccessResult(BitConverter.ToUInt32(read.Content, 0));
    }

    /// <summary>
    /// 将字符串的地址转换为内存的地址，其他地址则不操作。
    /// </summary>
    /// <param name="address">地址信息，s=A的地址转换为i=100000的形式</param>
    /// <returns>地址</returns>
    public async Task<OperateResult<string>> TransValueHandleAsync(string address)
    {
        if (address.StartsWith("s=") || address.StartsWith("S="))
        {
            if (UseTagCache && _tagCaches.TryGetValue(address, out var value))
            {
                return OperateResult.CreateSuccessResult($"i={value}");
            }
            var read = await ReadValueHandleAsync(address).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(read);
            }
            if (UseTagCache)
            {
                _ = _tagCaches.GetOrAdd(address, read.Content);
            }
            return OperateResult.CreateSuccessResult($"i={read.Content}");
        }
        return OperateResult.CreateSuccessResult(address);
    }

    /// <summary>
    /// 读取Ads设备的设备信息。主要是版本号，设备名称。
    /// </summary>
    /// <returns>设备信息</returns>
    public async Task<OperateResult<AdsDeviceInfo>> ReadAdsDeviceInfoAsync()
    {
        var build = AdsHelper.BuildReadDeviceInfoCommand();
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<AdsDeviceInfo>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<AdsDeviceInfo>(read);
        }
        return OperateResult.CreateSuccessResult(new AdsDeviceInfo(read.Content));
    }

    /// <summary>
    /// 读取Ads设备的状态信息，其中 Content1 是 Ads State，Content2 是 Device State。
    /// </summary>
    /// <returns>设备状态信息</returns>
    public async Task<OperateResult<ushort, ushort>> ReadAdsStateAsync()
    {
        var build = AdsHelper.BuildReadStateCommand();
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, ushort>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, ushort>(read);
        }
        return OperateResult.CreateSuccessResult(BitConverter.ToUInt16(read.Content, 0), BitConverter.ToUInt16(read.Content, 2));
    }

    /// <summary>
    /// 写入Ads的状态，可以携带数据信息，数据可以为空。
    /// </summary>
    /// <param name="state">ads state</param>
    /// <param name="deviceState">device state</param>
    /// <param name="data">数据信息</param>
    /// <returns>是否写入成功</returns>
    public async Task<OperateResult> WriteAdsStateAsync(short state, short deviceState, byte[] data)
    {
        var build = AdsHelper.BuildWriteControlCommand(state, deviceState, data);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
    }

    /// <summary>
    /// 释放当前的系统句柄。
    /// </summary>
    /// <param name="handle">句柄</param>
    /// <returns>是否释放成功</returns>
    public async Task<OperateResult> ReleaseSystemHandleAsync(uint handle)
    {
        var build = AdsHelper.BuildReleaseSystemHandle(handle);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取结构体的信息，传入结构体的类型，以及结构体的起始地址。
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    /// <param name="address">结构体的地址信息</param>
    /// <returns>是否读取成功</returns>
    public async Task<OperateResult<T>> ReadStructAsync<T>(string address) where T : struct
    {
        var operateResult = await ReadAsync(address, (ushort)Marshal.SizeOf<T>()).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<T>(operateResult);
        }
        var gCHandle = GCHandle.Alloc(operateResult.Content, GCHandleType.Pinned);
        try
        {
            var structure = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
            return OperateResult.CreateSuccessResult(structure);
        }
        finally
        {
            gCHandle.Free();
        }
    }

    /// <summary>
    /// 将一个结构体写入到指定的地址中去，需要指定写入的起始地址。
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    /// <param name="address">起始地址</param>
    /// <param name="value">结构体的值</param>
    /// <returns>是否写入成功</returns>
    public async Task<OperateResult> WriteStructAsync<T>(string address, T value) where T : struct
    {
        var value2 = new byte[Marshal.SizeOf(value)];
        var gCHandle = GCHandle.Alloc(value2, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, gCHandle.AddrOfPinnedObject(), fDeleteOld: false);
        }
        finally
        {
            gCHandle.Free();
        }
        
        return await WriteAsync(address, value2).ConfigureAwait(false);
    }

    public override async Task<OperateResult<T>> ReadAsync<T>()
    {
        var type = typeof(T);
        var obj = type.Assembly.CreateInstance(type.FullName);
        var array = CommReflectionHelper.GetHslPropertyInfos(type, GetType(), null, ByteTransform);
        var address = array.Select((m) => m.DeviceAddressAttribute.Address).ToArray();
        var length = array.Select((m) => (ushort)m.ByteLength).ToArray();
        var read = await ReadAsync(address, length).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<T>(read);
        }
        CommReflectionHelper.SetPropertyValueFrom(ByteTransform, obj, array, read.Content);
        return OperateResult.CreateSuccessResult((T)obj);
    }

    /// <summary>
    /// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">地址信息，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A</param>
    /// <param name="length">长度</param>
    /// <returns>包含是否成功的结果对象</returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var addressCheck = await TransValueHandleAsync(address).ConfigureAwait(false);
        if (!addressCheck.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressCheck);
        }
        address = addressCheck.Content;
        var build = AdsHelper.BuildReadCommand(address, length, isBit: false);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 批量读取PLC的数据，需要传入地址数组，以及读取的长度数组信息，长度单位为字节单位，如果是读取bool变量的，则以bool为单位，统一返回一串字节数据信息，需要进行二次解析的操作。
    /// </summary>
    /// <param name="addresses">地址数组信息</param>
    /// <param name="lengths">读取的长度数组信息</param>
    /// <returns>原始字节数组的结果对象</returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string[] addresses, ushort[] lengths)
    {
        if (addresses.Length != lengths.Length)
        {
            return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        for (var i = 0; i < addresses.Length; i++)
        {
            var addressCheck = await TransValueHandleAsync(addresses[i]).ConfigureAwait(false);
            if (!addressCheck.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(addressCheck);
            }
            addresses[i] = addressCheck.Content;
        }
        var build = AdsHelper.BuildReadCommand(addresses, lengths);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">地址信息，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A</param>
    /// <param name="values">数据值</param>
    /// <returns>是否写入成功</returns>
    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        var addressCheck = await TransValueHandleAsync(address).ConfigureAwait(false);
        if (!addressCheck.IsSuccess)
        {
            return addressCheck;
        }
        address = addressCheck.Content;
        var build = AdsHelper.BuildWriteCommand(address, values, isBit: false);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">PLC的地址信息，例如 M10</param>
    /// <param name="length">数据长度</param>
    /// <returns>包含是否成功的结果对象</returns>
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        if (Regex.IsMatch(address, "^[MIQ][0-9]+\\.[0-7]$", RegexOptions.IgnoreCase) && length > 1)
        {
            return await CommunicationHelper.ReadBoolAsync(this, address, length, 8).ConfigureAwait(continueOnCapturedContext: false);
        }
        var addressCheck = await TransValueHandleAsync(address).ConfigureAwait(continueOnCapturedContext: false);
        if (!addressCheck.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(addressCheck);
        }
        address = addressCheck.Content;
        var build = AdsHelper.BuildReadCommand(address, length, isBit: true);
        if (!build.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(build);
        }
        var read = await ReadFromCoreServerAsync(build.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.Select((m) => m != 0).ToArray());
    }

    /// <summary>
    /// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据值</param>
    /// <returns>是否写入成功</returns>
    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        var addressCheck = await TransValueHandleAsync(address).ConfigureAwait(continueOnCapturedContext: false);
        if (!addressCheck.IsSuccess)
        {
            return addressCheck;
        }
        address = addressCheck.Content;
        var build = AdsHelper.BuildWriteCommand(address, values, isBit: true);
        if (!build.IsSuccess)
        {
            return build;
        }
        return await ReadFromCoreServerAsync(build.Content).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 读取PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>包含是否成功的结果对象</returns>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(continueOnCapturedContext: false));
    }

    /// <summary>
    /// 写入PLC的数据，地址共有三种格式，一：I,Q,M数据信息，举例M0,M100；二：内存地址，i=100000；三：标签地址，s=A。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="value">数据值</param>
    /// <returns>是否写入成功</returns>
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override string ToString()
    {
        return $"BeckhoffAdsNet[{IpAddress}:{Port}]";
    }
}
