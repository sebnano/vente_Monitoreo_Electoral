using System;
using Refit;

namespace ElectoralMonitoring
{
    [Headers("Accept: application/json", "Content-Type: application/json", "Accept-Encoding: gzip, deflate, br", "User-Agent: " + nameof(ElectoralMonitoring) + "App", "")]
    public interface INodeApi
    {
        [Get("/api/registro-de-actas-por-usuarios/{userId}?_format=json")]
        Task<List<Minute>> GetMinutesByUser([AliasAs("userId")] string userId);

        [Get("/jsonapi/node/registro_de_actas")]
        Task<NodeResponse<MinuteAttributes, MinuteRelationships>> GetMinutes();

        [Get("/entity/entity_form_display/node.registro_de_actas.default?_format=json")]
        Task<FormResponse> GetMinutesForm();
    }
}

