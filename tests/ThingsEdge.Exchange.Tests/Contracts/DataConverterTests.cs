namespace ThingsEdge.Exchange.Tests.Contracts;

public class DataConverterTests
{
    /// <summary>
    /// 数据转换测试
    /// </summary>
    [Fact]
    public void Should_Could_Convert_ToDouble_Test()
    {
        double v1 = Convert.ToDouble(true);
        Assert.Equal(1.0, v1);

        double v2 = (byte)16;
        Assert.Equal(16, v2);

        double v3 = (ushort)16;
        Assert.Equal(16, v3);

        double v4 = (uint)16;
        Assert.Equal(16, v4);

        double v5 = (short)16;
        Assert.Equal(16, v5);

        double v6 = (int)16;
        Assert.Equal(16, v6);

        double v7 = (float)16;
        Assert.Equal(16, v7);
    }
}
