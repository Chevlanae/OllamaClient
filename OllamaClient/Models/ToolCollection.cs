using Microsoft.Extensions.Logging;
using Microsoft.JavaScript.NodeApi;
using OllamaClient.JsEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class ToolCollection : IToolCollection
    {
        public class JSFunctionReference(string name, string filename, JSReference reference)
        {
            public string Name { get; set; } = name;
            public string Filename { get; set; } = filename;
            public JSReference Reference { get; set; } = reference;
        }

        private ILogger _Logger;
        private INodeJs _NodeJs;
        private List<JSFunctionReference> _JSFunctions { get; set; } = [];

        public List<Tool> Items { get; set; } = [];

        public ToolCollection(ILogger<ToolCollection> logger, INodeJs nodejs)
        {
            _Logger = logger;
            _NodeJs = nodejs;
        }

        public async Task ProcessJavascriptFile(string filename)
        {
            string content = await File.ReadAllTextAsync(filename);

            _JSFunctions = _NodeJs.Runtime.Run(() =>
            {
                List<JSFunctionReference> result = new();

                try
                {
                    JSObject obj = (JSObject)JSValue.RunScript(content);

                    foreach (var key in obj.Keys)
                    {
                        JSFunction? value = obj.TryGetValue(key, out JSValue fn) ? (JSFunction)fn : default;

                        if (value is not null)
                        {
                            JSFunctionReference reference = new(value.Value.Name, filename, new(value.Value));

                            result.Add(reference);

                        }
                    }
                }
                catch(Exception e)
                {
                    _Logger.LogError(e, $"Error processing javascript file at {filename}");
                }

                return result;
            });


            foreach (var fn in _JSFunctions)
            {
                Items.Add(new Tool(fn.Name, $"JavaScript function {fn.Name}", _NodeJs, fn.Reference, fn.Filename));
            }
        }
    }
}
