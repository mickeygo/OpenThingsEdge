using System.Text.RegularExpressions;
using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Robot.YASKAWA.Helper;

/// <summary>
/// 安川机器人的静态辅助方法
/// </summary>
public static class YRCHelper
{
    /// <summary>
    /// 根据错误信息获取安川机器人的错误信息文本。
    /// </summary>
    /// <param name="err">错误号</param>
    /// <returns>错误文本信息</returns>
    public static string GetErrorMessage(int err)
    {
        return err switch
        {
            1010 => StringResources.Language.YRC1010,
            1011 => StringResources.Language.YRC1011,
            1012 => StringResources.Language.YRC1012,
            1013 => StringResources.Language.YRC1013,
            1020 => StringResources.Language.YRC1020,
            2010 => StringResources.Language.YRC2010,
            2020 => StringResources.Language.YRC2020,
            2030 => StringResources.Language.YRC2030,
            2040 => StringResources.Language.YRC2040,
            2050 => StringResources.Language.YRC2050,
            2060 => StringResources.Language.YRC2060,
            2070 => StringResources.Language.YRC2070,
            2080 => StringResources.Language.YRC2080,
            2090 => StringResources.Language.YRC2090,
            2100 => StringResources.Language.YRC2100,
            2110 => StringResources.Language.YRC2110,
            2120 => StringResources.Language.YRC2120,
            2130 => StringResources.Language.YRC2130,
            2150 => StringResources.Language.YRC2150,
            3010 => StringResources.Language.YRC3010,
            3040 => StringResources.Language.YRC3040,
            3050 => StringResources.Language.YRC3050,
            3070 => StringResources.Language.YRC3070,
            3220 => StringResources.Language.YRC3220,
            3230 => StringResources.Language.YRC3230,
            3350 => StringResources.Language.YRC3350,
            3360 => StringResources.Language.YRC3360,
            3370 => StringResources.Language.YRC3370,
            3380 => StringResources.Language.YRC3380,
            3390 => StringResources.Language.YRC3390,
            3400 => StringResources.Language.YRC3400,
            3410 => StringResources.Language.YRC3410,
            3420 => StringResources.Language.YRC3420,
            3430 => StringResources.Language.YRC3430,
            3450 => StringResources.Language.YRC3450,
            3460 => StringResources.Language.YRC3460,
            4010 => StringResources.Language.YRC4010,
            4012 => StringResources.Language.YRC4012,
            4020 => StringResources.Language.YRC4020,
            4030 => StringResources.Language.YRC4030,
            4040 => StringResources.Language.YRC4040,
            4060 => StringResources.Language.YRC4060,
            4120 => StringResources.Language.YRC4120,
            4130 => StringResources.Language.YRC4130,
            4140 => StringResources.Language.YRC4140,
            4150 => StringResources.Language.YRC4150,
            4170 => StringResources.Language.YRC4170,
            4190 => StringResources.Language.YRC4190,
            4200 => StringResources.Language.YRC4200,
            4230 => StringResources.Language.YRC4230,
            4420 => StringResources.Language.YRC4420,
            4430 => StringResources.Language.YRC4430,
            4480 => StringResources.Language.YRC4480,
            4490 => StringResources.Language.YRC4490,
            5110 => StringResources.Language.YRC5110,
            5120 => StringResources.Language.YRC5120,
            5130 => StringResources.Language.YRC5130,
            5170 => StringResources.Language.YRC5170,
            5180 => StringResources.Language.YRC5180,
            5200 => StringResources.Language.YRC5200,
            5310 => StringResources.Language.YRC5310,
            5340 => StringResources.Language.YRC5340,
            5370 => StringResources.Language.YRC5370,
            5390 => StringResources.Language.YRC5390,
            5430 => StringResources.Language.YRC5430,
            5480 => StringResources.Language.YRC5480,
            _ => StringResources.Language.UnknownError,
        };
    }

    /// <summary>
    /// 当机器人返回ERROR的错误指令后，检测消息里面是否有相关的错误码数据，如果存在，就解析出错误对应的文本。
    /// </summary>
    /// <param name="errText">返回的完整的报文</param>
    /// <returns>带有错误文本的数据信息</returns>
    public static OperateResult<string> ExtraErrorMessage(string errText)
    {
        var match = Regex.Match(errText, "\\([0-9]+\\)\\.$");
        if (match.Success)
        {
            var s = match.Value[1..^2];
            if (int.TryParse(s, out var result))
            {
                return new OperateResult<string>(errText + Environment.NewLine + GetErrorMessage(result));
            }
            return new OperateResult<string>(errText);
        }
        return new OperateResult<string>(errText);
    }
}
