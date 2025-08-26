﻿namespace OllamaClient.Services.Json
{
    public record struct ModelParameters
    {
        public int? mirostat { get; set; }
        public float? mirostat_eta { get; set; }
        public float? mirostat_tau { get; set; }
        public int? num_ctx { get; set; }
        public int? repeat_last_n { get; set; }
        public float? repeat_penalty { get; set; }
        public float? temperature { get; set; }
        public int? seed { get; set; }
        public string? stop { get; set; }
        public int? num_predict { get; set; }
        public int? num_keep { get; set; }
        public int? top_k { get; set; }
        public float? top_p { get; set; }
        public float? min_p { get; set; }
    }
}
