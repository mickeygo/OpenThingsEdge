using ThingsEdge.Exchange.Addresses;
using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.ConsoleApp.Addresses;

internal sealed class ModbusTcpAddressProvider : IAddressProvider
{
    public List<Channel> GetChannels()
    {
        return
        [
            new Channel() {
                Name = "TestChannel01",
                Devices = [
                    new Device {
                        Name = "Device01",
                        Model = DriverModel.ModbusTcp,
                        Host = "127.0.0.1",
                        Tags = [
                            new Tag { Name = "Heartbeat", Address = "s=1;x=3;1", DataType = TagDataType.Int, ScanRate = 500, Flag = TagFlag.Heartbeat },
                            new Tag { Name = "Heartbeat", Address = "s=1;x=3;3", DataType = TagDataType.Int, ScanRate = 1_000, Flag = TagFlag.Notice, PublishMode = PublishMode.OnlyDataChanged },
                            new Tag { Name = "Heartbeat", Address = "s=1;x=3;5", DataType = TagDataType.Int, ScanRate = 5_000, Flag = TagFlag.Notice, PublishMode = PublishMode.EveryScan },
                        ],
                    },
                ],
            },
        ];
    }
}
