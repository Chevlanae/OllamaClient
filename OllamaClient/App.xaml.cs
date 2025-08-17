using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static string LocalAppDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\OllamaClient";
        public static string MsixLocalAppDataPath = $"{ApplicationData.Current.LocalCacheFolder.Path}\\Local\\OllamaClient";
        public static string LogsDirectoryPath = $"{LocalAppDataPath}\\Logs";
        public static string LogsDirectoryMsixPath = $"{MsixLocalAppDataPath}\\Logs";
        public static StringWriter LoggedText = new StringWriter();
        public static string? EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        private static readonly IHost _Host = Host.CreateDefaultBuilder()
            .UseEnvironment(EnvironmentName ?? "Production")
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
            })
            .ConfigureServices((context, services) =>
            {
                //Logging
                services.AddSerilog((context, config) =>
                {
                    config.MinimumLevel.Debug();
                    config.WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug);
                    config.WriteTo.TextWriter(
                        LoggedText,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}");
                    config.WriteTo.File($"{LocalAppDataPath}\\Logs\\log.txt", rollingInterval: RollingInterval.Day);
                });

                //Settings
                services.Configure<OllamaApiService.Settings>(context.Configuration.GetSection("OllamaApiService.Settings"));
                services.Configure<SerializeableStorageService.Settings>(context.Configuration.GetSection("SerializableStorageService.Settings"));
                services.Configure<Conversation.Settings>(context.Configuration.GetSection("Conversation.Settings"));

                //Services
                services.AddSingleton<IDialogsService, DialogsService>();
                services.AddSingleton<IOllamaApiService, OllamaApiService>();
                services.AddSingleton<ISerializeableStorageService, SerializeableStorageService>();

                //Models
                services.AddTransient<IConversation, Conversation>();
                services.AddSingleton<IConversationCollection, ConversationCollection>();
                services.AddTransient<IModel, Model>();
                services.AddSingleton<IModelCollection, ModelCollection>();

                //Viewmodels
            })
            .Build();

        private Window? MainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        public static T GetRequiredService<T>() where T : class
        {
            return _Host.Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _Host.Start();
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
