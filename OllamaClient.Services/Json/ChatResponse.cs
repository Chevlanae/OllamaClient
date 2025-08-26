namespace OllamaClient.Services.Json
{
    public record struct ChatResponse
    {
        public string? model { get; set; }
        public string? created_at { get; set; }
        public Message? message { get; set; }
        public bool? done { get; set; }
        public string? done_reason { get; set; }
        public long? total_duration { get; set; }
        public long? load_duration { get; set; }
        public int? prompt_eval_count { get; set; }
        public long? prompt_eval_duration { get; set; }
        public int? eval_count { get; set; }
        public long? eval_duration { get; set; }
    }
}
