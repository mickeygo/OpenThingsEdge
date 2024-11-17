using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// ModbusRtu的协议类信息
/// </summary>
public class ModbusRtuMessage : NetMessageBase, INetMessage
{
    /// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
    public int ProtocolHeadBytesLength => -1;

    /// <inheritdoc cref="P:HslCommunication.ModBus.ModbusRtu.StationCheckMacth" />
    public bool StationCheckMacth { get; set; } = true;


    /// <summary>
    /// 指定是否检查站号来实例化一个对象
    /// </summary>
    /// <param name="stationCheck">是否检查站号</param>
    public ModbusRtuMessage(bool stationCheck)
    {
        StationCheckMacth = stationCheck;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckReceiveDataComplete(System.Byte[],System.IO.MemoryStream)" />
    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return ModbusInfo.CheckRtuReceiveDataComplete(send, ms.ToArray());
    }

    /// <inheritdoc />
    public override int CheckMessageMatch(byte[] send, byte[] receive)
    {
        if (!StationCheckMacth)
        {
            return 1;
        }
        return ModbusInfo.CheckRtuMessageMatch(send, receive);
    }
}
