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

    public record ClientCredentials(
        [property: JsonPropertyName("grant_type")] string GrantType,
        [property: JsonPropertyName("client_id")] string ClientId,
        [property: JsonPropertyName("client_secret")] string ClientSecret,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("password")] string Password
    );

    public record RefreshTokenCredentials(
        [property: JsonPropertyName("grant_type")] string GrantType,
        [property: JsonPropertyName("client_id")] string ClientId,
        [property: JsonPropertyName("client_secret")] string ClientSecret,
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );
}

