﻿namespace ThingsEdge.Server.Services.Impl;

internal sealed class DeviceService : IDeviceService, ITransientDependency
{
    private readonly IDeviceFactory _deviceFactory;

    public DeviceService(IDeviceFactory deviceFactory)
    {
        _deviceFactory = deviceFactory;
    }

    public List<TreeModel> GetDeviceTree()
    {
        var channels = _deviceFactory.GetChannels();
        List<TreeModel> devices = new(channels.Count);

        foreach (var channel in channels)
        {
            TreeModel model1 = new()
            {
                Id = channel.ChannelId,
                Name = channel.Name,
                Category = DeviceTreeCategory.Channel,
            };
            devices.Add(model1);

            foreach (var device in channel.Devices)
            {
                TreeModel model2 = new()
                {
                    Id = device.DeviceId,
                    Name = device.Name,
                    Category = DeviceTreeCategory.Device,
                };

                model1.Children ??= new();
                model1.Children.Add(model2);

                foreach (var tagGroup in device.TagGroups)
                {
                    TreeModel model3 = new()
                    {
                        Id = tagGroup.TagGroupId,
                        Name = tagGroup.Name,
                        Category = DeviceTreeCategory.TagGroup,
                    };

                    model2.Children ??= new();
                    model2.Children.Add(model3);
                }
            }
        }

        return new List<TreeModel> 
        {
            new()
            {
                 Id = "0",
                 Name = "项目",
                 Category = DeviceTreeCategory.Project,
                 Children = devices,
            }
        };
    }

    public List<TagModel>? GetTags(string id, string? category)
    {
        if (category == DeviceTreeCategory.Device)
        {
            var device = _deviceFactory.GetDevice(id);
            return device?.Tags.SelectMany(From).ToList() ?? new(0);
        }

        if (category == DeviceTreeCategory.TagGroup)
        {
            var tagGroup = _deviceFactory.GetDevices().SelectMany(s => s.TagGroups).FirstOrDefault(s => s.TagGroupId == id);
            return tagGroup?.Tags.SelectMany(From).ToList() ?? new(0);
        }

        return default;
    }

    private static List<TagModel> From(Tag tag)
    {
        List<TagModel> models = new(1 + tag.NormalTags.Count)
        {
            FromTag(tag),
        };

        var normalTags = tag.NormalTags.Select(FromTag).ToList();
        models.AddRange(normalTags);

        normalTags.ForEach(s =>
        {
            s.Name = $"- {s.Name}";
        });

        return models;

        static TagModel FromTag(Tag tag)
        {
            TagModel model = new()
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Address = tag.Address,
                Length = tag.Length,
                DataType = tag.DataType,
                Flag = tag.Flag,
                Keynote = tag.Keynote,
                Description = tag.Description,
                ScanRate = tag.ScanRate,
                PublishMode = tag.PublishMode,
                CurveUsage = tag.CurveUsage,
                ClientAccess = tag.ClientAccess,
            };
            return model;
        }
    }
}
