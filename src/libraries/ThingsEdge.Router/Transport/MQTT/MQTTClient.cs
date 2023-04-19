using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Router.Transport.MQTT;

internal sealed class MQTTClient : IMQTTClient
{
    private readonly ManagedMqttClientOptions _managedMqttClientOptions;

    public IManagedMqttClient ManagedMqttClient { get; }

    public MQTTClient(IManagedMqttClient managedMqttClient, ManagedMqttClientOptions managedMqttClientOptions)
    {
        ManagedMqttClient = managedMqttClient;
        _managedMqttClientOptions = managedMqttClientOptions;
    }

    public async Task StartAsync()
    {
        if (!ManagedMqttClient.IsStarted)
        {
            await ManagedMqttClient.StartAsync(_managedMqttClientOptions).ConfigureAwait(false);
        }
    }

    public async Task StopAsync(bool cleanDisconnect = true)
    {
        await ManagedMqttClient.StopAsync(cleanDisconnect).ConfigureAwait(false);
    }

    public void Dispose()
    {
        ManagedMqttClient.Dispose();
    }
}
