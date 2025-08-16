using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace OllamaClient.Services
{
    public interface IDialogsService
    {
        Task QueueDialog(ContentDialog dialog);
    }
}