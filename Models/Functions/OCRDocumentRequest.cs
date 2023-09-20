using System;
using System.Text.Encodings.Web;
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
        public Image(ImageType type, string content)
        {
            if(type == ImageType.URI)
            {
                Source = new(content);
            }else if(type == ImageType.BASE64)
            {
                Content = content;
            }
            
        }
        
        [JsonPropertyName("source")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Source? Source { get; set; }

        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Content { get; set; }

    }

    public enum ImageType
    {
        URI,
        BASE64
    }

    public class Source
    {
        [JsonPropertyName("imageUri")]
        public string ImageUri { get; set; }

        public Source(string imageUri)
        {
            ImageUri = imageUri;
        }
    }

    public class Request
    {
        public Request(Image img, List<Feature> features)
        {
            Image = img;
            Features = features;
            ImageContext = new();
        }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("features")]
        public List<Feature> Features { get; set; }

        [JsonPropertyName("imageContext")]
        public ImageContext ImageContext { get; set; }
    }

    public class ImageContext
    {
        public ImageContext()
        {
            LanguageHints = new() { "es" };
        }

        [JsonPropertyName("languageHints")]
        public List<string> LanguageHints { get; set; }
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

        public OCRDocumentRequest(ImageType type, string imageContent)
        {
            Data = new OCRDocument(
                        new Request(new Image(type, imageContent), new List<Feature>()
                        {
                            new("DOCUMENT_TEXT_DETECTION", 1)
                        }));
        }

        public string ToJson()
        {
            var options3 = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(Data.Requests, options3);
        }
    }
}

