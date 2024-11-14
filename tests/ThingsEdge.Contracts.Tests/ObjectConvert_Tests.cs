using ThingsEdge.Contracts.Utils;

namespace ThingsEdge.Contracts.Tests;

public class ObjectConvert_Tests
{
    [Fact]
    public void Should_ObjectConvert_Test()
    {
        object obj1 = new byte[] { 1, 2, 3 };
        Assert.Equal(new byte[] { 1, 2, 3 }, ObjectConvertUtil.ToByteArray(obj1));
        Assert.Equal(new short[] { 1, 2, 3 }, ObjectConvertUtil.ToInt16Array(obj1));
        Assert.Equal(new int[] { 1, 2, 3 }, ObjectConvertUtil.ToInt32Array(obj1));
        Assert.Equal([1, 2, 3], ObjectConvertUtil.ToDoubleArray(obj1));


        object obj2 = new bool[] { true, false };
        Assert.Equal(new int[] { 1, 0 }, ObjectConvertUtil.ToInt32Array(obj2));
        Assert.Equal([1, 0], ObjectConvertUtil.ToDoubleArray(obj2));
    }
}
