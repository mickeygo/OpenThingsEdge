using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ThingsEdge.Common.Tests.Extensions;

public class EnumExtension_Tests
{
    enum EnumTest
    {
        None,

        [Display(Name = "一")]
        One,
        Two,
        Three,
        Four,
        Five,
    }

    [Fact]
    public void Should_Enum_ToDictionary_Test()
    {
        bool showDisplayNameAttr = true;
        var fileds = typeof(EnumTest).GetFields(BindingFlags.Static | BindingFlags.Public);
        Dictionary<int, string> map = new(fileds.Length);
        foreach (var field in fileds)
        {
            string v = field.Name;
            if (showDisplayNameAttr)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (!string.IsNullOrEmpty(attr?.Name))
                {
                    v = attr.Name;
                }
            }

            int k = (int)field.GetRawConstantValue()!;
            map[k] = v;
        }

        Assert.Equal("None", map[0]);
        Assert.Equal("一", map[1]);
    }
}
