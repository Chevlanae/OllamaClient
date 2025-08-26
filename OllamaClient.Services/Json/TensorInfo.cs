namespace OllamaClient.Services.Json
{
    public record struct TensorInfo
    {
        public string name { get; set; }
        public string type { get; set; }
        public int[] shape { get; set; }
    }
}