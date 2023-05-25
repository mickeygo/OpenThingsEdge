using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ThingsEdge.Contracts.Variables;

namespace ThingsEdge.Contracts.Tests;

public class Json_Tests
{
    [Fact]
    public void Should_Json_Serialize_ToChannel_Test()
    {
        var str = JsonSerializer.Serialize(Channels, new JsonSerializerOptions 
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),
            WriteIndented = true,
        });
        Assert.NotNull(str);
    }

    [Fact]
    public void Should_Json_Deserialize_ToChannel_Test()
    {
        var str = @"
[
  {
    ""Name"": ""测试Channel"",
    ""Model"": ""Siemens"",
    ""Keynote"": """",
    ""Devices"": [
      {
        ""Name"": ""Name"",
        ""Model"": ""S7_1500"",
        ""Host"": """",
        ""Port"": 102,
        ""Keynote"": """",
        ""TagGroups"": [
          {
            ""Name"": """",
            ""Keynote"": """",
            ""Tags"": []
          }
        ],
        ""Tags"": [
          {
            ""Name"": ""Name"",
            ""Address"": """",
            ""Length"": 0,
            ""DataType"": ""String"",
            ""ClientAccess"": ""ReadAndWrite"",
            ""ScanRate"": 100,
            ""Flag"": ""Trigger"",
            ""Keynote"": """",
            ""Description"": """",
            ""NormalTags"": []
          }
        ]
      }
    ]
  }
]";

        var channels = JsonSerializer.Deserialize<List<Channel>>(str);
        Assert.NotNull(channels);
        Assert.NotEmpty(channels);
    }

    static List<Channel> Channels => new()
        {
            new Channel
            {
                 //ChannelId = "",
                 Name = "Name",
                 Model = ChannelModel.Siemens,
                 Keynote = "测试Channel",
                 Devices = new()
                 {
                     new Device
                     {
                          //DeviceId = "",
                          Name = "Name",
                          Host = "",
                          Port = 102,
                          Model = DriverModel.S7_1500,
                          Keynote = "",
                          TagGroups = new()
                          {
                              new()
                              {
                                  //TagGroupId = "",
                                  Name = "",
                                  Keynote = "",
                                  Tags = new()
                                  {

                                  },
                              },
                          },
                          Tags = new()
                          {
                             new()
                             {
                                 //TagId = "",
                                 Name = "Name",
                                 Address = "",
                                 Length = 0,
                                 DataType = TagDataType.String,
                                 ScanRate = 100,
                                 Flag = TagFlag.Trigger,
                                 Keynote = "",
                                 Description = "",
                                 NormalTags = new()
                                 {

                                 },
                             },
                          },
                     },
                 },
            },
        };
}
