using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThingsEdge.Contracts.Tests;

public class JsonElement_Tests
{
    [Fact]
    public void Should_Fetch_Value_From_JsonElement_Test()
    {
        string jsonString = """
        {
            "k1": "k",
            "k2": true,
            "k3": 16,
            "k4": 1024,
            "k5": 12.56,
            "k6": [ 1, 2, 3],
            "k7": [ "a", "b", "c" ],
            "k8": { 
                "n1": "n", 
                "n2": 16,
                "n3": 12.56
            },
            "k9": [
                { 
                  "n1": "n1", 
                  "n2": 16,
                  "n3": 12.56
                },
                { 
                  "n1": "n2", 
                  "n2": 16,
                  "n3": 12.56
                }
            ]
        }
""";

        JsonModel model = JsonSerializer.Deserialize<JsonModel>(jsonString)!;

        Assert.Equal("k", model.Props!["k1"].GetString());
        Assert.True(model.Props!["k2"].GetBoolean());

        // Number 类型可转换为兼容类型。
        Assert.Equal(16, model.Props!["k3"].GetByte());
        Assert.Equal(16, model.Props!["k3"].GetInt16());
        Assert.Equal(16, model.Props!["k3"].GetInt32());
        Assert.Equal("16", model.Props!["k3"].GetRawText());
        //Assert.Equal("16", model.Props!["k3"].GetString()); // 错误, 元素类型（'Number'）必须为 'String'
        Assert.Equal(1024, model.Props!["k4"].GetInt32());
        Assert.Equal(12.56, model.Props!["k5"].GetDouble());
        Assert.Equal((decimal)12.56, model.Props!["k5"].GetDecimal());
        Assert.Equal(3, model.Props!["k6"].GetArrayLength());

        var arr1 = model.Props!["k6"].EnumerateArray().Select(e => e.GetInt32()).ToArray();
        Assert.Equal(stackalloc[] { 1, 2, 3 }, arr1);

        var arr2 = model.Props!["k7"].EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(new string[] { "a", "b", "c" }, arr2);

        var item2 = model.Props!["k8"].Deserialize<JsonModelItem>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Equal(new JsonModelItem { N1 = "n", N2 = 16, N3 = 12.56 }, item2);
    }

    public sealed class JsonModel
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Props { get; set; }
    }

    public record JsonModelItem()
    {
        public string? N1 { get; set; }

        public int N2 { get; set; }

        public double N3 { get; set; }
    }
}
