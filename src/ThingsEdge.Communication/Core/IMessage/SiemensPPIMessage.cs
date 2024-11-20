using ThingsEdge.Communication.Profinet.Siemens.Helper;

namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 西门子PPI协议的消息类
/// </summary>
public class SiemensPPIMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => -1;

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return SiemensPPIHelper.CheckReceiveDataComplete(ms);
    }
}
