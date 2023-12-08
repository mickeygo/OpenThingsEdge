using ThingsEdge.Common.Utils;

namespace ThingsEdge.Common.Tests.Commons;

public class FolderUtil_Tests
{
    [Fact]
    public void Should_GetFolderInfo_Test()
    {
        string dirPath = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.FullName; // ThingsEdge.Common.Tests\bin\Debug\net7.0
        var folderInfo = FolderUtil.GetFolderTreeInfo(dirPath);
        Assert.NotNull(folderInfo);
    }
}
