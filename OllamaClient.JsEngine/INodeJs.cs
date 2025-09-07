using Microsoft.JavaScript.NodeApi.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.JsEngine
{
    public interface INodeJs
    {
        public NodeEmbeddingThreadRuntime Runtime { get; }
    }
}
