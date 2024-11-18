using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// AB PLC的数据通信类，使用CIP协议实现，适用1756，1769等型号，支持使用标签的形式进行读写操作，支持标量数据，一维数组，二维数组，三维数组等等。如果是局部变量，那么使用 Program:MainProgram.[变量名]。
/// </summary>
/// <remarks>
/// 默认的地址就是PLC里的TAG名字，比如A，B，C；如果你需要读取的数据是一个数组，那么A就是默认的A[0]，如果想要读取偏移量为10的数据，那么地址为A[10]，多维数组同理，使用A[10,10,10]的操作。
/// 假设你读取的是局部变量，那么使用 Program:MainProgram.变量名。
/// <para>
/// 目前适用的系列为1756 ControlLogix, 1756 GuardLogix, 1769 CompactLogix, 1769 Compact GuardLogix, 1789SoftLogix, 5069 CompactLogix, 5069 Compact GuardLogix, Studio 5000 Logix Emulate
/// 如果你有个Bool数组要读取，变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用
/// </para>
/// <para>
/// ReadBoolArray("A[0]")   // 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。
/// </para>
/// <para>
/// 地址可以携带站号信息，只要在前面加上slot=2;即可，这就是访问站号2的数据了，例如 slot=2;AAA，如果使用了自定义的消息路由，例如：[IP or Hostname],1,[Optional Routing Path],CPU Slot 172.20.1.109,1,[15,2,18,1],12。
/// 在实例化之后，连接PLC之前，需要调用如下代码 plc.MessageRouter = new MessageRouter( "1.15.2.18.1.12" )
/// </para>
/// </remarks>
public class AllenBradleyNet : DeviceTcpNet, IReadWriteCip, IReadWriteNet
{
    /// <summary>
    /// The current session handle, which is determined by the PLC when communicating with the PLC handshake
    /// </summary>
    public uint SessionHandle { get; protected set; }

    /// <summary>
    /// Gets or sets the slot number information for the current plc, which should be set before connections
    /// </summary>
    public byte Slot { get; set; }

    /// <summary>
    /// port and slot information
    /// </summary>
    public byte[] PortSlot { get; set; }

    /// <summary>
    /// 获取或设置整个交互指令的控制码，默认为0x6F，通常不需要修改<br />
    /// Gets or sets the control code of the entire interactive instruction. The default is 0x6F, and usually does not need to be modified.
    /// </summary>
    public ushort CipCommand { get; set; } = 111;

    /// <summary>
    /// 获取或设置当前的通信的消息路由信息，可以实现一些复杂情况的通信，数据包含背板号，路由参数，slot，例如：1.15.2.18.1.1<br />
    /// Get or set the message routing information of the current communication, which can realize some complicated communication. 
    /// The data includes the backplane number, routing parameters, and slot, for example: 1.15.2.18.1.1
    /// </summary>
    public MessageRouter MessageRouter { get; set; }

