using Microsoft.Extensions.Logging;
using Microsoft.JavaScript.NodeApi.Runtime;
using System;
using System.IO;

namespace OllamaClient.JsEngine
{
    public class NodeJs : INodeJs
    {
        private readonly ILogger _Logger;
        private NodeEmbeddingPlatform _Platform;

        public NodeEmbeddingThreadRuntime Runtime { get; }

        public NodeJs(ILogger<INodeJs> logger)
        {
            _Logger = logger;
            _Platform = new(new());

            Runtime = _Platform.CreateThreadRuntime();
        }
    }
}
