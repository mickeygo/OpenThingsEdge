using System.Text.RegularExpressions;

namespace ThingsEdge.Common.Tests.Commons;

public partial class Regex_Tests
{
    [Fact]
    public void Should_Regular_Replace_01_Test()
    {
        var ret1 = MyRegex().Replace("{ChannelName}/{DeviceName}/{TagGroupName}", MatchTest);
        Assert.Equal("line01/device01/op010", ret1);

        var ret2 = MyRegex().Replace("{ChannelName}/{DeviceName}/", MatchTest);
        Assert.Equal("line01/device01/", ret2);

        var ret3 = MyRegex().Replace("{ChannelName}//{TagGroupName}", MatchTest);
        Assert.Equal("line01//op010", ret3);

        var ret4 = MyRegex().Replace("", MatchTest);
        Assert.Equal("", ret4);

        var ret5 = MyRegex().Replace("{ChannelName}/abc/{TagGroupName}", MatchTest);
        Assert.Equal("line01/abc/op010", ret5);
    }

    private string MatchTest(Match match) => match.Value switch
    {
        "{ChannelName}" => "line01",
        "{DeviceName}" => "device01",
        "{TagGroupName}" => "op010",
        _ => "",
    };

    [GeneratedRegex("{ChannelName}|{DeviceName}|{TagGroupName}", RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex();
}
