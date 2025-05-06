using System;
using System.IO;

namespace OllamaClient.Services
{
    internal static class Paths
    {
        private static readonly string _AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string ParentDirectory = Path.Combine(_AppData, "OllamaClient");
        public static readonly string State = Path.Combine(ParentDirectory, "State");
        public static readonly string Logs = Path.Combine(ParentDirectory, "Logs");
        public static readonly string Settings = Path.Combine(ParentDirectory, "Settings");

        static Paths()
        {
            if (!Directory.Exists(ParentDirectory)) Directory.CreateDirectory(ParentDirectory);
            if (!Directory.Exists(State)) Directory.CreateDirectory(State);
            if (!Directory.Exists(Logs)) Directory.CreateDirectory(Logs);
            if (!Directory.Exists(Settings)) Directory.CreateDirectory(Settings);
        }
    }
}
