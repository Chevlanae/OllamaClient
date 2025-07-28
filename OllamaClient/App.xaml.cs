using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using Serilog;
using System;

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

        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration((context, config) =>
            {
                config
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSerilog((context, config) =>
                {
                    config.WriteTo.Debug();
                    config.WriteTo.File($"{LocalAppDataPath}\\log.txt");
                });

                //Settings
                services.Configure<OllamaApiService.Settings>(context.Configuration.GetSection("OllamaApiService.Settings"));
                services.Configure<SerializeableStorageService.Settings>(context.Configuration.GetSection("SerializableStorageService.Settings"));
                services.Configure<ConversationViewModel.Settings>(context.Configuration.GetSection("ConversationViewModel.Settings"));

                //Services
                services.AddSingleton<DialogsService>();
                services.AddSingleton<OllamaApiService>();
                services.AddSingleton<SerializeableStorageService>();

                //Viewmodels
                services.AddTransient<ConversationViewModel>();
                services.AddSingleton<ConversationSidebarViewModel>();
                services.AddTransient<ModelViewModel>();
                services.AddSingleton<ModelSidebarViewModel>();
            })
            .Build();

        private Window? m_window;

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
