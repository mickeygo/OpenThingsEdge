using MQTTnet.Client;

namespace ThingsEdge.Contrib.Mqtt.Transport;

internal sealed class MQTTClient : IMQTTClient
{
    private readonly MqttClientOptions _mqttClientOptions;

    public IMqttClient MqttClient { get; }

    public MQTTClient(IMqttClient mqttClient, MqttClientOptions mqttClientOptions)
    {
        MqttClient = mqttClient;
        _mqttClientOptions = mqttClientOptions;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (!MqttClient.IsConnected)
        {
            await MqttClient.ConnectAsync(_mqttClientOptions, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await MqttClient.DisconnectAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        MqttClient.Dispose();
    }
}
