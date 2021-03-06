using KHLBotSharp.Common.Request;
using KHLBotSharp.Core.Models.Config;
using KHLBotSharp.IService;
using KHLBotSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace KHLBotSharp.Core.BotHost
{
    /// <summary>
    /// WebHook DI帮助
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebhookHelper
    {
        /// <summary>
        /// WebHook DI 注册，内部使用一般插件无需知道
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterKHLBot(this IServiceCollection service)
        {
            if (!Directory.Exists("Profiles"))
            {
                Directory.CreateDirectory("Profiles");
            }
            var publicLog = new LogService();
            publicLog.Init("Public", new BotConfigSettings { Debug = false });
            service.AddSingleton<ILogService>(publicLog);
            var botInstances = Directory.GetDirectories("Profiles");
            //1 is Public, we can't count that as valid bot!
            if (botInstances.Length < 2)
            {
                if (!Directory.Exists("Profiles\\Bot\\Plugins"))
                {
                    Directory.CreateDirectory("Profiles\\Bot\\Plugins");
                }
                if (!Directory.Exists("Profiles\\Bot\\Log"))
                {
                    Directory.CreateDirectory("Profiles\\Bot\\Log");
                }
                botInstances = Directory.GetDirectories("Profiles");
            }
            WebhookInstanceManagerService hookManager = new WebhookInstanceManagerService();
            foreach (var bot in botInstances)
            {
                if (bot.EndsWith("Public"))
                {
                    continue;
                }
                if (!File.Exists(bot + "\\config.json"))
                {
                    var config = new BotConfigSettings()
                    {
                        EncryptKey = "",
                        VerifyToken = ""
                    };
                    File.WriteAllText(bot + "\\config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
                }
                var settings = new BotConfigSettings();
                settings.Load(bot);
                if (!settings.Active)
                {
                    //Skip load
                    continue;
                }
                var hookInstance = new WebHookInstance
                {
                    Name = bot.Split('\\').Last()
                };
                var pluginLoader = new PluginLoaderService();
                hookInstance.ServiceCollection = new ServiceCollection();
                pluginLoader.LoadPlugin(bot, hookInstance.ServiceCollection);
                hookInstance.ServiceCollection.AddSingleton(typeof(IPluginLoaderService), pluginLoader);
                hookInstance.ServiceCollection.AddScoped(typeof(IKHLHttpService), typeof(KHLHttpService));
                if (string.IsNullOrEmpty(settings.EncryptKey)&&!Console.IsInputRedirected && Console.KeyAvailable)
                {
                    settings.EncryptKey = AnsiConsole.Ask<string>("Input EncryptKey");
                    File.WriteAllText(bot + "\\config.json", JsonConvert.SerializeObject(settings));
                }
                if (string.IsNullOrEmpty(settings.VerifyToken) && !Console.IsInputRedirected && Console.KeyAvailable)
                {
                    settings.VerifyToken = AnsiConsole.Ask<string>("Input VerifyToken");
                    File.WriteAllText(bot + "\\config.json", JsonConvert.SerializeObject(settings));
                }
                hookInstance.ServiceCollection.AddSingleton(typeof(IBotConfigSettings), settings);
                var logService = new LogService();
                logService.Init(bot.Split('\\').Last(), settings);
                hookInstance.ServiceCollection.AddSingleton(typeof(ILogService), logService);
                hookInstance.ServiceCollection.AddSingleton(typeof(IErrorRateService), typeof(ErrorRateService));
                hookInstance.ServiceCollection.AddHttpClient<IHttpClientService, HttpClientService>();
                hookInstance.ServiceCollection.AddScoped<IDecoderService, DecoderService>();
                hookInstance.ServiceCollection.AddMemoryCache();
                hookManager.Add(hookInstance);
            }
            service.AddSingleton<IWebhookInstanceManagerService>(hookManager);
            return service;
        }
    }
}
