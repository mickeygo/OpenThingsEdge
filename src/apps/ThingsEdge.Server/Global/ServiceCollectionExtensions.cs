using ThingsEdge.Server.Services;
using ThingsEdge.Server.Services.Impl;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGlobalForServer(this IServiceCollection services)
        {
            services.AddNav(Path.Combine(AppContext.BaseDirectory, $"wwwroot/nav/nav.json"));
            services.AddScoped<GlobalConfig>();

            services.AddTransient<IDeviceService, DeviceService>();

            return services;
        }
    }
}