    /// <summary>
    /// Instantiate a communication object for a Allenbradley PLC protocol
    /// </summary>
    public AllenBradleyNet()
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// Instantiate a communication object for a Allenbradley PLC protocol
    /// </summary>
    /// <param name="ipAddress">PLC IpAddress</param>
    /// <param name="port">PLC Port</param>
    public AllenBradleyNet(string ipAddress, int port = 44818)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new AllenBradleyMessage();
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        return AllenBradleyHelper.PackRequestHeader(CipCommand, SessionHandle, command);
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        var operateResult = ReadFromCoreServer(CommunicationPipe, AllenBradleyHelper.RegisterSessionHandle(), hasResponseData: true, usePackAndUnpack: false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = AllenBradleyHelper.CheckResponse(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        if (operateResult.Content.Length >= 8)
        {
            SessionHandle = BitConverter.ToUInt32(operateResult.Content, 4);
        }
        if (MessageRouter != null)
        {
            var routerCIP = MessageRouter.GetRouterCIP();
            var operateResult3 = ReadFromCoreServer(CommunicationPipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, AllenBradleyHelper.PackCommandSpecificData(new byte[4], AllenBradleyHelper.PackCommandSingleService(routerCIP, 178, isConnected: false, 0))), hasResponseData: true, usePackAndUnpack: false);
            if (!operateResult3.IsSuccess)
            {
                return operateResult3;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override OperateResult ExtraOnDisconnect()
    {
        if (CommunicationPipe != null)
        {
            var operateResult = ReadFromCoreServer(CommunicationPipe, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle), hasResponseData: true, usePackAndUnpack: false);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read = await ReadFromCoreServerAsync(CommunicationPipe, AllenBradleyHelper.RegisterSessionHandle(), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return check;
        }
        if (read.Content.Length >= 8)
        {
            SessionHandle = BitConverter.ToUInt32(read.Content, 4);
        }
        if (MessageRouter != null)
        {
            var cip = MessageRouter.GetRouterCIP();
            var messageRouter = await ReadFromCoreServerAsync(CommunicationPipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, AllenBradleyHelper.PackCommandSpecificData(new byte[4], AllenBradleyHelper.PackCommandSingleService(cip, 178, isConnected: false, 0))), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(continueOnCapturedContext: false);
            if (!messageRouter.IsSuccess)
            {
                return messageRouter;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> ExtraOnDisconnectAsync()
    {
        if (CommunicationPipe != null)
        {
            var read = await ReadFromCoreServerAsync(CommunicationPipe, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 创建一个读取标签的报文指定，标签地址可以手动动态指定slot编号，例如 slot=2;AAA。
    /// </summary>
    /// <param name="address">the address of the tag name</param>
    /// <param name="length">Array information, if not arrays, is 1 </param>
    /// <returns>Message information that contains the result object </returns>
    public virtual OperateResult<byte[]> BuildReadCommand(string[] address, ushort[] length)
    {
        if (address == null || length == null)
        {
            return new OperateResult<byte[]>("address or length is null");
        }
        if (address.Length != length.Length)
        {
            return new OperateResult<byte[]>("address and length is not same array");
        }

        try
        {
            var b = Slot;
            var list = new List<byte[]>();
            for (var i = 0; i < address.Length; i++)
            {
                b = (byte)CommunicationHelper.ExtractParameter(ref address[i], "slot", Slot);
                list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i]));
            }
            var value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? [1, b], list.ToArray()));
            return OperateResult.CreateSuccessResult(value);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    /// <summary>
    /// 创建一个读取多标签的报文。
    /// </summary>
    /// <param name="address">The address of the tag name </param>
    /// <returns>Message information that contains the result object </returns>
    public OperateResult<byte[]> BuildReadCommand(string[] address)
    {
        if (address == null)
        {
            return new OperateResult<byte[]>("address or length is null");
        }
        var array = new ushort[address.Length];
        for (var i = 0; i < address.Length; i++)
        {
            array[i] = 1;
        }
        return BuildReadCommand(address, array);
    }

    /// <summary>
    /// Create a written message instruction
    /// </summary>
    /// <param name="address">The address of the tag name </param>
    /// <param name="typeCode">Data type</param>
    /// <param name="data">Source Data </param>
    /// <param name="length">In the case of arrays, the length of the array </param>
    /// <returns>Message information that contains the result object</returns>
    protected virtual OperateResult<List<byte[]>> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
    {
        try
        {
            var b = (byte)CommunicationHelper.ExtractParameter(ref address, "slot", Slot);
            var num = CommunicationHelper.ExtractParameter(ref address, "x", -1);
            if (num == 83 || num == 82)
            {
                var num2 = 0;
                var list = SoftBasic.ArraySplitByLength(data, 474);
                for (var i = 0; i < list.Count; i++)
                {
                    var array = AllenBradleyHelper.PackRequestWriteSegment(address, typeCode, list[i], num2, length);
                    num2 += list[i].Length;
                    var value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, array));
                    list[i] = value;
                }
                return OperateResult.CreateSuccessResult(list);
            }
            var array2 = AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length);
            var item = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, array2));
            return OperateResult.CreateSuccessResult(new List<byte[]> { item });
        }
        catch (Exception ex)
        {
            return new OperateResult<List<byte[]>>("Address Wrong:" + ex.Message);
        }
    }

    /// <summary>
    /// Create a written message instruction
    /// </summary>
    /// <param name="address">The address of the tag name </param>
    /// <param name="data">Bool Data </param>
    /// <returns>Message information that contains the result object</returns>
    public OperateResult<byte[]> BuildWriteCommand(string address, bool data)
    {
        try
        {
            var b = (byte)CommunicationHelper.ExtractParameter(ref address, "slot", Slot);
            var array = AllenBradleyHelper.PackRequestWrite(address, data);
            var value = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, b }, array));
            return OperateResult.CreateSuccessResult(value);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    private OperateResult CheckResponse(byte[] response)
    {
        var operateResult = AllenBradleyHelper.CheckResponse(response);
        if (!operateResult.IsSuccess && operateResult.ErrorCode == 100)
        {
            CommunicationPipe.RaisePipeError();
        }
        return operateResult;
    }

