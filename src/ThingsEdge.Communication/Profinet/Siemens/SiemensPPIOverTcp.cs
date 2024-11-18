using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.Siemens.Helper;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <inheritdoc cref="T:HslCommunication.Profinet.Siemens.SiemensPPI" />
public class SiemensPPIOverTcp : DeviceTcpNet, ISiemensPPI, IReadWriteNet
{
    private byte station = 2;

    private object communicationLock;

    /// <inheritdoc cref="P:HslCommunication.Profinet.Siemens.SiemensPPI.Station" />
    public byte Station
    {
        get
        {
            return station;
        }
        set
        {
            station = value;
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.#ctor" />
    public SiemensPPIOverTcp()
    {
        WordLength = 2;
        ByteTransform = new ReverseBytesTransform();
        communicationLock = new object();
    }

    /// <summary>
    /// 使用指定的ip地址和端口号来实例化对象<br />
    /// Instantiate the object with the specified IP address and port number
    /// </summary>
    /// <param name="ipAddress">Ip地址信息</param>
    /// <param name="port">端口号信息</param>
    public SiemensPPIOverTcp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SiemensPPIMessage();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Read(System.String,System.UInt16)" />
    [HslMqttApi("ReadByteArray", "")]
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return SiemensPPIHelper.Read(this, address, length, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    [HslMqttApi("ReadBool", "")]
    public override OperateResult<bool> ReadBool(string address)
    {
        return SiemensPPIHelper.ReadBool(this, address, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    [HslMqttApi("ReadBoolArray", "")]
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return SiemensPPIHelper.ReadBool(this, address, length, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Write(System.String,System.Byte[])" />
    [HslMqttApi("WriteByteArray", "")]
    public override OperateResult Write(string address, byte[] value)
    {
        return SiemensPPIHelper.Write(this, address, value, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] value)
    {
        return SiemensPPIHelper.Write(this, address, value, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.ReadByte(System.String)" />
    [HslMqttApi("ReadByte", "")]
    public OperateResult<byte> ReadByte(string address)
    {
        return ByteTransformHelper.GetResultFromArray(Read(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Write(System.String,System.Byte)" />
    [HslMqttApi("WriteByte", "")]
    public OperateResult Write(string address, byte value)
    {
        return Write(address, new byte[1] { value });
    }

    /// <inheritdoc />
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return await Task.Run(() => ReadBool(address));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPIOverTcp.ReadByte(System.String)" />
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPIOverTcp.Write(System.String,System.Byte)" />
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteAsync(address, new byte[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Start(System.String)" />
    [HslMqttApi]
    public OperateResult Start(string parameter = "")
    {
        return SiemensPPIHelper.Start(this, parameter, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Stop(System.String)" />
    [HslMqttApi]
    public OperateResult Stop(string parameter = "")
    {
        return SiemensPPIHelper.Stop(this, parameter, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    [HslMqttApi]
    public OperateResult<string> ReadPlcType(string parameter = "")
    {
        return SiemensPPIHelper.ReadPlcType(this, parameter, Station, communicationLock);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Start(System.String)" />
    public async Task<OperateResult> StartAsync(string parameter = "")
    {
        return await Task.Run(() => Start(parameter));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.SiemensPPI.Stop(System.String)" />
    public async Task<OperateResult> StopAsync(string parameter = "")
    {
        return await Task.Run(() => Stop(parameter));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Siemens.Helper.SiemensPPIHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte,System.Object)" />
    public async Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
    {
        return await Task.Run(() => SiemensPPIHelper.ReadPlcType(this, parameter, Station, communicationLock));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"SiemensPPIOverTcp[{IpAddress}:{Port}]";
    }
}
