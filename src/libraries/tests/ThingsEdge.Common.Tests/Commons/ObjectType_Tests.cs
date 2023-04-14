namespace ThingsEdge.Common.Tests.Commons;

public class ObjectType_Tests
{
    [Fact]
    public void Should_ObjectType_Test()
    {
        object obj1 = new int[] { 1, 2, 3 };
        var ret1 = To<int[]>(obj1);
        Assert.Null(ret1);

        object obj2 = new List<int>();
        var ret2 = To<List<int>>(obj2);
        Assert.Null(ret2);
    }

    private static T To<T>(object obj) 
    {
        var typ = typeof(T);

        if (typ.IsArray)
        {
            return (T)obj;
        }

        
        if (typ.IsGenericType)
        {
            
        }

        return (T)obj;
    }
}
