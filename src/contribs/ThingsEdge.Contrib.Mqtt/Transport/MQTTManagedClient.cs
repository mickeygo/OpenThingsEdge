using MQTTnet.Extensions.ManagedClient;

namespace ThingsEdge.Contrib.Mqtt.Transport;

internal sealed class MQTTManagedClient : IMQTTManagedClient
{
    private readonly ManagedMqttClientOptions _managedMqttClientOptions;

    public IManagedMqttClient ManagedMqttClient { get; }

    public MQTTManagedClient(IManagedMqttClient managedMqttClient, ManagedMqttClientOptions managedMqttClientOptions)
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
