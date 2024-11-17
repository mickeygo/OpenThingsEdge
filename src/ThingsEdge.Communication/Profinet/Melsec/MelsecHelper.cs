using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 所有三菱通讯类的通用辅助工具类，包含了一些通用的静态方法，可以使用本类来获取一些原始的报文信息。详细的操作参见例子<br />
/// All general auxiliary tool classes of Mitsubishi communication class include some general static methods. 
/// You can use this class to get some primitive message information. See the example for detailed operation
/// </summary>
public class MelsecHelper
{
    /// <summary>
    /// 解析A1E协议数据地址<br />
    /// Parse A1E protocol data address
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <returns>结果对象</returns>
    public static OperateResult<MelsecA1EDataType, int> McA1EAnalysisAddress(string address)
    {
        var operateResult = new OperateResult<MelsecA1EDataType, int>();
        try
        {
            switch (address[0])
            {
                case 'T':
                case 't':
                    if (address[1] == 'S' || address[1] == 's')
                    {
                        operateResult.Content1 = MelsecA1EDataType.TS;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TS.FromBase);
                        break;
                    }
                    if (address[1] == 'C' || address[1] == 'c')
                    {
                        operateResult.Content1 = MelsecA1EDataType.TC;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TC.FromBase);
                        break;
                    }
                    if (address[1] == 'N' || address[1] == 'n')
                    {
                        operateResult.Content1 = MelsecA1EDataType.TN;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TN.FromBase);
                        break;
                    }
                    throw new Exception(StringResources.Language.NotSupportedDataType);
                case 'C':
                case 'c':
                    if (address[1] == 'S' || address[1] == 's')
                    {
                        operateResult.Content1 = MelsecA1EDataType.CS;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CS.FromBase);
                        break;
                    }
                    if (address[1] == 'C' || address[1] == 'c')
                    {
                        operateResult.Content1 = MelsecA1EDataType.CC;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CC.FromBase);
                        break;
                    }
                    if (address[1] == 'N' || address[1] == 'n')
                    {
                        operateResult.Content1 = MelsecA1EDataType.CN;
                        operateResult.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CN.FromBase);
                        break;
                    }
                    throw new Exception(StringResources.Language.NotSupportedDataType);
                case 'X':
                case 'x':
                    operateResult.Content1 = MelsecA1EDataType.X;
                    address = address.Substring(1);
                    if (address.StartsWith("0"))
                    {
                        operateResult.Content2 = Convert.ToInt32(address, 8);
                    }
                    else
                    {
                        operateResult.Content2 = Convert.ToInt32(address, MelsecA1EDataType.X.FromBase);
                    }
                    break;
                case 'Y':
                case 'y':
                    operateResult.Content1 = MelsecA1EDataType.Y;
                    address = address.Substring(1);
                    if (address.StartsWith("0"))
                    {
                        operateResult.Content2 = Convert.ToInt32(address, 8);
                    }
                    else
                    {
                        operateResult.Content2 = Convert.ToInt32(address, MelsecA1EDataType.Y.FromBase);
                    }
                    break;
                case 'M':
                case 'm':
                    operateResult.Content1 = MelsecA1EDataType.M;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.M.FromBase);
                    break;
                case 'S':
                case 's':
                    operateResult.Content1 = MelsecA1EDataType.S;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.S.FromBase);
                    break;
                case 'F':
                case 'f':
                    operateResult.Content1 = MelsecA1EDataType.F;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.F.FromBase);
                    break;
                case 'B':
                case 'b':
                    operateResult.Content1 = MelsecA1EDataType.B;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.B.FromBase);
                    break;
                case 'D':
                case 'd':
                    operateResult.Content1 = MelsecA1EDataType.D;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.D.FromBase);
                    break;
                case 'R':
                case 'r':
                    operateResult.Content1 = MelsecA1EDataType.R;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.R.FromBase);
                    break;
                case 'W':
                case 'w':
                    operateResult.Content1 = MelsecA1EDataType.W;
                    operateResult.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.W.FromBase);
                    break;
                default:
                    throw new Exception(StringResources.Language.NotSupportedDataType);
            }
        }
        catch (Exception ex)
        {
            operateResult.Message = ex.Message;
            return operateResult;
        }
        operateResult.IsSuccess = true;
        return operateResult;
    }

    /// <summary>
    /// 根据三菱的错误码去查找对象描述信息
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>描述信息</returns>
    public static string GetErrorDescription(int code)
    {
        switch (code)
        {
            case 2:
                return StringResources.Language.MelsecError02;
            case 81:
                return StringResources.Language.MelsecError51;
            case 82:
                return StringResources.Language.MelsecError52;
            case 84:
                return StringResources.Language.MelsecError54;
            case 85:
                return StringResources.Language.MelsecError55;
            case 86:
                return StringResources.Language.MelsecError56;
            case 88:
                return StringResources.Language.MelsecError58;
            case 89:
                return StringResources.Language.MelsecError59;
            case 17165:
                return StringResources.Language.MelsecError430D;
            case 49229:
                return StringResources.Language.MelsecErrorC04D;
            case 49232:
                return StringResources.Language.MelsecErrorC050;
            case 49233:
            case 49234:
            case 49235:
            case 49236:
                return StringResources.Language.MelsecErrorC051_54;
            case 49237:
                return StringResources.Language.MelsecErrorC055;
            case 49238:
                return StringResources.Language.MelsecErrorC056;
            case 49239:
                return StringResources.Language.MelsecErrorC057;
            case 49240:
                return StringResources.Language.MelsecErrorC058;
            case 49241:
                return StringResources.Language.MelsecErrorC059;
            case 49242:
            case 49243:
                return StringResources.Language.MelsecErrorC05A_B;
            case 49244:
                return StringResources.Language.MelsecErrorC05C;
            case 49245:
                return StringResources.Language.MelsecErrorC05D;
            case 49246:
                return StringResources.Language.MelsecErrorC05E;
            case 49247:
                return StringResources.Language.MelsecErrorC05F;
            case 49248:
                return StringResources.Language.MelsecErrorC060;
            case 49249:
                return StringResources.Language.MelsecErrorC061;
            case 49250:
                return StringResources.Language.MelsecErrorC062;
            case 49264:
                return StringResources.Language.MelsecErrorC070;
            case 49266:
                return StringResources.Language.MelsecErrorC072;
            case 49268:
                return StringResources.Language.MelsecErrorC074;
            default:
                return StringResources.Language.MelsecPleaseReferToManualDocument;
        }
    }

    /// <summary>
    /// 从三菱的地址中构建MC协议的6字节的ASCII格式的地址
    /// </summary>
    /// <param name="address">三菱地址</param>
    /// <param name="type">三菱的数据类型</param>
    /// <returns>6字节的ASCII格式的地址</returns>
    internal static byte[] BuildBytesFromAddress(int address, MelsecMcDataType type)
    {
        return Encoding.ASCII.GetBytes(address.ToString(type.FromBase == 10 ? "D6" : "X6"));
    }

    /// <summary>
    /// 将0，1，0，1的字节数组压缩成三菱格式的字节数组来表示开关量的
    /// </summary>
    /// <param name="value">原始的数据字节</param>
    /// <returns>压缩过后的数据字节</returns>
    internal static byte[] TransBoolArrayToByteData(byte[] value)
    {
        return TransBoolArrayToByteData(value.Select((m) => m != 0).ToArray());
    }

    internal static bool[] TransByteArrayToBoolData(byte[] value, int offset, int length)
    {
        var array = new bool[length > (value.Length - offset) * 2 ? (value.Length - offset) * 2 : length];
        for (var i = 0; i < array.Length; i++)
        {
            if (i % 2 == 0)
            {
                array[i] = (value[offset + i / 2] & 0x10) == 16;
            }
            else
            {
                array[i] = (value[offset + i / 2] & 1) == 1;
            }
        }
        return array;
    }

    /// <summary>
    /// 将bool的组压缩成三菱格式的字节数组来表示开关量的
    /// </summary>
    /// <param name="value">原始的数据字节</param>
    /// <returns>压缩过后的数据字节</returns>
    internal static byte[] TransBoolArrayToByteData(bool[] value)
    {
        var num = (value.Length + 1) / 2;
        var array = new byte[num];
        for (var i = 0; i < num; i++)
        {
            if (value[i * 2])
            {
                array[i] += 16;
            }
            if (i * 2 + 1 < value.Length && value[i * 2 + 1])
            {
                array[i]++;
            }
        }
        return array;
    }

    internal static byte[] TransByteArrayToAsciiByteArray(byte[] value)
    {
        if (value == null)
        {
            return new byte[0];
        }
        var array = new byte[value.Length * 2];
        for (var i = 0; i < value.Length / 2; i++)
        {
            SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(array, 4 * i);
        }
        return array;
    }

    internal static byte[] TransAsciiByteArrayToByteArray(byte[] value)
    {
        var array = new byte[value.Length / 2];
        for (var i = 0; i < array.Length / 2; i++)
        {
            var value2 = Convert.ToUInt16(Encoding.ASCII.GetString(value, i * 4, 4), 16);
            BitConverter.GetBytes(value2).CopyTo(array, i * 2);
        }
        return array;
    }

    /// <summary>
    /// 计算Fx协议指令的和校验信息
    /// </summary>
    /// <param name="data">字节数据</param>
    /// <param name="start">起始的索引信息</param>
    /// <param name="tail">结束的长度信息</param>
    /// <returns>校验之后的数据</returns>
    internal static byte[] FxCalculateCRC(byte[] data, int start = 1, int tail = 2)
    {
        var num = 0;
        for (var i = start; i < data.Length - tail; i++)
        {
            num += data[i];
        }
        return SoftBasic.BuildAsciiBytesFrom((byte)num);
    }

    /// <summary>
    /// 检查指定的和校验是否是正确的
    /// </summary>
    /// <param name="data">字节数据</param>
    /// <returns>是否成功</returns>
    internal static bool CheckCRC(byte[] data)
    {
        var array = FxCalculateCRC(data);
        if (array[0] != data[data.Length - 2])
        {
            return false;
        }
        if (array[1] != data[data.Length - 1])
        {
            return false;
        }
        return true;
    }
}
