using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Core;

/// <summary>
/// 所有数据转换类的静态辅助方法。
/// </summary>
public static class ByteTransformHelper
{
    /// <summary>
    /// 结果转换操作的基础方法，需要支持类型，及转换的委托，并捕获转换时的异常方法
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="result">源</param>
    /// <param name="translator">实际转换的委托</param>
    /// <returns>转换结果</returns>
    public static OperateResult<TResult> GetResultFromBytes<TResult>(OperateResult<byte[]> result, Func<byte[], TResult> translator)
    {
        try
        {
            if (result.IsSuccess)
            {
                return OperateResult.CreateSuccessResult(translator(result.Content));
            }
            return OperateResult.CreateFailedResult<TResult>(result);
        }
        catch (Exception ex)
        {
            var operateResult = new OperateResult<TResult>
            {
                Message = $"{StringResources.Language.DataTransformError} {result.Content.ToHexString()} : Length({result.Content?.Length}) {ex.Message}"
            };
            return operateResult;
        }
    }

    /// <summary>
    /// 结果转换操作的基础方法，需要支持类型，及转换的委托
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <param name="result">源结果</param>
    /// <returns>转换结果</returns>
    public static OperateResult<TResult> GetResultFromArray<TResult>(OperateResult<TResult[]> result)
    {
        return GetSuccessResultFromOther(result, (m) => m[0]);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容，所转换的规则
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetSuccessResultFromOther<TResult, TIn>(OperateResult<TIn> result, Func<TIn, TResult> trans)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        try
        {
            var value = trans(result.Content);
            return OperateResult.CreateSuccessResult(value);
        }
        catch (Exception ex)
        {
            return new OperateResult<TResult>(StringResources.Language.DataTransformError + " " + ex.Message);
        }
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult GetResultFromOther<TIn>(OperateResult<TIn> result, Func<TIn, OperateResult> trans)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        return trans(result.Content);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans">转换方法，从类型TIn转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn>(OperateResult<TIn> result, Func<TIn, OperateResult<TResult>> trans)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        return trans(result.Content);
    }

    /// <summary>
    /// 使用指定的转换方法，来获取到实际的结果对象内容
    /// </summary>
    /// <typeparam name="TResult">结果类型</typeparam>
    /// <typeparam name="TIn1">输入类型1</typeparam>
    /// <typeparam name="TIn2">输入类型2</typeparam>
    /// <param name="result">原始的结果对象</param>
    /// <param name="trans1">转换方法1，从类型TIn1转换拿到OperateResult的TIn2的泛型委托</param>
    /// <param name="trans2">转换方法2，从类型TIn2转换拿到OperateResult的TResult的泛型委托</param>
    /// <returns>类型为TResult的对象</returns>
    public static OperateResult<TResult> GetResultFromOther<TResult, TIn1, TIn2>(OperateResult<TIn1> result, Func<TIn1, OperateResult<TIn2>> trans1, Func<TIn2, OperateResult<TResult>> trans2)
    {
        if (!result.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(result);
        }

        var operateResult = trans1(result.Content);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<TResult>(operateResult);
        }
        return trans2(operateResult.Content);
    }
}
