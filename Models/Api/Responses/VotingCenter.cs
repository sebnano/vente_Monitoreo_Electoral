using System;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public record VotingCenter(
        [property: JsonPropertyName("userid")] string Userid,
        [property: JsonPropertyName("nombre_votacion")] string NombreVotacion,
        [property: JsonPropertyName("id_votacion")] string IdVotacion,
        [property: JsonPropertyName("nombre_centro_votacion")] string NombreCentroVotacion,
        [property: JsonPropertyName("id_centro_votacion")] string IdCentroVotacion,
        [property: JsonPropertyName("mesa")] string Mesa,
        [property: JsonPropertyName("taxonomia")] string Taxonomia,
        [property: JsonPropertyName("nombre_ubicacion")] string NombreUbicacion,
        [property: JsonPropertyName("codCNE_centro_votacion")] string CodCNECentroVotacion
    );
}

