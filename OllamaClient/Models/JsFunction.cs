using Microsoft.JavaScript.NodeApi;
using OllamaClient.JsEngine;
using System;
using System.Text;

namespace OllamaClient.Models
{
    public enum ParameterType
    {
        Object
    }

    public enum PropertyType
    {
        Object,
        String,
        Integer,
        Boolean
    }

    public class JsFunction
    {
        private INodeJs _NodeJS;

        public JSReference Reference { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public JsFunctionParameters Parameters { get; set; }

        public JsFunction(string name, string description, JSReference reference, INodeJs nodejs, ParameterType? parameterType = null)
        {
            _NodeJS = nodejs;

            Name = name;
            Description = description;
            Reference = reference;
            Parameters = new JsFunctionParameters(parameterType ?? ParameterType.Object);

            ProcessReference();
        }

        private void ProcessReference()
        {
            _NodeJS.Runtime.Run(() =>
            {
                if (Reference.GetValue() is JSValue value)
                {
                    JSFunction fn = (JSFunction)value;

                    Name = fn.Name;
                }
            });
        }

        public T? CallFunction<T>() where T : struct, IJSValue<T>
        {
            return _NodeJS.Runtime.Run(() =>
            {
                if (Reference.GetValue() is JSValue value)
                {
                    JSFunction fn = (JSFunction)value;

                    JSValue[] jsArgs = new JSValue[Parameters.Properties.Count];
                    int i = 0;

                    foreach (var arg in Parameters.Properties)
                    {
                        jsArgs[i] = arg.Value.Type switch
                        {
                            PropertyType.String => JSValue.CreateStringUtf8(Encoding.UTF8.GetBytes((string)arg.Value.Value)),
                            PropertyType.Integer => JSValue.CreateBigInt((long)arg.Value.Value),
                            PropertyType.Boolean => JSValue.GetBoolean((bool)arg.Value.Value),
                            PropertyType.Object => JSValue.CreateObject().Wrap(arg.Value.Value),
                            _ => JSValue.Null
                        };
                        i++;
                    }

                    var result = fn.CallAsStatic();
                    return result.As<T>();
                }
                return default(T);
            });
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
    }
}
