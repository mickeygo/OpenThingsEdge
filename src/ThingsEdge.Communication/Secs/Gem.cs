using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Secs.Types;

namespace ThingsEdge.Communication.Secs;

/// <summary>
/// GEM相关的数据读写信息
/// </summary>
public class Gem
{
    private ISecs secs;

    /// <summary>
    /// 使用指定的 <see cref="T:HslCommunication.Secs.Types.ISecs" /> 接口来初始化 GEM 对象，然后进行数据读写操作
    /// </summary>
    /// <param name="secs">Secs的通信对象</param>
    public Gem(ISecs secs)
    {
        this.secs = secs;
    }

    /// <summary>
    /// S1F1的功能方法
    /// </summary>
    /// <returns>在线数据信息</returns>
    public OperateResult<OnlineData> S1F1_AreYouThere()
    {
        var operateResult = secs.ReadSecsMessage(1, 1, new SecsValue(), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<OnlineData>(operateResult);
        }
        return OperateResult.CreateSuccessResult((OnlineData)operateResult.Content.GetItemValues());
    }

    /// <summary>
    /// S1F11的功能方法
    /// </summary>
    /// <returns>变量名称数组</returns>
    public OperateResult<VariableName[]> S1F11_StatusVariableNamelist()
    {
        var operateResult = secs.ReadSecsMessage(1, 11, new SecsValue(), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<VariableName[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content.GetItemValues().ToVaruableNames());
    }

    /// <summary>
    /// S1F11的功能方法，带参数传递
    /// </summary>
    /// <param name="statusVaruableId"></param>
    /// <returns></returns>
    public OperateResult<VariableName[]> S1F11_StatusVariableNamelist(params int[] statusVaruableId)
    {
        var operateResult = secs.ReadSecsMessage(1, 11, new SecsValue(statusVaruableId), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<VariableName[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content.GetItemValues().ToVaruableNames());
    }

    /// <summary>
    /// S1F13的功能方法，测试连接的
    /// </summary>
    /// <returns></returns>
    public OperateResult<OnlineData> S1F13_EstablishCommunications()
    {
        var operateResult = secs.ReadSecsMessage(1, 13, new SecsValue(), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<OnlineData>(operateResult);
        }
        var itemValues = operateResult.Content.GetItemValues();
        var array = itemValues.Value as SecsValue[];
        var array2 = (byte[])array[0].Value;
        return array2[0] == 0 ? OperateResult.CreateSuccessResult((OnlineData)array[1]) : new OperateResult<OnlineData>($"establish communications acknowledgement denied! source: {Environment.NewLine}{itemValues.ToXElement()}");
    }

    /// <summary>
    /// S1F15的功能方法
    /// </summary>
    /// <remarks>
    /// 返回值说明，0: ok, 1: refused, 2: already online
    /// </remarks>
    /// <returns>返回值说明，0: ok, 1: refused, 2: already online</returns>
    public OperateResult<byte> S1F15_OfflineRequest()
    {
        var operateResult = secs.ReadSecsMessage(1, 15, new SecsValue(), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(operateResult);
        }
        return OperateResult.CreateSuccessResult(((byte[])operateResult.Content.GetItemValues().Value)[0]);
    }

    /// <summary>
    /// S1F17的功能方法
    /// </summary>
    /// <remarks>
    /// 返回值说明，0: ok, 1: refused, 2: already online
    /// </remarks>
    /// <returns>返回值说明，0: ok, 1: refused, 2: already online</returns>
    public OperateResult<byte> S1F17_OnlineRequest()
    {
        var operateResult = secs.ReadSecsMessage(1, 17, new SecsValue(), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte>(operateResult);
        }
        return OperateResult.CreateSuccessResult(((byte[])operateResult.Content.GetItemValues().Value)[0]);
    }

    /// <summary>
    /// S2F13的功能方法
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public OperateResult<SecsValue> S2F13_EquipmentConstantRequest(object[] list = null)
    {
        var operateResult = secs.ReadSecsMessage(2, 13, new SecsValue(list), back: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<SecsValue>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content.GetItemValues());
    }
}
