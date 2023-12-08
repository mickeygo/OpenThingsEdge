namespace ThingsEdge.Common.Tests.Commons;

public class File_Tests
{
    [Fact]
    public void Should_Get_Folder_Size_Test()
    {
        string dirPath = AppContext.BaseDirectory; // xx\OpenThingsEdge\src\libraries\tests\ThingsEdge.Common.Tests\bin\Debug\net7.0\
        DirectoryInfo di = new(dirPath);
        var size = di.GetFiles("*", SearchOption.AllDirectories).Sum(s => s.Length);

        Assert.True(size == 0, size.ToString());
    }

    /// <summary>
    /// 验证文件夹中有新增文件或文件夹时，会更改 LastWriteTime 时间。
    /// </summary>
    [Fact]
    public void Should_Folder_LastWriteTime_Changed_Test()
    {
        string dirPath = @"E:\Test\Test1";

        // 文件夹中有新增文件或文件夹时，会更改 LastWriteTime 时间；更新文件内容时，不会更新 LastWriteTime 时间；
        // 但，不会更新父文件的任何信息。
        DirectoryInfo di = new(dirPath);
        di.CreateSubdirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
        var lastWriteTime = di.LastWriteTime;

        Assert.True(lastWriteTime < DateTime.Now);
    }
}
