using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Tests.Contracts;

public class PayloadDataExtensionsTest
{
    [Fact]
    public void Should_Could_PayloadData_TryGetAsDouble_Test()
    {
        var payload = new PayloadData()
        {
            TagName = "PLC_Tightening_Angle",
            DataType = TagDataType.Real,
            Length = 0,
            Value = 28.43f,
        };

        var ok = payload.TryGetAsDouble(out var v1);
        Assert.True(ok);
        Assert.True(Math.Abs(v1!.Value - 28.43) < 0.001);
    }
}
