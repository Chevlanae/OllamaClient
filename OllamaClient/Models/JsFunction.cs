using Microsoft.JavaScript.NodeApi;
using OllamaClient.JsEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public enum ParameterType
    {
        Object
    }

    public enum ReturnType
    {
        Object,
        String,
        Integer,
        Boolean
    }

    public class JsFunction
    {
        private INodeJs _NodeJS;
        private string? _Filename { get; set; }

        public JSReference Reference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public JsFunctionParameters Parameters { get; set; }

        public JsFunction(string name, string description, string filename, JSReference reference, INodeJs nodejs)
        {
            _NodeJS = nodejs;
            _Filename = filename;

            Name = name;
            Description = description;
            Reference = reference;
            Parameters = new JsFunctionParameters();


        }

        public Services.Json.Function ToJson()
        {
            return new()
            {
                name = Name,
                description = Description,
                parameters = Parameters.ToJson()
            };
        }

        public object Invoke(params object[]argsNet)
        {
            return _NodeJS.Runtime.Run(() =>
            {
                try
                {
                    JSValue[] argsJs = new JSValue[argsNet.Length];

                    for (int i = 0; i < argsNet.Length; i++)
                    {
                        switch (argsNet[i])
                        {
                            case string s:
                                argsJs[i] = JSValue.CreateStringUtf8(Encoding.UTF8.GetBytes(s));
                                break;
                            case int n:
                                argsJs[i] = JSValue.CreateNumber(n);
                                break;
                        }
                    }

                    JSFunction fn = (JSFunction)Reference.GetValue();

                    JSValue result = fn.Call(fn, argsJs);

                    object res = result.Unwrap("int");

                    return res;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error invoking function {Name} from file {_Filename}", ex);
                }
            });
        }
    }
}
