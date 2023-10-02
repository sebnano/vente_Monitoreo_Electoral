using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public static class RefitExtensions
    {
        public static T For<T>(string hostUrl) => RestService.For<T>(hostUrl, GetJsonRefitSettings());
        public static T For<T>(HttpClient client) => RestService.For<T>(client, GetJsonRefitSettings());

        public static RefitSettings GetJsonRefitSettings() => new(new SystemTextJsonContentSerializer(new JsonSerializerOptions()));
    }
}

