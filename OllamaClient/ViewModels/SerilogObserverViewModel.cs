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
        private MemoryStream _TextStream { get; set; }
        private TextWriter _TextWriter { get; set; }

        public ObservableCollection<string> Logs { get; set; } = [];

        public SerilogObserverViewModel(DispatcherQueue dispatcherQueue)
        {
            _DispatcherQueue = dispatcherQueue;
            _TextStream = new();
            _TextWriter = new StreamWriter(_TextStream);
        }

        public void OnCompleted()
        {
            _TextStream.Dispose();
            _TextWriter.Dispose();
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
            _TextStream.Position = 0;
            _TextStream.SetLength(0);

            logEvent.RenderMessage(_TextWriter);

            _TextWriter.Flush();

            string render = Encoding.UTF8.GetString(_TextStream.ToArray());

            return $"{logEvent.Timestamp.ToLocalTime().ToString("HH:mm:ss")} [{logEvent.Level}] {render}\u00A0";
        }
    }
}
