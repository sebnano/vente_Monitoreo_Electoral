using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class VotingCentersAttrs : NodeAttributes
    {
        public long field_codigo_centro_votacion { get; set; }
        public string field_direccion_centro_votacion { get; set; }
    }

    public class MinuteAttributes : NodeAttributes
    {
        public object field_boletas_escrutadas { get; set; }
        public object field_cargo_del_portador_del_act { get; set; }
        public object field_cedula_miembro_de_mesa { get; set; }
        public object field_cedula_portador_del_acta { get; set; }
        public object field_cedula_presidente_de_mesa { get; set; }
        public object field_cedula_secretario_de_mesa { get; set; }
        public object field_hora_cierre_de_mesa { get; set; }
        public object field_hora_fin_del_escrutinio { get; set; }
        public int field_mesa { get; set; }
        public object field_nombre_miembro_de_mesa { get; set; }
        public object field_nombre_portador_acta { get; set; }
        public object field_nombre_presidente_de_mesa { get; set; }
        public object field_nombre_secretario_miembro_ { get; set; }
        public object field_observaciones { get; set; }
        public object field_participantes_segun_cuader { get; set; }
        public object field_votos_nulos { get; set; }
    }

    public class MinuteRelationships : NodeRelationships
    {
        public NodeType field_centro_de_votacion { get; set; }
        public NodeType field_image { get; set; }
        public NodeType field_votacion_a_observar { get; set; }
        public NodeTypeList field_votos_por_candidatos { get; set; }
    }

    public abstract class NodeAttributes
    {
        public int drupal_internal__nid { get; set; }
        public int drupal_internal__vid { get; set; }
        public string langcode { get; set; }
        public DateTime revision_timestamp { get; set; }
        public object revision_log { get; set; }
        public bool status { get; set; }
        public string title { get; set; }
        public DateTime created { get; set; }
        public DateTime changed { get; set; }
        public bool promote { get; set; }
        public bool sticky { get; set; }
        public bool default_langcode { get; set; }
        public bool revision_translation_affected { get; set; }
        public Path path { get; set; }
        public object body { get; set; }
    }

    public class Data<N,R> where N : NodeAttributes where R : NodeRelationships
    {
        public string type { get; set; }
        public string id { get; set; }
        public Links links { get; set; }
        public N attributes { get; set; }
        public R relationships { get; set; }
    }

    public class NodeTypeData
    {
        public string type { get; set; }
        public string id { get; set; }
        public Meta meta { get; set; }
    }

    public class NodeTypeList
    {
        public List<NodeTypeData> data { get; set; }
        public Links links { get; set; }
    }

    public class Meta
    {
        public Links links { get; set; }
        public object drupal_internal__target_id { get; set; }
        public string about { get; set; }
        public string alt { get; set; }
        public string title { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object target_revision_id { get; set; }
    }

    public class NodeType
    {
        public NodeTypeData data { get; set; }
        public Links links { get; set; }
    }

    public class Path
    {
        public object alias { get; set; }
        public object pid { get; set; }
        public string langcode { get; set; }
    }

    public class NodeRelationships
    {
        public NodeType node_type { get; set; }
        public NodeType revision_uid { get; set; }
        public NodeType uid { get; set; }

    }

    public class NodeResponse<N, R> where N : NodeAttributes where R : NodeRelationships
    {
        public Jsonapi jsonapi { get; set; }
        public List<Data<N,R>> data { get; set; }
        public Links links { get; set; }
    }

    public class Jsonapi
    {
        public string version { get; set; }
        public Meta meta { get; set; }
    }

    public class Links
    {
        public Link self { get; set; }
        public Link related { get; set; }
        public Link help { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
        public Meta meta { get; set; }
    }
}

