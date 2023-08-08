using System;
using System.Text.Json.Serialization;

namespace ProRecords
{
    public record ServerResponse([property: JsonPropertyName("message")] string Message);

    public record LoginResponse(
        [property: JsonPropertyName("current_user")] CurrentUser CurrentUser,
        [property: JsonPropertyName("csrf_token")] string CsrfToken,
        [property: JsonPropertyName("logout_token")] string LogoutToken
    );

    public record CurrentUser(
        [property: JsonPropertyName("uid")] string Uid,
        [property: JsonPropertyName("name")] string Name
    );
}

