using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// ModbusRtu的协议类信息
/// </summary>
public class ModbusRtuMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => -1;

    public bool StationCheckMacth { get; set; } = true;

    /// <summary>
    /// 指定是否检查站号来实例化一个对象
    /// </summary>
    /// <param name="stationCheck">是否检查站号</param>
    public ModbusRtuMessage(bool stationCheck)
    {
        StationCheckMacth = stationCheck;
    }

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

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
