namespace OllamaClient.Services.Json
{
    public record struct Tool
    {
        public string type { get; set; }
        public Function? function { get; set; }
    }
}
