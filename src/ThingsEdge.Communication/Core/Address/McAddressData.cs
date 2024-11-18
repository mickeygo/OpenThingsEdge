using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;
using ThingsEdge.Communication.Profinet.Melsec;
using ThingsEdge.Communication.Profinet.Panasonic;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 三菱的数据地址表示形式。
/// </summary>
public class McAddressData : DeviceAddressDataBase
{
    /// <summary>
    /// 三菱的数据类型及地址信息。
    /// </summary>
    public MelsecMcDataType McDataType { get; set; }

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public McAddressData()
    {
        McDataType = MelsecMcDataType.D;
    }

    /// <summary>
    /// 从指定的地址信息解析成真正的设备地址信息，默认是三菱的地址。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    public override void Parse(string address, ushort length)
    {
        var operateResult = ParseMelsecFrom(address, length, isBit: false);
        if (operateResult.IsSuccess)
        {
            AddressStart = operateResult.Content!.AddressStart;
            Length = operateResult.Content.Length;
            McDataType = operateResult.Content.McDataType;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return McDataType.AsciiCode.Replace("*", "") + Convert.ToString(AddressStart, McDataType.FromBase);
    }

    /// <summary>
    /// 从实际三菱的地址里面解析出我们需要的地址类型。
    /// </summary>
    /// <param name="address">三菱的地址数据信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="isBit">是否读写bool的操作</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<McAddressData> ParseMelsecFrom(string address, ushort length, bool isBit)
    {
        var mcAddressData = new McAddressData
        {
            Length = length
        };
        try
        {
            switch (address[0])
            {
                case 'M' or 'm':
                    mcAddressData.McDataType = MelsecMcDataType.M;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.M.FromBase);
                    break;
                case 'X' or 'x':
                    mcAddressData.McDataType = MelsecMcDataType.X;
                    address = address.Substring(1);
                    if (address.StartsWith('0'))
                    {
                        mcAddressData.AddressStart = Convert.ToInt32(address, 8);
                    }
                    else
                    {
                        mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.X.FromBase);
                    }
                    break;
                case 'Y' or 'y':
                    mcAddressData.McDataType = MelsecMcDataType.Y;
                    address = address[1..];
                    if (address.StartsWith('0'))
                    {
                        mcAddressData.AddressStart = Convert.ToInt32(address, 8);
                    }
                    else
                    {
                        mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.Y.FromBase);
                    }
                    break;
                case 'D' or 'd':
                    if (address[1] is 'X' or 'x')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.DX;
                        address = address[2..];
                        if (address.StartsWith('0'))
                        {
                            mcAddressData.AddressStart = Convert.ToInt32(address, 8);
                        }
                        else
                        {
                            mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.DX.FromBase);
                        }
                    }
                    else if (address[1] is 'Y' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.DY;
                        address = address[2..];
                        if (address.StartsWith('0'))
                        {
                            mcAddressData.AddressStart = Convert.ToInt32(address, 8);
                        }
                        else
                        {
                            mcAddressData.AddressStart = Convert.ToInt32(address, MelsecMcDataType.DY.FromBase);
                        }
                    }
                    else
                    {
                        mcAddressData.McDataType = MelsecMcDataType.D;
                        mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.D.FromBase);
                    }
                    break;
                case 'W' or 'w':
                    mcAddressData.McDataType = MelsecMcDataType.W;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.W.FromBase);
                    break;
                case 'L' or 'l':
                    mcAddressData.McDataType = MelsecMcDataType.L;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.L.FromBase);
                    break;
                case 'F' or 'f':
                    mcAddressData.McDataType = MelsecMcDataType.F;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.F.FromBase);
                    break;
                case 'V' or 'v':
                    mcAddressData.McDataType = MelsecMcDataType.V;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.V.FromBase);
                    break;
                case 'B' or 'b':
                    mcAddressData.McDataType = MelsecMcDataType.B;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.B.FromBase);
                    break;
                case 'R' or 'r':
                    mcAddressData.McDataType = MelsecMcDataType.R;
                    mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.R.FromBase);
                    break;
                case 'S' or 's':
                    if (address[1] is 'N' or 'n')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SN;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SN.FromBase);
                    }
                    else if (address[1] is 'S' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SS;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SS.FromBase);
                    }
                    else if (address[1] is 'C' or 'c')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SC;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SC.FromBase);
                    }
                    else if (address[1] is 'M' or 'm')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SM;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SM.FromBase);
                    }
                    else if (address[1] is 'D' or 'd')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SD;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SD.FromBase);
                    }
                    else if (address[1] is 'B' or 'b')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SB;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SB.FromBase);
                    }
                    else if (address[1] is 'W' or 'w')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.SW;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.SW.FromBase);
                    }
                    else
                    {
                        mcAddressData.McDataType = MelsecMcDataType.S;
                        mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.S.FromBase);
                    }
                    break;
                case 'Z' or 'z':
                    if (address.StartsWith("ZR") || address.StartsWith("zr"))
                    {
                        mcAddressData.McDataType = MelsecMcDataType.ZR;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.ZR.FromBase);
                    }
                    else
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Z;
                        mcAddressData.AddressStart = Convert.ToInt32(address[1..], MelsecMcDataType.Z.FromBase);
                    }
                    break;
                case 'T' or 't':
                    if (address[1] is 'N' or 'n')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.TN;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TN.FromBase);
                        break;
                    }
                    if (address[1] is 'S' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.TS;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TS.FromBase);
                        break;
                    }
                    if (address[1] is 'C' or 'c')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.TC;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.TC.FromBase);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'C' or 'c':
                    if (address[1] is 'N' or 'n')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.CN;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CN.FromBase);
                        break;
                    }
                    if (address[1] is 'S' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.CS;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CS.FromBase);
                        break;
                    }
                    if (address[1] is 'C' or 'c')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.CC;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..], MelsecMcDataType.CC.FromBase);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                default:
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<McAddressData>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(mcAddressData);
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Address.McAddressData.ParseMelsecFrom(System.String,System.UInt16,System.Boolean)" />
    public static OperateResult<McAddressData> ParseMelsecRFrom(string address, ushort length, bool isBit)
    {
        var operateResult = MelsecMcRNet.AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<McAddressData>(operateResult);
        }
        return OperateResult.CreateSuccessResult(new McAddressData
        {
            McDataType = operateResult.Content1,
            AddressStart = operateResult.Content2,
            Length = length
        });
    }

    /// <summary>
    /// 从实际松下的地址里面解析出MC协议标准的地址对象
    /// </summary>
    /// <param name="address">松下的地址数据信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="isBit">是否进行bool类型的读写操作</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<McAddressData> ParsePanasonicFrom(string address, ushort length, bool isBit)
    {
        var mcAddressData = new McAddressData
        {
            Length = length
        };
        try
        {
            switch (address[0])
            {
                case 'R' or 'r':
                    {
                        var num2 = PanasonicHelper.CalculateComplexAddress(address[1..]);
                        if (num2 < 14400)
                        {
                            mcAddressData.McDataType = MelsecMcDataType.Panasonic_R;
                            mcAddressData.AddressStart = num2;
                        }
                        else
                        {
                            mcAddressData.McDataType = MelsecMcDataType.Panasonic_SM;
                            mcAddressData.AddressStart = num2 - 14400;
                        }
                        break;
                    }
                case 'X' or 'x':
                    mcAddressData.McDataType = MelsecMcDataType.Panasonic_X;
                    mcAddressData.AddressStart = PanasonicHelper.CalculateComplexAddress(address[1..]);
                    break;
                case 'Y' or 'y':
                    mcAddressData.McDataType = MelsecMcDataType.Panasonic_Y;
                    mcAddressData.AddressStart = PanasonicHelper.CalculateComplexAddress(address[1..]);
                    break;
                case 'L' or 'l':
                    if (address[1] is 'D' or 'd')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_LD;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                    }
                    else
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_L;
                        mcAddressData.AddressStart = PanasonicHelper.CalculateComplexAddress(address[1..]);
                    }
                    break;
                case 'D' or 'd':
                    {
                        var num = Convert.ToInt32(address[1..]);
                        if (num < 90000)
                        {
                            mcAddressData.McDataType = MelsecMcDataType.Panasonic_DT;
                            mcAddressData.AddressStart = Convert.ToInt32(address[1..]);
                        }
                        else
                        {
                            mcAddressData.McDataType = MelsecMcDataType.Panasonic_SD;
                            mcAddressData.AddressStart = Convert.ToInt32(address[1..]) - 90000;
                        }
                        break;
                    }
                case 'T' or 't':
                    if (address[1] is 'N' or 'n')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_TN;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                        break;
                    }
                    if (address[1] is 'S' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_TS;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'C' or 'c':
                    if (address[1] is 'N' or 'n')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_CN;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                        break;
                    }
                    if (address[1] is 'S' or 's')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_CS;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                case 'S' or 's':
                    if (address[1] is 'D' or 'd')
                    {
                        mcAddressData.McDataType = MelsecMcDataType.Panasonic_SD;
                        mcAddressData.AddressStart = Convert.ToInt32(address[2..]);
                        break;
                    }
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                default:
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<McAddressData>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(mcAddressData);
    }
}
