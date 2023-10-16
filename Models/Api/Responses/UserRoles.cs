using System;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
	public class UserRoles
	{
        [JsonPropertyName("usuario")]
        public string User { get; set; }

        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class AppConfig
    {
        [JsonPropertyName("grupo")]
        public string Grupo { get; set; }

        [JsonPropertyName("sesion")]
        public string Sesion { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        [JsonConverter(typeof(BooleanConverter))]
        public bool Value { get; set; }
    }

    public class AppOptions
    {
        public static string MINUTES_ADD = "registro_de_actas_crear";
        public static string MINUTES_EDIT = "registro_de_actas_edit";
        public static string REPORT_OPENED = "reporte_apertura_crear";
        public static string REPORT_SCHEDULED = "reporte_programado_crear";
        public static string REPORT_INCIDENCE = "reporte_de_incidencia_crear";

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }

        [JsonPropertyName("option_icon")]
        public string OptionIcon { get; set; }

        [JsonPropertyName("option_title")]
        public string OptionTitle { get; set; }

        [JsonPropertyName("rol")]
        public string Rol { get; set; }

        [JsonPropertyName("option_key")]
        public string OptionKey { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; }
    }


}

