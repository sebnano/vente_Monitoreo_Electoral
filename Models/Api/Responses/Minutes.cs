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
}

