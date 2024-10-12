using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CFSyncFolders.Forms;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Services;
using CFUtilities.Interfaces;
using CFUtilities.Logging;
using CFUtilities.Services;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CFSyncFolders
{
    static class Program
    {        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {           
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }
     
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Create a host builder to build the service provider
        /// </summary>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder()
        {            
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddTransient<IPlaceholderService, PlaceholderService>();
                    services.AddTransient<ILogger>((scope) =>
                    {
                        var placeholderService = scope.GetRequiredService<IPlaceholderService>();                        

                        // Get log file
                        var logsFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("LogsFolder").ToString();                        
                        logsFolder = placeholderService.GetWithPlaceholdersReplaced(logsFolder, new Dictionary<string, object>());                    
                        return new CSVLogger((Char)9, Encoding.UTF8, Path.Combine(logsFolder, "{date:MM-yyyy}"), placeholderService);
                    });
                    services.AddTransient<ISyncConfigurationService>((scope) =>
                    {
                        var placeholderService = scope.GetRequiredService<IPlaceholderService>();

                        // Get data folder
                        var dataFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("DataFolder").ToString();                                           
                        dataFolder = placeholderService.GetWithPlaceholdersReplaced(dataFolder, new Dictionary<string, object>());                    
                        return new SyncConfigurationService(dataFolder);
                    });

                    //// Event handling:
                    //// - We load custom types from this assembly and generic types from CFEventHandler.                    
                    //// - GetEventHandlerRules returns the links between the event type & IEventHandler instance.
                    //// - Settings service must be registered even if we don't use the IEventHandler (TODO: Fix this)
                    //services.RegisterAllTypes<IEventHandler>(new[] { Assembly.GetExecutingAssembly(), typeof(IEventManagerService).Assembly });   // Custom & generic implementations
                    //services.RegisterAllTypes<IEmailCreator>(new[] { Assembly.GetExecutingAssembly(), typeof(IEventManagerService).Assembly });   // Custom & generic implementations

                    //services.AddSingleton<IConsoleSettingsService, ConsoleSettingsService>();
                    //services.AddSingleton<ICSVSettingsService, CSVSettingsService>();
                    //services.AddSingleton<IEmailSettingsService>((scope) =>
                    //{
                    //    return new EmailSettingsService(System.Configuration.ConfigurationSettings.AppSettings.Get("Email.Server").ToString(),
                    //                        Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings.Get("Email.Port").ToString()),
                    //                        System.Configuration.ConfigurationSettings.AppSettings.Get("Email.Username").ToString(),
                    //                        System.Configuration.ConfigurationSettings.AppSettings.Get("Email.Password").ToString(),
                    //                        new Dictionary<string, string>()
                    //                        {
                    //                            { "Test", new GenericEmailCreator().Id },
                    //                            { "SyncComplete", new SyncCompleteEmailCreator().Id }
                    //                        });                        
                    //});
                    //services.AddSingleton<IHTTPSettingsService, HTTPSettingsService>();
                    //services.AddSingleton<ISQLSettingsService, SQLSettingsService>();
                    //services.AddSingleton<ITeamsSettingsService, TeamsSettingsService>();

                    //services.AddSingleton<IEventManagerService>((scope) =>
                    //{
                    //    return new EventManagerService(scope.GetServices<IEventHandler>().ToList(),
                    //                                    GetEventHandlerRules());                        
                    //});

                    services.AddTransient<MainForm>();
                });
        }

        /// <summary>
        /// Registers all types implementing interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="lifetime"></param>
        private static void RegisterAllTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }

        ///// <summary>
        ///// Returns event handler rules.       
        ///// </summary>
        ///// <returns></returns>
        ///// <remarks>Please ensure that DI defines all dependencies. E.g. Settings service</remarks>
        //private static List<EventHandlerRule> GetEventHandlerRules()
        //{
        //    var rules = new List<EventHandlerRule>();

        //    // Set rule for handling Test
        //    rules.Add(new EventHandlerRule()
        //    {
        //        EventTypeId = "Test",
        //        EventHandlerIds = new List<string>() { typeof(EmailEventHandler).Name }
        //    });

        //    // Set rule for handling SyncComplete
        //    rules.Add(new EventHandlerRule()
        //    {
        //        EventTypeId = "SyncComplete",
        //        EventHandlerIds = new List<string>() { typeof(EmailEventHandler).Name }
        //    });

        //    return rules;
        //}
    }
}
