﻿using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

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

    public async Task SubcribeAsync(string[] topics, MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce)
    {
        var topicFilters = topics.Select(s => new MqttTopicFilterBuilder().WithTopic(s).WithQualityOfServiceLevel(qualityOfServiceLevel).Build()).ToList();
        await ManagedMqttClient.SubscribeAsync(topicFilters);
    }

    public async Task StartAsync()
    {
        await ManagedMqttClient.StartAsync(_managedMqttClientOptions);
    }

    public async Task StopAsync(bool cleanDisconnect = true)
    {
        await ManagedMqttClient.StopAsync(cleanDisconnect);
    }

    public void Dispose()
    {
        ManagedMqttClient.Dispose();
    }
}