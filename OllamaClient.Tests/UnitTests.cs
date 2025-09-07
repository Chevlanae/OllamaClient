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

            string currentDirectory = Directory.GetCurrentDirectory();
            string mockFilesDir = Path.Combine(currentDirectory, "OllamaClient.Tests\\Mocks\\Typescript");
            string mockFile = Path.Combine(mockFilesDir, "TestFunctions.ts");

            await collection.ProcessJavascriptFile(mockFile);
        }
    }
}