    /// <summary>
    /// 读取指定地址的二进制数据内容，长度为地址长度，一般都是1，除非读取数组时，如果需要强制使用 片段读取功能码，则地址里携带 x=0x52; 或是 x=82; 则强制使用片段读取。
    /// </summary>
    /// <remarks>
    /// 使用片段读取的时候，可以读取一些数量量非常大的地址，例如一个结构体标签有100_000个字节长度的时候。
    /// </remarks>
    /// <param name="address">Address format of the node</param>
    /// <param name="length">In the case of arrays, the length of the array </param>
    /// <returns>Result data with result object </returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        CommunicationHelper.ExtractParameter(ref address, "type", 0);
        var x = CommunicationHelper.ExtractParameter(ref address, "x", -1);
        if (x == 82 || x == 83)
        {
            return await ReadSegmentAsync(address, 0, length).ConfigureAwait(continueOnCapturedContext: false);
        }
        if (length > 1)
        {
            return await ReadSegmentAsync(address, 0, length).ConfigureAwait(continueOnCapturedContext: false);
        }
        return await ReadAsync([address], [length]).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 批量读取多地址的数据信息，例如我可以读取两个标签的数据 "A","B[0]"，每个地址的数据长度为1，表示一个数据，最终读取返回的是一整个的字节数组，需要自行解析。
    /// </summary>
    /// <param name="address">Name of the node </param>
    /// <returns>Result data with result object </returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address)
    {
        if (address == null)
        {
            return new OperateResult<byte[]>("address can not be null");
        }

        var length = new ushort[address.Length];
        for (var i = 0; i < length.Length; i++)
        {
            length[i] = 1;
        }
        return await ReadAsync(address, length).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量读取多地址的数据信息，例如我可以读取两个标签的数据 "A","B[0]"， 长度为 [1, 5]，返回的是一整个的字节数组，需要自行解析。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <param name="length">如果是数组，就为数组长度</param>
    /// <returns>带有结果对象的结果数据</returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address, ushort[] length)
    {
        var read = await ReadWithTypeAsync(address, length).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content1);
    }

