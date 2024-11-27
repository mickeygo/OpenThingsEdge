using System.Net.Sockets;

namespace ThingsEdge.Communication.Common.Extensions;

internal static class SocketExtensions
{
    /// <summary>
    /// 设置套接字的活动时间和活动间歇时间，此值会设置到socket低级别的控制中，传入值如果为负数，则表示不使用 KeepAlive 功能。
    /// </summary>
    /// <param name="socket">套接字对象</param>
    /// <param name="keepAliveTime">保持活动时间。单位 ms</param>
    /// <param name="keepAliveInterval">保持活动的间歇时间，单位 ms</param>
    /// <returns>返回获取的参数的字节</returns>
    public static void SetKeepAlive(this Socket socket, int keepAliveTime, int keepAliveInterval)
    {
        if (keepAliveTime <= 0)
        {
            return;
        }

        try
        {
            var size = sizeof(uint);
            var optionInValue = new byte[size * 3];
            BitConverter.GetBytes(1u).CopyTo(optionInValue, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(optionInValue, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(optionInValue, size * 2);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                socket.IOControl(IOControlCode.KeepAliveValues, optionInValue, null);
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionInValue);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// 安全关闭 Socket 
    /// </summary>
    /// <param name="socket"></param>
    public static void SafeClose(this Socket? socket)
    {
        try
        {
            socket?.Close();
        }
        catch
        {
            Debug.WriteLine("Close the socket exception.");
        }
    }
}
