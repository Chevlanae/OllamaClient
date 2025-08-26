namespace OllamaClient.Services.Json
{
    public record struct StatusResponse
    {
        public string status { get; set; }
        public string? digestname { get; set; }
        public long? total { get; set; }
        public long? completed { get; set; }
    }
}