namespace ThingsEdge.Router.Transport.MQTT;

public sealed partial class MQTTClientTopicFormater
{
    public static string Default(Schema schema, string? topicFormater, bool topicFormatMatchLower)
    {
        topicFormater ??= "{ChannelName}/{DeviceName}/{TagGroupName}";
        var match = TopicRegex().Replace(topicFormater, match =>
        {
            return match.Value switch
            {
                "{ChannelName}" => MatchToLower(schema.ChannelName),
                "{DeviceName}" => MatchToLower(schema.DeviceName),
                "{TagGroupName}" => MatchToLower(schema.TagGroupName ?? ""),
                _ => "",
            };
        });

        // 移除首尾斜杠
        var topic = match.Trim('/');

        return topic;

        string MatchToLower(string str)
        {
            if (topicFormatMatchLower)
            {
                return str.ToLower();
            }

            return str;
        }
    }

    [GeneratedRegex("{ChannelName}|{DeviceName}|{TagGroupName}", RegexOptions.IgnoreCase)]
    private static partial Regex TopicRegex();
}
