using System;
using System.IO;

namespace OllamaClient.Services
{
    internal static class Paths
    {
        private static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string ParentDirectory = Path.Combine(AppData, "OllamaClient");
        public static readonly string Persistence = Path.Combine(ParentDirectory, "Persistence");
        public static readonly string Logs = Path.Combine(ParentDirectory, "Logs");
        public static readonly string Settings = Path.Combine(ParentDirectory, "Settings");

        static Paths()
        {
            if (!Directory.Exists(ParentDirectory)) Directory.CreateDirectory(ParentDirectory);
            if (!Directory.Exists(Persistence)) Directory.CreateDirectory(Persistence);
            if (!Directory.Exists(Logs)) Directory.CreateDirectory(Logs);
            if (!Directory.Exists(Settings)) Directory.CreateDirectory(Settings);
        }
    }
}
