using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class Feature
    {
        public Feature(string type, int maxResults)
        {
            Type = type;
            MaxResults = maxResults;
        }
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; }
    }

    public class Image
    {
        public Image(string content)
        {
            Content = content;
        }
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class Request
    {
        public Request(Image img, List<Feature> features)
        {
            Image = img;
            Features = features;
        }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("features")]
        public List<Feature> Features { get; set; }
    }

    public class OCRDocument
    {
        public OCRDocument(Request requests)
        {
            Requests = requests;
        }

        [JsonPropertyName("requests")]
        public Request Requests { get; set; }
    }

    public class OCRDocumentRequest
    {
        public OCRDocument Data { get; set; }

        public OCRDocumentRequest(string imageContent)
        {
            Data = new OCRDocument(
                        new Request(new Image(imageContent), new List<Feature>()
                        {
                            new("DOCUMENT_TEXT_DETECTION", 1)
                        }));
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(Data.Requests);
        }
    }
}

