using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Hosting;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole;
using Serilog;
using Microsoft.Extensions.Configuration;
using OllamaClient.Services;
using System;
using System.Threading;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        private Window? m_window;

        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
            })
            .ConfigureServices((context, services) =>
            {
                //Settings
                services.Configure<OllamaApiService.Settings>(context.Configuration.GetSection("OllamaApi.Settings"));
                services.Configure<SerializeableStorageService.Settings>(context.Configuration.GetSection("SerializableStorage.Settings"));
                
                //Singleton Services
                services.AddSingleton<DialogsService>();
                services.AddSingleton<OllamaApiService>();
                services.AddSingleton<SerializeableStorageService>();

                //Viewmodels
                services.AddTransient<ConversationViewModel>();
                services.AddSingleton<ConversationSidebarViewModel>();
                services.AddTransient<ModelViewModel>();
                services.AddSingleton<ModelSidebarViewModel>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddSerilog(new LoggerConfiguration()
                    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                    .WriteTo.Debug(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                    .WriteTo.File("logs.txt")
                    .CreateLogger()
                );
            })
            .Build();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        public static T? GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _host.Start();
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
