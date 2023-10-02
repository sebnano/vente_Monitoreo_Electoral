using System;
using Refit;

namespace ElectoralMonitoring
{
    [Headers("Accept: application/json", "Content-Type: application/json", "User-Agent: " + nameof(ElectoralMonitoring) + "App", "")]
    public interface INodeApi
    {
        [Get("/api/registro-de-actas-por-usuarios/{userId}?_format=json")]
        Task<List<Minute>> GetMinutesByUser(string userId);

        [Get("/api/registro-de-actas/{ccv}/{table}?_format=json")]
        Task<List<Minute>> GetMinutesByCcvAndTable(string ccv, string table);

        [Get("/jsonapi/node/registro_de_actas")]
        Task<NodeResponse<MinuteAttributes, MinuteRelationships>> GetMinutes();

        [Get("/entity/entity_form_display/node.registro_de_actas.default?_format=json")]
        Task<FormResponse> GetMinutesForm();

        [Get("/api/campos-por-content-type/registro_de_actas?_format=json")]
        Task<List<FieldForm>> GetMinutesFormFields();

        [Post("/node?_format=json")]
        Task<Dictionary<string, List<Node>>> CreateNode([Body] object body);

        [Get("/garantes-y-sus-centros/{userId}?_format=json")]
        Task<List<VotingCenter>> GetVotingCenters(string userId);

        [Post("/file/upload/node/registro_de_actas/field_image?_format=json")]
        [Headers("Content-Type: application/octet-stream", "Accept: application/vnd.api+json")]
        Task<Dictionary<string, List<Node>>> UploadFile([Header("Content-Disposition")] string contentDisposition, [Body] FileStream bytes);
    }
}

