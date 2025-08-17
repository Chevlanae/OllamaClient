using Microsoft.UI.Dispatching;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace OllamaClient.ViewModels
{
    public class SerilogObserverViewModel : IObserver<LogEvent>
    {
        private DispatcherQueue _DispatcherQueue { get; set; }

        public ObservableCollection<string> Logs { get; set; } = [];

        public SerilogObserverViewModel(DispatcherQueue dispatcherQueue)
        {
            _DispatcherQueue = dispatcherQueue;
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception e)
        {
            throw e;
        }

        public void OnNext(LogEvent logEvent)
        {
            _DispatcherQueue.TryEnqueue(() => Logs.Add(RenderMessage(logEvent)));
        }

        private string RenderMessage(LogEvent logEvent)
        {
            using MemoryStream stream = new();
            using TextWriter writer = new StreamWriter(stream);

            logEvent.RenderMessage(writer);

            writer.Flush();

            string render = Encoding.UTF8.GetString(stream.ToArray());

            return $"{logEvent.Timestamp.ToLocalTime().ToString("HH:mm:ss")} [{logEvent.Level}] {render}\u00A0";
        }
    }
}
