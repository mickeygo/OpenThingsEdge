using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Serial;

namespace ThingsEdge.Communication.Secs.Helper;

/// <summary>
/// Secs-1的协议信息
/// </summary>
public static class Secs1
{
    /// <summary>
    /// 根据传入的参数信息，构建完整的SECS消息报文列表
    /// </summary>
    /// <param name="deviceID">装置识别码</param>
    /// <param name="streamNo">主功能码</param>
    /// <param name="functionNo">子功能码</param>
    /// <param name="blockNo">数据块号</param>
    /// <param name="messageID">消息序号</param>
    /// <param name="data">真实数据消息</param>
    /// <param name="wBit">是否必须回复讯息</param>
    /// <returns>完整的报文信息</returns>
    public static List<byte[]> BuildSecsOneMessage(ushort deviceID, byte streamNo, byte functionNo, ushort blockNo, uint messageID, byte[] data, bool wBit)
    {
        var list = new List<byte[]>();
        var list2 = data.Length <= 244 ? SoftBasic.ArraySplitByLength(data, 244) : SoftBasic.ArraySplitByLength(data, 224);
        for (var i = 0; i < list2.Count; i++)
        {
            var array = new byte[13 + list2[i].Length];
            array[0] = (byte)(10 + list2[i].Length);
            array[1] = BitConverter.GetBytes(deviceID)[1];
            array[2] = BitConverter.GetBytes(deviceID)[0];
            array[3] = wBit ? (byte)(streamNo | 0x80u) : streamNo;
            array[4] = functionNo;
            array[5] = i == list2.Count - 1 ? (byte)(BitConverter.GetBytes(blockNo)[1] | 0x80u) : BitConverter.GetBytes(blockNo)[1];
            array[6] = BitConverter.GetBytes(blockNo)[0];
            array[7] = BitConverter.GetBytes(messageID)[3];
            array[8] = BitConverter.GetBytes(messageID)[2];
            array[9] = BitConverter.GetBytes(messageID)[1];
            array[10] = BitConverter.GetBytes(messageID)[0];
            list2[i].CopyTo(array, 11);
            var value = SoftLRC.CalculateAcc(array, 1, 2);
            array[array.Length - 2] = BitConverter.GetBytes(value)[1];
            array[array.Length - 1] = BitConverter.GetBytes(value)[0];
            list.Add(array);
        }
        return list;
    }

    /// <summary>
    /// 根据传入的参数信息，构建完整的SECS/HSMS消息报文列表
    /// </summary>
    /// <param name="deviceID">装置识别码</param>
    /// <param name="streamNo">主功能码</param>
    /// <param name="functionNo">子功能码</param>
    /// <param name="blockNo">数据块号</param>
    /// <param name="messageID">消息序号</param>
    /// <param name="data">真实数据消息</param>
    /// <param name="wBit">是否必须回复讯息</param>
    /// <returns>完整的报文信息</returns>
    public static byte[] BuildHSMSMessage(ushort deviceID, byte streamNo, byte functionNo, ushort blockNo, uint messageID, byte[] data, bool wBit)
    {
        data ??= [];
        var array = new byte[14 + data.Length];
        array[0] = BitConverter.GetBytes(array.Length - 4)[3];
        array[1] = BitConverter.GetBytes(array.Length - 4)[2];
        array[2] = BitConverter.GetBytes(array.Length - 4)[1];
        array[3] = BitConverter.GetBytes(array.Length - 4)[0];
        array[4] = BitConverter.GetBytes(deviceID)[1];
        array[5] = BitConverter.GetBytes(deviceID)[0];
        array[6] = wBit ? (byte)(streamNo | 0x80u) : streamNo;
        array[7] = functionNo;
        array[8] = BitConverter.GetBytes(blockNo)[1];
        array[9] = BitConverter.GetBytes(blockNo)[0];
        array[10] = BitConverter.GetBytes(messageID)[3];
        array[11] = BitConverter.GetBytes(messageID)[2];
        array[12] = BitConverter.GetBytes(messageID)[1];
        array[13] = BitConverter.GetBytes(messageID)[0];
        data.CopyTo(array, 14);
        return array;
    }
}
