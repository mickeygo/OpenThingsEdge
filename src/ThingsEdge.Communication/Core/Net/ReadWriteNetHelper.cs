using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 读写网络的辅助类
/// </summary>
public static class ReadWriteNetHelper
{
    /// <summary>
    /// 写入位到字寄存器的功能，该功能先读取字寄存器的字数据，然后修改其中的位，再写入回去，可能存在脏数据的风险。
    /// </summary>
    /// <remarks>
    /// 关于脏数据风险：从读取数据，修改位，再次写入数据时，大概需要经过3ms~10ms不等的时间，如果此期间内PLC修改了该字寄存器的其他位，
    /// 再次写入数据时会恢复该点位的数据到读取时的初始值，可能引发设备故障，请谨慎开启此功能。
    /// </remarks>
    /// <param name="readWrite">通信对象信息</param>
    /// <param name="address">写入的地址信息，需要携带'.'号</param>
    /// <param name="values">写入的值信息</param>
    /// <param name="addLength">多少长度的bit位组成一个字地址信息</param>
    /// <param name="reverseWord">对原始数据是否按照字单位进行反转操作</param>
    /// <param name="bitStr">额外指定的位索引，如果为空，则使用<paramref name="address" />中的地址位偏移信息</param>
    /// <returns>是否写入成功</returns>
    public static async Task<OperateResult> WriteBoolWithWordAsync(IReadWriteNet readWrite, string address, bool[] values, int addLength = 16, bool reverseWord = false, string? bitStr = null)
    {
        var adds = address.SplitDot();
        var bit = 0;
        try
        {
            if (string.IsNullOrEmpty(bitStr))
            {
                if (adds.Length > 1)
                {
                    bit = Convert.ToInt32(adds[1]);
                }
            }
            else
            {
                bit = Convert.ToInt32(bitStr);
            }
        }
        catch (Exception ex2)
        {
            var ex = ex2;
            return new OperateResult(address + " Bit index input wrong: " + ex.Message);
        }

        var read = await readWrite.ReadAsync(length: (ushort)((bit + values.Length + addLength - 1) / addLength), address: adds[0]).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }

        var array = reverseWord ? read.Content.ReverseByWord().ToBoolArray() : read.Content.ToBoolArray();
        if (bit + values.Length <= array.Length)
        {
            values.CopyTo(array, bit);
        }
        return await readWrite.WriteAsync(adds[0], reverseWord ? array.ToByteArray().ReverseByWord() : array.ToByteArray()).ConfigureAwait(false);
    }
}
