using Proto;
using Proto.DependencyInjection;

namespace ThingsEdge.Exchange.Infrastructure.Actors;

internal static class ActorExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册 Proto Actor 服务。
        /// </summary>
        /// <returns></returns>
        public IServiceCollection AddActorSetup()
        {
            services.AddSingleton(sp => new ActorSystem().WithServiceProvider(sp));

            return services;
        }
    }
}
