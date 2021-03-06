using KHLBotSharp.Models.EventsMessage;
using KHLBotSharp.Models.MessageHttps.EventMessage.Abstract;
using System;
using System.Threading.Tasks;

namespace KHLBotSharp
{
    /// <summary>
    /// 插件interface
    /// </summary>
    /// <typeparam name="T">事件class</typeparam>
    public interface IKHLPlugin<T> : IKHLPlugin where T : Extra
    {
        /// <summary>
        /// <typeparamref name="T"/>事件处理，插件用
        /// </summary>
        /// <returns>true 为事件处理完毕，不需要接下去尝试别的Plugin class</returns>
        Task<bool> Handle(EventMessage<T> eventArgs);
    }
    /// <summary>
    /// 插件interface
    /// </summary>
    public interface IKHLPlugin
    {
        /// <summary>
        /// 用于获取已注册的DI Services
        /// </summary>
        /// <returns></returns>
        Task Ctor(IServiceProvider provider);
        /// <summary>
        /// 指令名字，可选是否需要
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 其他指令，用于运行相同的功能
        /// </summary>
        string[] Prefix { get; }
        /// <summary>
        /// 指令帮助，用于生成help功能
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 指令分类，用于生成help功能
        /// </summary>
        string Group { get; }
    }
    /// <summary>
    /// 注册插件为特定DI方式，当前支持Singleton和Transient。<br/>
    /// 如果注册在非插件的class则不会有任何反应，想要注册插件以外的请使用<seealso cref="IServiceRegister"/>。<br/>
    /// 如果无加上这个interface则默认自动注册为Scoped
    /// </summary>
    public interface IPluginType : IDisposable
    {
        RegisterType RegisterType { get; }
    }
    /// <summary>
    /// 插件注册方式
    /// </summary>
    public enum RegisterType
    {
        Singleton,
        Transient
    }
}
