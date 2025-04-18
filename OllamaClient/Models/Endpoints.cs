namespace OllamaClient.Models
{
    public class Endpoints
    {
        public string BaseUrl { get; private set; }
        public string GenerateCompletion { get; private set; }
        public string Chat { get; private set; }
        public string Create { get; private set; }
        public string List { get; private set; }
        public string Show { get; private set; }
        public string Copy { get; private set; }
        public string Delete { get; private set; }
        public string Pull { get; private set; }
        public string Push { get; private set; }
        public string Embed { get; private set; }
        public string Ps { get; private set; }
        public string Version { get; private set; }

        public Endpoints(string socketAddress, bool useHttps = false)
        {
            BaseUrl = useHttps ? "https" : "http" + "://" + socketAddress + "/api/";
            GenerateCompletion = BaseUrl + "generate";
            Chat = BaseUrl + "chat";
            Create = BaseUrl + "create";
            List = BaseUrl + "tags";
            Show = BaseUrl + "show";
            Copy = BaseUrl + "copy";
            Delete = BaseUrl + "delete";
            Pull = BaseUrl + "pull";
            Push = BaseUrl + "push";
            Embed = BaseUrl + "embed";
            Ps = BaseUrl + "ps";
            Version = BaseUrl + "version";
        }
    }
}
