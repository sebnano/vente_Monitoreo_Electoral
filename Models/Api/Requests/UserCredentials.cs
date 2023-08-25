using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public record Field(
        [property: JsonPropertyName("value")] object Value
    );

    public record UserRegister(
        [property: JsonPropertyName("name")] IReadOnlyList<Field> Name,
        [property: JsonPropertyName("mail")] IReadOnlyList<Field> Mail,
        [property: JsonPropertyName("pass")] IReadOnlyList<Field> Pass
    );

    public record User(
        [property: JsonPropertyName("uid")] IReadOnlyList<Field> Uid,
        [property: JsonPropertyName("name")] IReadOnlyList<Field> Name
    );

    public record UserCredentials(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("pass")] string Pass
    );
}

