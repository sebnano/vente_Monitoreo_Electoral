using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class BooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    return reader.GetString() switch
                    {
                        "1" => true,
                        "0" => false,
                        _ => throw new JsonException()
                    };
                default:
                    throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }

    public class FieldForm
    {
        [JsonPropertyName("numero_candidato")]
        public string NumeroCandidato { get; set; }

        [JsonPropertyName("campo")]
        public string Key { get; set; }

        [JsonPropertyName("field_mapeo_texto")]
        public string FieldMapeoTexto { get; set; }

        [JsonPropertyName("field_content_type")]
        public string FieldContentType { get; set; }

        [JsonPropertyName("parent")]
        public string ContentTypeParent { get; set; }

        [JsonPropertyName("weight")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Weight { get; set; }

        [JsonPropertyName("grupo")]
        public string Grupo { get; set; }

        [JsonPropertyName("Field_type")]
        public string Type { get; set; }

        [JsonPropertyName("form_default_value")]
        public string? DefaultValue { get; set; }

        [JsonPropertyName("form_value_available")]
        public string? ValuesAvailable { get; set; }

        [JsonConverter(typeof(BooleanConverter))]
        [JsonPropertyName("campo_obligatorio")]
        public bool Required { get; set; }

        [JsonConverter(typeof(BooleanConverter))]
        [JsonPropertyName("campo_para_escanear")]
        public bool NeedScan { get; set; }

        [JsonConverter(typeof(BooleanConverter))]
        [JsonPropertyName("campo_activo")]
        public bool Active { get; set; }

        [JsonConverter(typeof(BooleanConverter))]
        [JsonPropertyName("campo_oculto")]
        public bool Hidden { get; set; }

        public const string NUMBER = "number"; 
        public const string TEXT = "string_textfield";
        public const string TEXTAREA = "string_textarea";
        public const string REFERENCE = "entity_reference_autocomplete";
        public const string IMAGE = "image_image";
        public const string OPTIONS = "options_select";
    }


    public class Content
    {
        public FormField body { get; set; }
        public FormField created { get; set; }
        public FormField field_boletas_escrutadas { get; set; }
        public FormField field_cargo_del_portador_del_act { get; set; }
        public FormField field_cedula_miembro_de_mesa { get; set; }
        public FormField field_cedula_portador_del_acta { get; set; }
        public FormField field_cedula_presidente_de_mesa { get; set; }
        public FormField field_cedula_secretario_de_mesa { get; set; }
        public FormField field_centro_de_votacion { get; set; }
        public FormField field_hora_cierre_de_mesa { get; set; }
        public FormField field_hora_fin_del_escrutinio { get; set; }
        public FormField field_image { get; set; }
        public FormField field_mesa { get; set; }
        public FormField field_nombre_miembro_de_mesa { get; set; }
        public FormField field_nombre_portador_acta { get; set; }
        public FormField field_nombre_presidente_de_mesa { get; set; }
        public FormField field_nombre_secretario_miembro_ { get; set; }
        public FormField field_observaciones { get; set; }
        public FormField field_participantes_segun_cuader { get; set; }
        public FormField field_votacion_a_observar { get; set; }
        public FormField field_votos_nulos { get; set; }
        public FormField field_votos_por_candidatos { get; set; }
        public FormField langcode { get; set; }
        public FormField path { get; set; }
        public FormField promote { get; set; }
        public FormField status { get; set; }
        public FormField sticky { get; set; }
        public FormField title { get; set; }
        public FormField uid { get; set; }
    }

    public class Dependencies
    {
        public List<string> config { get; set; }
        public List<string> module { get; set; }
    }

    public class Features
    {
        public string collapse_edit_all { get; set; }
        public string duplicate { get; set; }
    }

    public class FormField
    {
        public string type { get; set; }
        public int weight { get; set; }
    }

    public class FormResponse
    {
        public string uuid { get; set; }
        public string langcode { get; set; }
        public bool status { get; set; }
        public Dependencies dependencies { get; set; }
        public string id { get; set; }
        public string targetEntityType { get; set; }
        public string bundle { get; set; }
        public string mode { get; set; }
        public Dictionary<string, FormField> content { get; set; }
        //public Content content { get; set; }
        public List<object> hidden { get; set; }
    }

    public class Settings
    {
        public int rows { get; set; }
        public int summary_rows { get; set; }
        public string placeholder { get; set; }
        public bool show_summary { get; set; }
        public int size { get; set; }
        public string progress_indicator { get; set; }
        public string preview_image_style { get; set; }
        public string match_operator { get; set; }
        public int match_limit { get; set; }
        public string title { get; set; }
        public string title_plural { get; set; }
        public string edit_mode { get; set; }
        public string closed_mode { get; set; }
        public string autocollapse { get; set; }
        public int closed_mode_threshold { get; set; }
        public string add_mode { get; set; }
        public string form_display_mode { get; set; }
        public string default_paragraph_type { get; set; }
        public Features features { get; set; }
        public bool include_locked { get; set; }
        public bool display_label { get; set; }
    }
}

