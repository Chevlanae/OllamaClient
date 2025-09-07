using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public interface IToolCollection
    {
        public List<Tool> Items { get; set; }

        Task ProcessJavascriptFile(string filename);
    }
}