using System.Globalization;
using ThingsEdge.Communication.Languages;

namespace ThingsEdge.Communication.Common;

/// <summary>
/// 系统的字符串资源及多语言管理中心。
/// </summary>
public static class StringResources
{
    /// <summary>
    /// 获取或设置系统的语言选项。
    /// </summary>
    [NotNull]
    public static DefaultLanguage? Language { get; private set; }

    static StringResources()
    {
        if (CultureInfo.CurrentCulture.ToString().StartsWith("zh"))
        {
            SetLanguageChinese();
        }
        else
        {
            SeteLanguageEnglish();
        }
    }

    /// <summary>
    /// 将语言设置为中文。
    /// </summary>
    public static void SetLanguageChinese()
    {
        Language = new DefaultLanguage();
    }

    /// <summary>
    /// 将语言设置为英文。
    /// </summary>
    public static void SeteLanguageEnglish()
    {
        Language = new English();
    }
}
