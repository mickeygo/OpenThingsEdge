using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Exceptions;
using ThingsEdge.Communication.Profinet.Omron;

namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// 欧姆龙的Fins协议的地址类对象。
/// </summary>
public class OmronFinsAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 进行位操作的指令
    /// </summary>
    public byte BitCode { get; set; }

    /// <summary>
    /// 进行字操作的指令
    /// </summary>
    public byte WordCode { get; set; }

    /// <summary>
    /// 从指定的地址信息解析成真正的设备地址信息。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    public override void Parse(string address, ushort length)
    {
        var operateResult = ParseFrom(address, length, OmronPlcType.CSCJ);
        if (operateResult.IsSuccess)
        {
            AddressStart = operateResult.Content.AddressStart;
            Length = operateResult.Content.Length;
            BitCode = operateResult.Content.BitCode;
            WordCode = operateResult.Content.WordCode;
        }
    }

    /// <summary>
    /// 从实际的欧姆龙的地址里面解析出地址对象。
    /// </summary>
    /// <param name="address">欧姆龙的地址数据信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<OmronFinsAddress> ParseFrom(string address)
    {
        return ParseFrom(address, 0, OmronPlcType.CSCJ);
    }

    private static int CalculateBitIndex(string address)
    {
        var array = address.SplitDot();
        var num = ushort.Parse(array[0]) * 16;
        if (array.Length > 1)
        {
            num += CommunicationHelper.CalculateBitStartIndex(array[1]);
        }
        return num;
    }

    /// <summary>
    /// 从实际的欧姆龙的地址里面解析出地址对象。
    /// </summary>
    /// <param name="address">欧姆龙的地址数据信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="plcType">PLC的类型信息</param>
    /// <returns>是否成功的结果对象</returns>
    public static OperateResult<OmronFinsAddress> ParseFrom(string address, ushort length, OmronPlcType plcType)
    {
        var omronFinsAddress = new OmronFinsAddress();
        try
        {
            omronFinsAddress.Length = length;
            if (address.StartsWith("DR") || address.StartsWith("dr"))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.WordCode = 156;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]) + 48;
                }
                else
                {
                    omronFinsAddress.WordCode = 188;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]) + 8192;
                }
            }
            else if (address.StartsWith("IR") || address.StartsWith("ir"))
            {
                omronFinsAddress.WordCode = 220;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]) + 4096;
            }
            else if (address.StartsWith("DM") || address.StartsWith("dm"))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.DM.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.DM.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]);
            }
            else if (address.StartsWith("TIM") || address.StartsWith("tim"))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 1;
                    omronFinsAddress.WordCode = 129;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.TIM.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.TIM.WordCode;
                }
                omronFinsAddress.AddressStart = CalculateBitIndex(address[3..]);
            }
            else if (address.StartsWith("CNT") || address.StartsWith("cnt"))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 1;
                    omronFinsAddress.WordCode = 129;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[3..]) + 32768;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.TIM.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.TIM.WordCode;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[3..]) + 524288;
                }
            }
            else if (address.StartsWith("CIO") || address.StartsWith("cio"))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 0;
                    omronFinsAddress.WordCode = 128;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.CIO.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.CIO.WordCode;
                }
                omronFinsAddress.AddressStart = CalculateBitIndex(address[3..]);
            }
            else if (address.StartsWith("WR") || address.StartsWith("wr"))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.WR.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.WR.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]);
            }
            else if (address.StartsWith("HR") || address.StartsWith("hr"))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.HR.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.HR.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]);
            }
            else if (address.StartsWith("AR") || address.StartsWith("ar"))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 0;
                    omronFinsAddress.WordCode = 128;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]) + 45056;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.AR.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.AR.WordCode;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]);
                }
            }
            else if (address.StartsWith("CF") || address.StartsWith("cf"))
            {
                omronFinsAddress.BitCode = 7;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[2..]);
            }
            else if (address.StartsWith("EM") || address.StartsWith("em") || address.StartsWith('E') || address.StartsWith('e'))
            {
                if (address.IndexOf('.') > 0)
                {
                    var array = address.SplitDot();
                    var num = Convert.ToInt32(array[0][(address[1] is not 'M' and not 'm' ? 1 : 2)..], 16);
                    if (num < 16)
                    {
                        omronFinsAddress.BitCode = (byte)(32 + num);
                        if (plcType == OmronPlcType.CV)
                        {
                            omronFinsAddress.WordCode = (byte)(144 + num);
                        }
                        else
                        {
                            omronFinsAddress.WordCode = (byte)(160 + num);
                        }
                    }
                    else
                    {
                        omronFinsAddress.BitCode = (byte)(224 + num - 16);
                        omronFinsAddress.WordCode = (byte)(96 + num - 16);
                    }
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[(address.IndexOf('.') + 1)..]);
                }
                else
                {
                    omronFinsAddress.BitCode = 10;
                    omronFinsAddress.WordCode = 152;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[(address[1] != 'M' && address[1] != 'm' ? 1 : 2)..]);
                }
            }
            else if (address.StartsWith('D') || address.StartsWith('d'))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.DM.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.DM.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]);
            }
            else if (address.StartsWith('C') || address.StartsWith('c'))
            {
                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 0;
                    omronFinsAddress.WordCode = 128;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.CIO.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.CIO.WordCode;
                }
                omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]);
            }
            else if (address.StartsWith('W') || address.StartsWith('w'))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.WR.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.WR.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]);
            }
            else if (address.StartsWith('H') || address.StartsWith('h'))
            {
                omronFinsAddress.BitCode = OmronFinsDataType.HR.BitCode;
                omronFinsAddress.WordCode = OmronFinsDataType.HR.WordCode;
                omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]);
            }
            else
            {
                if (!address.StartsWith('A') && !address.StartsWith('a'))
                {
                    throw new CommunicationException(StringResources.Language.NotSupportedDataType);
                }

                if (plcType == OmronPlcType.CV)
                {
                    omronFinsAddress.BitCode = 0;
                    omronFinsAddress.WordCode = 128;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]) + 45056;
                }
                else
                {
                    omronFinsAddress.BitCode = OmronFinsDataType.AR.BitCode;
                    omronFinsAddress.WordCode = OmronFinsDataType.AR.WordCode;
                    omronFinsAddress.AddressStart = CalculateBitIndex(address[1..]);
                }
            }
        }
        catch (Exception ex)
        {
            return new OperateResult<OmronFinsAddress>(GetUnsupportedAddressInfo(address, ex));
        }
        return OperateResult.CreateSuccessResult(omronFinsAddress);
    }
}
