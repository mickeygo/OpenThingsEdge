using Proto;
using Proto.DependencyInjection;

namespace ThingsEdge.Exchange.Infrastructure.Actors;

/// <summary>
/// <see cref="IContext"/> 扩展类。
/// </summary>
public static class ActorContextExtension
{
    extension(IContext context)
    {
        /// <summary>
        /// 创建一个新的子 Actor。
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="args">Actor 构造参数</param>
        /// <returns></returns>
        /// <remarks>
        /// 前提：Context 隶属于的 ActorSystem 在创建时需要附加 IServiceProvider。
        /// </remarks>
        public PID SpawnFor<TActor>(IEnumerable<object>? args = null)
            where TActor : IActor
        {
            var props = args != null
               ? context.System.DI().PropsFor<TActor>(args)
               : context.System.DI().PropsFor<TActor>();
            return context.Spawn(props);
        }

        /// <summary>
        /// 创建一个新的子 Actor。
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="propsHandler">Props 处理方法</param>
        /// <param name="args">Actor 构造参数</param>
        /// <returns></returns>
        /// <remarks>
        /// 前提：Context 隶属于的 ActorSystem 在创建时需要附加 IServiceProvider。
        /// </remarks>
        public PID SpawnFor<TActor>(Action<Props> propsHandler, IEnumerable<object>? args = null)
            where TActor : IActor
        {
            var props = args != null
               ? context.System.DI().PropsFor<TActor>(args)
               : context.System.DI().PropsFor<TActor>();
            propsHandler.Invoke(props);
            return context.Spawn(props);
        }

        /// <summary>
        /// 创建一个新的带有指定名称的子 Actor。
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="name">Actor 唯一名称</param>
        /// <param name="propsHandler">Props 处理方法</param>
        /// <param name="args">Actor 构造参数</param>
        /// <returns></returns>
        /// <remarks>
        /// 前提：Context 隶属于的 ActorSystem 在创建时需要附加 IServiceProvider。
        /// </remarks>
        public PID SpawnFor<TActor>(string name, Action<Props>? propsHandler = null, IEnumerable<object>? args = null)
            where TActor : IActor
        {
            var props = args != null
                ? context.System.DI().PropsFor<TActor>(args)
                : context.System.DI().PropsFor<TActor>();
            propsHandler?.Invoke(props);
            return context.SpawnNamed(props, name);
        }

        /// <summary>
        /// 向当前 Actor 的所有子 Actor 中发送指定消息。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        public void SendToChildren(object message)
        {
            foreach (var pid in context.Children)
            {
                context.Send(pid, message);
            }
        }
    }
}
