namespace OllamaClient.Services.Json
{
    public record struct Function
    {
        public string name { get; set; }
        public string description { get; set; }
        public FunctionParameters parameters { get; set; }

        public Function()
        {
            name = string.Empty;
            description = string.Empty;
            parameters = new FunctionParameters();
        }
    }
}