    private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string[] address, ushort[] length)
    {
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(read);
        }
        var check = CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(check);
        }
        return AllenBradleyHelper.ExtractActualData(read.Content, isRead: true);
    }

    /// <summary>
    /// Read Segment Data Array form plc, use address tag name
    /// </summary>
    /// <param name="address">Tag name in plc</param>
    /// <param name="startIndex">array start index, uint byte index</param>
    /// <param name="length">array length, data item length</param>
    /// <returns>Results Bytes</returns>
    public async Task<OperateResult<byte[]>> ReadSegmentAsync(string address, int startIndex, int length)
    {
        try
        {
            var bytesContent = new List<byte>();
            OperateResult<byte[], ushort, bool> analysis;
            do
            {
                var read = await ReadCipFromServerAsync(AllenBradleyHelper.PackRequestReadSegment(address, startIndex, length)).ConfigureAwait(continueOnCapturedContext: false);
                if (!read.IsSuccess)
                {
                    return read;
                }
                analysis = AllenBradleyHelper.ExtractActualData(read.Content, isRead: true);
                if (!analysis.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(analysis);
                }
                startIndex += analysis.Content1.Length;
                bytesContent.AddRange(analysis.Content1);
            }
            while (analysis.Content3);
            return OperateResult.CreateSuccessResult(bytesContent.ToArray());
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    /// <summary>
    /// 使用CIP报文和服务器进行核心的数据交换
    /// </summary>
    /// <param name="cips">Cip commands</param>
    /// <returns>Results Bytes</returns>
    public async Task<OperateResult<byte[]>> ReadCipFromServerAsync(params byte[][] cips)
    {
        var commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(new byte[4], PackCommandService(PortSlot ?? new byte[2] { 1, Slot }, cips.ToArray()));
        var read = await ReadFromCoreServerAsync(commandSpecificData).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(read.Content);
    }

    /// <summary>
    /// 使用EIP报文和服务器进行核心的数据交换
    /// </summary>
    /// <param name="eip">eip commands</param>
    /// <returns>Results Bytes</returns>
    public async Task<OperateResult<byte[]>> ReadEipFromServerAsync(params byte[][] eip)
    {
        var commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(eip);
        var read = await ReadFromCoreServerAsync(commandSpecificData).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(read.Content);
    }

    /// <summary>
    /// 读取单个的bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是由int组成的bool数组的一个值，一律带"i="开头访问，例如"i=A[0]"。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <returns>带有结果对象的结果数据</returns>
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        if (address.StartsWith("i="))
        {
            return ByteTransformHelper.GetResultFromArray(await ReadBoolAsync(address, 1).ConfigureAwait(continueOnCapturedContext: false));
        }
        var read = await ReadAsync(address, 1).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool>(read);
        }
        return OperateResult.CreateSuccessResult(ByteTransform.TransBool(read.Content, 0));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        OperateResult<byte[]> read;
        if (address.StartsWith("i="))
        {
            address = address.Substring(2);
            address = AllenBradleyHelper.AnalysisArrayIndex(address, out var bitIndex);
            var uintIndex = bitIndex / 32 == 0 ? "" : $"[{bitIndex / 32}]";
            read = await ReadAsync(length: (ushort)CommunicationHelper.CalculateOccupyLength(bitIndex, length, 32), address: address + uintIndex).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            return OperateResult.CreateSuccessResult(read.Content.ToBoolArray().SelectMiddle(bitIndex % 32, length));
        }
        read = await ReadAsync(address, length).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content, length));
    }

    /// <summary>
    /// 批量读取的bool数组信息，如果你有个Bool数组变量名为 A, 那么读第0个位，可以通过 ReadBool("A")，但是第二个位需要使用 ReadBoolArray("A[0]")，
    /// 返回32个bool长度，0-31的索引，如果我想读取32-63的位索引，就需要 ReadBoolArray("A[1]") ，以此类推。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <returns>带有结果对象的结果数据</returns>
    public async Task<OperateResult<bool[]>> ReadBoolArrayAsync(string address)
    {
        var read = await ReadAsync(address, 1).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray());
    }

    /// <summary>
    /// 读取PLC的byte类型的数据。
    /// </summary>
    /// <param name="address">节点的名称</param>
    /// <returns>带有结果对象的结果数据</returns>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }

    /// <summary>
    /// 从PLC里读取一个指定标签名的原始数据信息及其数据类型信息。
    /// </summary>
    /// <param name="address">PLC的标签地址信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>包含原始数据信息及数据类型的结果对象</returns>
    public async Task<OperateResult<ushort, byte[]>> ReadTagAsync(string address, ushort length = 1)
    {
        var read = await ReadWithTypeAsync([address], [length]).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content2, read.Content1);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.TagEnumerator" />
    public async Task<OperateResult<AbTagItem[]>> TagEnumeratorAsync()
    {
        var lists = new List<AbTagItem>();
        for (var i = 0; i < 2; i++)
        {
            var instanceAddress = 0u;
            OperateResult<byte[], ushort, bool> analysis;
            do
            {
                var readCip = await ReadCipFromServerAsync(i == 0 ? AllenBradleyHelper.BuildEnumeratorCommand(instanceAddress) : AllenBradleyHelper.BuildEnumeratorProgrameMainCommand(instanceAddress)).ConfigureAwait(false);
                if (!readCip.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<AbTagItem[]>(readCip);
                }
                analysis = AllenBradleyHelper.ExtractActualData(readCip.Content, isRead: true);
                if (!analysis.IsSuccess)
                {
                    if (i == 1)
                    {
                        return OperateResult.CreateSuccessResult(lists.ToArray());
                    }
                    return OperateResult.CreateFailedResult<AbTagItem[]>(analysis);
                }
                if (readCip.Content.Length >= 43 && BitConverter.ToUInt16(readCip.Content, 40) == 213)
                {
                    lists.AddRange(AbTagItem.PraseAbTagItems(readCip.Content, 44, i == 0, out var instance));
                    instanceAddress = instance + 1;
                    continue;
                }
                return new OperateResult<AbTagItem[]>(StringResources.Language.UnknownError + " Source: " + readCip.Content.ToHexString(' '));
            }
            while (analysis.Content3);
        }
        return OperateResult.CreateSuccessResult(lists.ToArray());
    }

    private async Task<OperateResult<AbStructHandle>> ReadTagStructHandleAsync(AbTagItem structTag)
    {
        var operateResult = await ReadCipFromServerAsync(AllenBradleyHelper.GetStructHandleCommand(structTag.SymbolType)).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<AbStructHandle>(operateResult);
        }
        if (operateResult.Content.Length >= 43 && BitConverter.ToInt32(operateResult.Content, 40) == 131)
        {
            return OperateResult.CreateSuccessResult(new AbStructHandle(operateResult.Content, 44));
        }
        return new OperateResult<AbStructHandle>(StringResources.Language.UnknownError + " Source Data: " + operateResult.Content.ToHexString(' '));
    }

    /// <summary>
    /// 枚举结构体的方法，传入结构体的标签对象，返回结构体子属性标签列表信息，子属性有可能是标量数据，也可能是另一个结构体。
    /// </summary>
    /// <param name="structTag">结构体的标签</param>
    /// <returns>是否成功</returns>
    public async Task<OperateResult<AbTagItem[]>> StructTagEnumeratorAsync(AbTagItem structTag)
    {
        var operateResult = await ReadTagStructHandleAsync(structTag).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<AbTagItem[]>(operateResult);
        }
        var operateResult2 = await ReadCipFromServerAsync(AllenBradleyHelper.GetStructItemNameType(structTag.SymbolType, operateResult.Content, 0)).ConfigureAwait(false);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<AbTagItem[]>(operateResult2);
        }
        if (operateResult2.Content.Length >= 43 && operateResult2.Content[40] == 204 && operateResult2.Content[41] == 0 && operateResult2.Content[42] == 0)
        {
            return OperateResult.CreateSuccessResult(AbTagItem.PraseAbTagItemsFromStruct(operateResult2.Content, 44, operateResult.Content).ToArray());
        }
        return new OperateResult<AbTagItem[]>(StringResources.Language.UnknownError + " Status:" + operateResult2.Content[42]);
    }

    public OperateResult<string> ReadPlcType()
    {
        return AllenBradleyHelper.ReadPlcType(this);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return await ReadStringAsync(address, 1).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取字符串数据，默认为<see cref="P:System.Text.Encoding.UTF8" />编码
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的string数据</returns>
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await ReadStringAsync(address, length, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        CommunicationHelper.ExtractParameter(ref address, "type", 0);
        return AllenBradleyHelper.ExtractActualString(await ReadWithTypeAsync([address], [length]).ConfigureAwait(false), ByteTransform, encoding);
    }

    public async Task<OperateResult<string>> ReadPlcTypeAsync()
    {
        return await AllenBradleyHelper.ReadPlcTypeAsync(this).ConfigureAwait(false);
    }

    /// <summary>
    /// 当前写入字节数组使用数据类型 0xD1 写入，如果其他的字节类型需要调用 <see cref="WriteTagAsync" /> 方法来实现。
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="value">值</param>
    /// <returns>写入结果值</returns>
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteTagAsync(address, 209, value, !CommunicationHelper.IsAddressEndWithIndex(address) ? 1 : value.Length).ConfigureAwait(false);
    }

    public virtual async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
    {
        typeCode = (ushort)CommunicationHelper.ExtractParameter(ref address, "type", typeCode);
        var command = BuildWriteCommand(address, typeCode, value, length);
        if (!command.IsSuccess)
        {
            return command;
        }
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var check = CheckResponse(read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(check);
            }
            OperateResult extra = AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
            if (!extra.IsSuccess)
            {
                return extra;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    public override async Task<OperateResult> WriteAsync(string address, short[] values)
    {
        return await WriteTagAsync(address, 195, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ushort[] values)
    {
        return await WriteTagAsync(address, 199, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteTagAsync(address, 196, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteTagAsync(address, 200, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteTagAsync(address, 202, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteTagAsync(address, 197, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteTagAsync(address, 201, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteTagAsync(address, 203, ByteTransform.TransByte(values), GetWriteValueLength(address, values.Length)).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var typeCode = (ushort)CommunicationHelper.ExtractParameter(ref address, "type", 194);
        byte[] data;
        if (typeCode == 218)
        {
            data = encoding.GetBytes(value);
            return await WriteTagAsync(address, typeCode, SoftBasic.SpliceArray([(byte)data.Length], data)).ConfigureAwait(false);
        }
        data = encoding.GetBytes(value);
        var write = await WriteAsync(address + ".LEN", data.Length).ConfigureAwait(continueOnCapturedContext: false);
        if (!write.IsSuccess)
        {
            return write;
        }
        return await WriteTagAsync(value: SoftBasic.ArrayExpandToLengthEven(data), address: address + ".DATA[0]", typeCode: typeCode, length: data.Length).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// 写入单个Bool的数据信息。如果读取的是单bool变量，就直接写变量名，如果是bool数组的一个值，一律带下标访问，例如a[0]。
    /// </summary>
    /// <param name="address">标签的地址数据</param>
    /// <param name="value">bool数据值</param>
    /// <returns>是否写入成功</returns>
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        if (address.StartsWith("i=") && Regex.IsMatch(address, "\\[[0-9]+\\]$"))
        {
            var command = BuildWriteCommand(address.Substring(2), value);
            if (!command.IsSuccess)
            {
                return command;
            }
            var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(continueOnCapturedContext: false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var check = CheckResponse(read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(check);
            }
            return AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
        }
        return await WriteTagAsync(address, 193, !value ? new byte[2] : [255, 255]).ConfigureAwait(continueOnCapturedContext: false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        return await WriteTagAsync(address, 193, value.Select((m) => (byte)(m ? 1 : 0)).ToArray(), !CommunicationHelper.IsAddressEndWithIndex(address) ? 1 : value.Length).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入Byte数据，返回是否写入成功，默认使用类型 0xC2, 如果PLC的变量类型不一样，则需要指定实际的变量类型，例如PLC的变量 A 是0xD1类型，那么地址需要携带类型信息，type=0xD1;A。
    /// </summary>
    /// <param name="address">标签的地址数据</param>
    /// <param name="value">Byte数据</param>
    /// <returns>是否写入成功</returns>
    public virtual async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteTagAsync(address, 194, [value, 0]).ConfigureAwait(false);
    }

    public async Task<OperateResult<DateTime>> ReadDateAsync(string address)
    {
        return await AllenBradleyHelper.ReadDateAsync(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteDateAsync(this, address, date).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeAndDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteTimeAndDateAsync(this, address, date).ConfigureAwait(false);
    }

    public async Task<OperateResult<TimeSpan>> ReadTimeAsync(string address)
    {
        return await AllenBradleyHelper.ReadTimeAsync(this, address).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeAsync(string address, TimeSpan time)
    {
        return await AllenBradleyHelper.WriteTimeAsync(this, address, time).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteTimeOfDateAsync(string address, TimeSpan timeOfDate)
    {
        return await AllenBradleyHelper.WriteTimeOfDateAsync(this, address, timeOfDate).ConfigureAwait(false);
    }

    protected virtual byte[] PackCommandService(byte[] portSlot, params byte[][] cips)
    {
        if (MessageRouter != null)
        {
            portSlot = MessageRouter.GetRouter();
        }
        return AllenBradleyHelper.PackCommandService(portSlot, cips);
    }

    /// <summary>
    /// 获取写入数据的长度信息，此处直接返回数组的长度信息
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数组长度信息</param>
    /// <returns>实际的写入长度信息</returns>
    protected virtual int GetWriteValueLength(string address, int length)
    {
        return length;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AllenBradleyNet[{IpAddress}:{Port}]";
    }
}
