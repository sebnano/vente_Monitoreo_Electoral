using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class Node
    {
        [JsonPropertyName("target_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TargetId { get; set; }

        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; set; }
    }
}

