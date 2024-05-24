using System;
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
                    services.AddTransient<IAuditLog>((scope) =>
                    {                        
                        // Get log file
                        var logsFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("LogsFolder").ToString();
                        if (logsFolder.Equals("{default}"))
                        {
                            logsFolder = Path.Combine(currentFolder, "Logs");
                        }
                        logsFolder = logsFolder.Replace("{user}", Environment.UserName);
                        logsFolder = logsFolder.Replace("{machine}", Environment.MachineName);
                        
                        return new CSVAuditLogFile(Path.Combine(logsFolder, "{date}"));
                    });
                    services.AddTransient<ISyncConfigurationService>((scope) =>
                    {
                        // Get data folder
                        var dataFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("DataFolder").ToString();
                        if (dataFolder.Equals("{default}"))
                        {
                            dataFolder = Path.Combine(currentFolder, "Configuration");
                        }
                        dataFolder = dataFolder.Replace("{user}", Environment.UserName);
                        dataFolder = dataFolder.Replace("{machine}", Environment.MachineName);

                        return new SyncConfigurationService(dataFolder);
                    });
                    services.AddTransient<MainForm>();
                });
        }
    }
}
