using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CFSyncFolders.Forms;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Services;
using CFSyncFolders.Log;

namespace CFSyncFolders
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new MainForm());
        //}

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
            // Get path to executable
            string currentFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddTransient<IPlaceholderService, PlaceholderService>();
                    services.AddTransient<IAuditLog>((scope) =>
                    {
                        var placeholderService = scope.GetRequiredService<IPlaceholderService>();
                        var placeholderParameters = new Dictionary<string, object>()
                        {
                            { "date", DateTime.Now }
                        };

                        // Get log file
                        var logsFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("LogsFolder").ToString();
                        if (logsFolder.Equals("{default}"))
                        {
                            logsFolder = Path.Combine(currentFolder, "Logs");
                        }
                        logsFolder = placeholderService.GetWithPlaceholdersReplaced(logsFolder, placeholderParameters);                    
                        return new CSVAuditLogFile((Char)9, Path.Combine(logsFolder, "{date:MM-yyyy}"), placeholderService);
                    });
                    services.AddTransient<ISyncConfigurationService>((scope) =>
                    {
                        var placeholderService = scope.GetRequiredService<IPlaceholderService>();
                        var placeholderParameters = new Dictionary<string, object>()
                        {
                            { "date", DateTime.Now }
                        };

                        // Get data folder
                        var dataFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("DataFolder").ToString();
                        if (dataFolder.Equals("{default}"))
                        {
                            dataFolder = Path.Combine(currentFolder, "Configuration");
                        }
                        dataFolder = placeholderService.GetWithPlaceholdersReplaced(dataFolder, placeholderParameters);                    
                        return new SyncConfigurationService(dataFolder);
                    });
                    services.AddTransient<MainForm>();
                });
        }
    }
}
