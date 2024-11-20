namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 三菱的FxLinks的消息类
/// </summary>
public class MelsecFxLinksMessage : NetMessageBase, INetMessage
{
    private int _format = 1;

    private bool _sumCheck = true;

    public int ProtocolHeadBytesLength => -1;

    /// <summary>
    /// 指定格式，及是否和校验实例化一个对象
    /// </summary>
    /// <param name="format">格式信息，1, 4</param>
    /// <param name="sumCheck">是否和校验</param>
    public MelsecFxLinksMessage(int format, bool sumCheck)
    {
        _format = format;
        _sumCheck = sumCheck;
    }

    public int GetContentLengthByHeadBytes()
    {
        return 0;
    }

    public override bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        var array = ms.ToArray();
        if (array.Length < 5)
        {
            return false;
        }

        if (_format == 1)
        {
            if (array[0] == 21)
            {
                return array.Length == 7;
            }
            if (array[0] == 6)
            {
                return array.Length == 5;
            }
            if (array[0] == 2)
            {
                if (_sumCheck)
                {
                    return array[^3] == 3;
                }
                return array[^1] == 3;
            }
            return false;
        }
        if (_format == 4)
        {
            return array[^1] == 10 && array[^2] == 13;
        }
        return false;
    }
}
