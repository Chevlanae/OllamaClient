using Microsoft.Extensions.Logging;
using Microsoft.JavaScript.NodeApi;
using OllamaClient.JsEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class ToolCollection : IToolCollection
    {
        private ILogger _Logger;
        private INodeJs _NodeJs;
        private Dictionary<string, JSReference> _JSFunctions { get; set; } = [];

        public List<Tool> Items { get; set; } = [];

        public ToolCollection(ILogger<ToolCollection> logger, INodeJs nodejs)
        {
            _Logger = logger;
            _NodeJs = nodejs;
        }

        public void Refresh()
        {
            Items.Clear();
            foreach (var fn in _JSFunctions)
            {
                Items.Add(new Tool(fn.Key, $"JavaScript function {fn.Key}", _NodeJs, fn.Value));
            }
        }

        public async Task ProcessJavascriptFile(string filename)
        {
            string content = await File.ReadAllTextAsync(filename);

            _JSFunctions = _NodeJs.Runtime.Run(() =>
            {
                Dictionary<string, JSReference> result = new();

                JSObject obj = (JSObject)JSValue.RunScript(content);

                foreach (var key in obj.Keys)
                {
                    var value = obj.TryGetValue(key, out JSValue fn) ? fn : default;

                    result.TryAdd((string)key, new(value));
                }

                return result;
            });

            Refresh();

        }
    }
}
