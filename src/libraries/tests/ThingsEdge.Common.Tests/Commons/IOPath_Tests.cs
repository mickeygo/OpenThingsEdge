namespace ThingsEdge.Common.Tests.Commons;

public class IOPath_Tests
{
    [Fact]
    public void Should_Get_Directory_Name_Test()
    {
        string path = @"E:\FolderA\FolderB\FolderC\log.txt";

        var dirName1 = Path.GetDirectoryName(path);
        Assert.Equal(@"E:\FolderA\FolderB\FolderC", dirName1);

        var path2 = Path.Combine("FolderA", "FolderB");
        Assert.Equal(@"FolderA\FolderB", path2); // 不含分隔符后缀

        var path3 = Path.GetFullPath(@"\");
        Assert.Equal(@"E:\", path3); // 如：E:\ 不含分隔符后缀

        var path4 = Path.GetFullPath(path);
        Assert.Equal(path, path4);
    }

    [Fact]
    public void Should_FilePath_Equal_Test()
    {
        string path1 = @"E:\FolderA\FolderB\FolderC\log.txt";
        string path2 = @"E:\FolderA\FolderB";

        var path3 = path1[(path2.Length + 1)..];
        Assert.Equal(@"FolderC\log.txt", path3);

        var path4 = Path.Combine("E:\\Folder1", path3);
        Assert.Equal(@"E:\Folder1\FolderC\log.txt", path4);
    }
}
