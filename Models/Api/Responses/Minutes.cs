using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class Minute
    {
        public string field_centro_de_votacion { get; set; }
        public string field_mesa { get; set; }
        public string nid { get; set; }

        [JsonIgnore]
        public string Icon { get; set; }
    }

    public class DocumentDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Icon { get; set; }
    }
}

