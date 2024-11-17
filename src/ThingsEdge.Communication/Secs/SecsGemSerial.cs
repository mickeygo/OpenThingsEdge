using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Secs;

/// <summary>
/// 串口类相关的Secs
/// </summary>
public class SecsGemSerial : DeviceSerialPort
{
    private SoftIncrementCount incrementCount;

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public SecsGemSerial()
    {
        incrementCount = new SoftIncrementCount(4294967295L, 0L);
        ByteTransform = new ReverseBytesTransform();
        WordLength = 2;
    }

    /// <summary>
    /// 执行SECS命令
    /// </summary>
    /// <param name="command">命令信息</param>
    /// <returns>是否成功的结果</returns>
    public OperateResult<byte[]> ExecuteCommand(byte[] command)
    {
        var operateResult = ReadFromCoreServer(new byte[1] { 5 });
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        if (operateResult.Content[0] != 4)
        {
            return new OperateResult<byte[]>($"Send Enq to device, but receive [{operateResult.Content[0]}], need receive [EOT]");
        }
        OperateResult<byte[]> operateResult2;
        do
        {
            operateResult2 = ReadFromCoreServer(command);
        }
        while (operateResult2.IsSuccess);
        return operateResult2;
    }
}
