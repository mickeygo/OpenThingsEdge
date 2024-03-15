using System.Net.NetworkInformation;
using ThingsEdge.Common.Utils;

namespace ThingsEdge.Common.Tests.Utils;

public class ObjectEqualUtil_Tests
{
    [Fact]
    public void Should_Object_Equal_Test()
    {
        bool b1 = true;
        bool b2 = true;
        bool b3 = false;
        Assert.True(ObjectEqualUtil.IsEqual(b1, b2));
        Assert.False(ObjectEqualUtil.IsEqual(b1, b3));

        byte be1 = 3;
        byte be2 = 3;
        byte be3 = 4;
        Assert.True(ObjectEqualUtil.IsEqual(be1, be2));
        Assert.False(ObjectEqualUtil.IsEqual(be1, be3));

        sbyte sbe1 = 3;
        Assert.True(ObjectEqualUtil.IsEqual(sbe1, be1));

        int i1 = 1;
        int i2 = 1;
        int i3 = 2;
        int? i4 = 2;
        Assert.True(ObjectEqualUtil.IsEqual(i1, i2));
        Assert.False(ObjectEqualUtil.IsEqual(i1, i3));
        Assert.True(ObjectEqualUtil.IsEqual(i3, i4));

        uint ui1 = 1;
        uint ui2 = 2;
        Assert.True(ObjectEqualUtil.IsEqual(ui1, i1));
        Assert.False(ObjectEqualUtil.IsEqual(ui2, i1));

        long l1 = 1;
        long l2 = 1;
        long l3 = 3;
        Assert.True(ObjectEqualUtil.IsEqual(l1, l2));
        Assert.False(ObjectEqualUtil.IsEqual(l1, l3));

        uint ul1 = 1;
        uint ul2 = 2;
        Assert.True(ObjectEqualUtil.IsEqual(ul1, l1));
        Assert.False(ObjectEqualUtil.IsEqual(ul2, l1));

        string s1 = "s1";
        string s2 = "s1";
        string s3 = "s3";
        Assert.True(ObjectEqualUtil.IsEqual(s1, s2));
        Assert.False(ObjectEqualUtil.IsEqual(s1, s3));

        bool[] bArr1 = [true, true, false];
        bool[] bArr2 = [true, true, false];
        bool[] bArr3 = [true, false, false];
        Assert.True(ObjectEqualUtil.IsEqual(bArr1, bArr2));
        Assert.False(ObjectEqualUtil.IsEqual(bArr1, bArr3));

        int[] iArr1 = [1, 2, 3];
        int[] iArr2 = [1, 2, 3];
        int[] iArr3 = [1, 2, 2];
        int[]? iArr4 = [1, 2, 2];
        Assert.True(ObjectEqualUtil.IsEqual(iArr1, iArr2));
        Assert.False(ObjectEqualUtil.IsEqual(iArr1, iArr3));
        Assert.False(ObjectEqualUtil.IsEqual(iArr1, bArr3));
        Assert.True(ObjectEqualUtil.IsEqual(iArr3, iArr4));
    }
}
