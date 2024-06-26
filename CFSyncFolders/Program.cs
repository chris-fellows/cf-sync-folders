﻿using System;
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
                        return new CSVLogger((Char)9, Path.Combine(logsFolder, "{date:MM-yyyy}"), placeholderService);
                    });
                    services.AddTransient<ISyncConfigurationService>((scope) =>
                    {
                        var placeholderService = scope.GetRequiredService<IPlaceholderService>();

                        // Get data folder
                        var dataFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("DataFolder").ToString();                                           
                        dataFolder = placeholderService.GetWithPlaceholdersReplaced(dataFolder, new Dictionary<string, object>());                    
                        return new SyncConfigurationService(dataFolder);
                    });
                    services.AddTransient<MainForm>();
                });
        }
    }
}
