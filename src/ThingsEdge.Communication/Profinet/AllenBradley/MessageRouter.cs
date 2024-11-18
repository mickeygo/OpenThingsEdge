using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// 自定义的消息路由类，可以实现CIP协议自定义的路由消息。
/// </summary>
public class MessageRouter
{
    private readonly byte[] _router = new byte[6];

    /// <summary>
    /// 背板信息
    /// </summary>
    public byte Backplane
    {
        get
        {
            return _router[0];
        }
        set
        {
            _router[0] = value;
        }
    }

    /// <summary>
    /// 槽号信息
    /// </summary>
    public byte Slot
    {
        get
        {
            return _router[5];
        }
        set
        {
            _router[5] = value;
        }
    }

    /// <summary>
    /// 实例化一个默认的实例对象<br />
    /// instantiate a default instance object
    /// </summary>
    public MessageRouter()
    {
        _router[0] = 1;
        new byte[4] { 15, 2, 18, 1 }.CopyTo(_router, 1);
        _router[5] = 12;
    }

    /// <summary>
    /// 指定路由来实例化一个对象，使用字符串的表示方式。
    /// </summary>
    /// <remarks>
    /// 路有消息支持两种格式，格式1：1.15.2.18.1.12   格式2： 1.1.2.130.133.139.61.1.0。
    /// </remarks>
    /// <param name="router">路由信息</param>
    public MessageRouter(string router)
    {
        var array = router.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
        if (array.Length <= 6)
        {
            if (array.Length != 0)
            {
                _router[0] = byte.Parse(array[0]);
            }
            if (array.Length > 1)
            {
                _router[1] = byte.Parse(array[1]);
            }
            if (array.Length > 2)
            {
                _router[2] = byte.Parse(array[2]);
            }
            if (array.Length > 3)
            {
                _router[3] = byte.Parse(array[3]);
            }
            if (array.Length > 4)
            {
                _router[4] = byte.Parse(array[4]);
            }
            if (array.Length > 5)
            {
                _router[5] = byte.Parse(array[5]);
            }
        }
        else if (array.Length == 9)
        {
            var text = array[3] + "." + array[4] + "." + array[5] + "." + array[6];
            _router = new byte[6 + text.Length];
            _router[0] = byte.Parse(array[0]);
            _router[1] = byte.Parse(array[1]);
            _router[2] = (byte)(16 + byte.Parse(array[2]));
            _router[3] = (byte)text.Length;
            Encoding.ASCII.GetBytes(text).CopyTo(_router, 4);
            _router[^2] = byte.Parse(array[7]);
            _router[^1] = byte.Parse(array[8]);
        }
    }

    /// <summary>
    /// 使用完全自定义的消息路由来初始化数据。
    /// </summary>
    /// <param name="router">完全自定义的路由消息</param>
    public MessageRouter(byte[] router)
    {
        _router = router;
    }

    /// <summary>
    /// 获取路由信息
    /// </summary>
    /// <returns>路由消息的字节信息</returns>
    public byte[] GetRouter()
    {
        return _router;
    }

    /// <summary>
    /// 获取用于发送的CIP路由报文信息。
    /// </summary>
    /// <returns>路由信息</returns>
    public byte[] GetRouterCIP()
    {
        var array = GetRouter();
        if (array.Length % 2 == 1)
        {
            array = SoftBasic.SpliceArray(array, new byte[1]);
        }
        var array2 = new byte[46 + array.Length];
        "54022006240105f70200 00800100fe8002001b05 28a7fd03020000008084 1e00f44380841e00f443 a305".ToHexBytes().CopyTo(array2, 0);
        array.CopyTo(array2, 42);
        "20022401".ToHexBytes().CopyTo(array2, 42 + array.Length);
        array2[41] = (byte)(array.Length / 2);
        return array2;
    }
}
