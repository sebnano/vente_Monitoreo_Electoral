using System;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public record ServerResponse([property: JsonPropertyName("message")] string Message);

    public record LoginResponse(
        [property: JsonPropertyName("current_user")] CurrentUser CurrentUser,
        [property: JsonPropertyName("csrf_token")] string CsrfToken,
        [property: JsonPropertyName("logout_token")] string LogoutToken
    );

    public record TokenResponse(
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpireIn,
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );

    public record CurrentUser(
        [property: JsonPropertyName("uid")] string Uid,
        [property: JsonPropertyName("name")] string Name
    );
}

