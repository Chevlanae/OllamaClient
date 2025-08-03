namespace OllamaClient.Json
{
    public record struct ModelParameterKeyValue : IModelParameter
    {
        public ModelParameterKey Key { get; set; }
        public string Value { get; set; }
        public ModelParameterKeyValue(ModelParameterKey key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
