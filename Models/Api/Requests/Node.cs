using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class Node
    {
        [JsonPropertyName("target_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? TargetId { get; set; }

        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Value { get; set; }
        
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }
    }

    public class SavedNode
    {
        public string Id { get; set; }
        public string? NodeId { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public Dictionary<string,List<Node>> values { get; set; }
    }
}

