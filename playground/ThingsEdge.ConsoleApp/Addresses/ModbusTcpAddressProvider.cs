using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Utils;

namespace ThingsEdge.ConsoleApp.Addresses;

internal sealed class ModbusTcpAddressProvider : IAddressProvider
{
    public List<Channel> GetChannels()
    {
        var json = """
[
    {
        "Name": "TestChannel01",
        "Devices": [
            {
                "Name": "Device01",
                "Model": "ModbusTcp",
                "Host": "127.0.0.1",
                "Tags": [
                    { "Name": "Heartbeat", "Address": "s=1;x=3;1", "DataType": "Int", "ScanRate": 500, "Flag": "Heartbeat" },
                    { "Name": "Notice01", "Address": "s=1;x=3;3", "DataType": "Int", "ScanRate": 1000, "Flag": "Notice", "PublishMode": "OnlyDataChanged" },
                    { "Name": "Notice02", "Address": "s=1;x=3;5", "DataType": "Int", "ScanRate": 10000, "Flag": "Notice", "PublishMode": "EveryScan" },
                    { "Name": "PLC_Archive_Sign", "Address": "s=1;x=3;10", "DataType": "Int", "ScanRate": 500, "Flag": "Trigger",
                        "NormalTags": [
                            { "Name": "Trigger01_1", "Address": "s=1;x=3;11", "DataType": "Int" },
                            { "Name": "Trigger01_2", "Address": "s=1;x=3;12", "DataType": "Int" },
                            { "Name": "Trigger01_3", "Address": "s=1;x=3;13", "DataType": "Int" },
                        ],
                    },
                ],
            },
        ],
    },
]
""";

        return JsonUtils.Deserialize<List<Channel>>(json) ?? [];
    }
}
