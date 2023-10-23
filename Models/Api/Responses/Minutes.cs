using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

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

    public partial class DocumentDTO : ObservableObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }

        [ObservableProperty]
        string icon;
    }
}

