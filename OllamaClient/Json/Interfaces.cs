namespace OllamaClient.Json
{
    public interface IMessage
    {
        string? role { get; set; }
        string? content { get; set; }
    }

    public interface IModelParameter
    {
        ModelParameterKey Key { get; set; }
        string Value { get; set; }
    }
}
