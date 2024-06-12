using ThingsEdge.Contracts.Utils;

namespace ThingsEdge.Contracts.Tests;

public class ObjectConvert_Tests
{
    [Fact]
    public void Should_Convert_Test()
    {
        object obj1 = new byte[] { 1, 2, 3 };
        Assert.Equal(new byte[] { 1, 2, 3 }, ObjectConvertUtil.ToByteArray(obj1));
        Assert.Equal(new short[] { 1, 2, 3 }, ObjectConvertUtil.ToInt16Array(obj1));
        Assert.Equal(new int[] { 1, 2, 3 }, ObjectConvertUtil.ToInt32Array(obj1));
    }
}
