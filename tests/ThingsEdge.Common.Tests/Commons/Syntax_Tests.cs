namespace ThingsEdge.Common.Tests.Commons;

public class Syntax_Tests
{
    [Fact]
    public void Should_Nullable_Could_Compare_Test()
    {
        int? a1 = null;
        int a2 = 3;

        // 空与任何值比较都是 false
        Assert.False(a1 < a2);
        Assert.False(a1 == a2);
        Assert.False(a1 > a2);

        a1 = 1;
        Assert.True(a1 < a2);
    }
}
