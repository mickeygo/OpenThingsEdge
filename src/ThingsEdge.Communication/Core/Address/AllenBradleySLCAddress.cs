using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 罗克韦尔PLC的地址信息
/// </summary>
public class AllenBradleySLCAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置等待读取的数据的代码。
    /// </summary>
    public byte DataCode { get; set; }

    /// <summary>
    /// 获取或设置PLC的DB块数据信息。
    /// </summary>
    public ushort DbBlock { get; set; }

    /// <summary>
    /// 从指定的地址信息解析成真正的设备地址信息。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    public override void Parse(string address, ushort length)
    {
        var operateResult = ParseFrom(address, length);
        if (operateResult.IsSuccess)
        {
            AddressStart = operateResult.Content.AddressStart;
            Length = operateResult.Content.Length;
            DataCode = operateResult.Content.DataCode;
            DbBlock = operateResult.Content.DbBlock;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return DataCode switch
        {
            142 => $"A{DbBlock}:{AddressStart}",
            133 => $"B{DbBlock}:{AddressStart}",
            137 => $"N{DbBlock}:{AddressStart}",
            138 => $"F{DbBlock}:{AddressStart}",
            141 => $"ST{DbBlock}:{AddressStart}",
            132 => $"S{DbBlock}:{AddressStart}",
            135 => $"C{DbBlock}:{AddressStart}",
            131 => $"I{DbBlock}:{AddressStart}",
            130 => $"O{DbBlock}:{AddressStart}",
            136 => $"R{DbBlock}:{AddressStart}",
            134 => $"T{DbBlock}:{AddressStart}",
            145 => $"L{DbBlock}:{AddressStart}",
            _ => AddressStart.ToString(),
        };
    }

    /// <summary>
    /// 从实际的罗克韦尔的地址里面解析出地址对象，例如 A9:0。
    /// </summary>
    /// <param name="address">实际的地址数据信息，例如 A9:0</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<AllenBradleySLCAddress> ParseFrom(string address)
    {
        return ParseFrom(address, 0);
    }

    /// <summary>
    /// 从实际的罗克韦尔的地址里面解析出地址对象，例如 A9:0。
    /// </summary>
    /// <param name="address">实际的地址数据信息，例如 A9:0</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<AllenBradleySLCAddress> ParseFrom(string address, ushort length)
    {
        if (!address.Contains(':'))
        {
            return new OperateResult<AllenBradleySLCAddress>("Address can't find ':', example : A9:0");
        }

        var array = address.Split(':');
        try
        {
            var allenBradleySLCAddress = new AllenBradleySLCAddress();
            switch (array[0][0])
            {
                case 'A':
                    allenBradleySLCAddress.DataCode = 142;
                    break;
                case 'B':
                    allenBradleySLCAddress.DataCode = 133;
                    break;
                case 'N':
                    allenBradleySLCAddress.DataCode = 137;
                    break;
                case 'F':
                    allenBradleySLCAddress.DataCode = 138;
                    break;
                case 'S':
                    if (array[0].Length > 1 && array[0][1] == 'T')
                    {
                        allenBradleySLCAddress.DataCode = 141;
                    }
                    else
                    {
                        allenBradleySLCAddress.DataCode = 132;
                    }
                    break;
                case 'C':
                    allenBradleySLCAddress.DataCode = 135;
                    break;
                case 'I':
                    allenBradleySLCAddress.DataCode = 131;
                    break;
                case 'O':
                    allenBradleySLCAddress.DataCode = 130;
                    break;
                case 'R':
                    allenBradleySLCAddress.DataCode = 136;
                    break;
                case 'T':
                    allenBradleySLCAddress.DataCode = 134;
                    break;
                case 'L':
                    allenBradleySLCAddress.DataCode = 145;
                    break;
                default:
                    throw new CommunicationException("Address code wrong, must be A,B,N,F,S,C,I,O,R,T,ST,L");
            }

            allenBradleySLCAddress.DbBlock = allenBradleySLCAddress.DataCode switch
            {
                132 => (ushort)(array[0].Length == 1 ? 2 : ushort.Parse(array[0].Substring(1))),
                130 => (ushort)(array[0].Length != 1 ? ushort.Parse(array[0].Substring(1)) : 0),
                131 => (ushort)(array[0].Length == 1 ? 1 : ushort.Parse(array[0].Substring(1))),
                141 => (ushort)(array[0].Length == 2 ? 1 : ushort.Parse(array[0].Substring(2))),
                _ => ushort.Parse(array[0].Substring(1)),
            };
            allenBradleySLCAddress.AddressStart = ushort.Parse(array[1]);
            return OperateResult.CreateSuccessResult(allenBradleySLCAddress);
        }
        catch (Exception ex)
        {
            return new OperateResult<AllenBradleySLCAddress>("Wrong Address format: " + ex.Message);
        }
    }
}
