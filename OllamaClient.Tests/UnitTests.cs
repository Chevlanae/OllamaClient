using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OllamaClient.JsEngine;
using OllamaClient.Models;
using System.IO;
using System.Threading.Tasks;

namespace OllamaClient.Tests
{
    [TestClass]
    public partial class Models
    {
        [TestMethod]
        public async Task TestToolCollection()
        {
            LoggerFactory factory = new();

            ToolCollection collection = new(
                new Logger<ToolCollection>(factory),
                new NodeJs(new Logger<INodeJs>(factory))
                );

            string mockFile = Path.Combine("C:\\Users\\leahv\\source\\repos\\Chevlanae\\OllamaClient\\OllamaClient.Tests\\Mocks\\Javascript", "TestFunctions.js");

            await collection.ProcessJavascriptFile(mockFile);

            foreach(var item in collection.Items)
            {
                item.Function.Invoke(1, 2);
            }
        }
    }
}
