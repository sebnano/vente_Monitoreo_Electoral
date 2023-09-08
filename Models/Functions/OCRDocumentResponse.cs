using System;
using System.Text.Json.Serialization;

namespace ElectoralMonitoring
{
    public class Block
    {
        [JsonPropertyName("property")]
        public Property Property { get; set; }

        [JsonPropertyName("boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonPropertyName("paragraphs")]
        public List<Paragraph> Paragraphs { get; set; }

        [JsonPropertyName("blockType")]
        public string BlockType { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    public class BoundingBox
    {
        [JsonPropertyName("vertices")]
        public List<Vertex> Vertices { get; set; }

        [JsonPropertyName("normalizedVertices")]
        public List<NormalizedVertex> NormalizedVertices { get; set; }
    }

    public class Context
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }
    }

    public class Detail
    {
        [JsonPropertyName("@type")]
        public string Type { get; set; }
    }

    public class DetectedBreak
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("isPrefix")]
        public bool IsPrefix { get; set; }
    }

    public class DetectedLanguage
    {
        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    public class Error
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public List<Detail> Details { get; set; }
    }

    public class FullTextAnnotation
    {
        [JsonPropertyName("pages")]
        public List<Page> Pages { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class LabelAnnotation
    {
        [JsonPropertyName("pages")]
        public List<Page> Pages { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class NormalizedVertex
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class Page
    {
        [JsonPropertyName("property")]
        public Property Property { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("blocks")]
        public List<Block> Blocks { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    public class Paragraph
    {
        [JsonPropertyName("property")]
        public Property Property { get; set; }

        [JsonPropertyName("boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonPropertyName("words")]
        public List<Word> Words { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    public class Property
    {
        [JsonPropertyName("detectedLanguages")]
        public List<DetectedLanguage> DetectedLanguages { get; set; }

        [JsonPropertyName("detectedBreak")]
        public DetectedBreak DetectedBreak { get; set; }
    }

    public class Symbol
    {
        [JsonPropertyName("property")]
        public Property Property { get; set; }

        [JsonPropertyName("boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    public class TextAnnotation
    {
        [JsonPropertyName("pages")]
        public List<Page> Pages { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class Vertex
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }

    public class Word
    {
        [JsonPropertyName("property")]
        public Property Property { get; set; }

        [JsonPropertyName("boundingBox")]
        public BoundingBox BoundingBox { get; set; }

        [JsonPropertyName("symbols")]
        public List<Symbol> Symbols { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }
    }

    //public class OCRDocumentResponse
    //{
    //    [JsonPropertyName("responses")]
    //    public List<Response> Responses { get; set; }
    //}

    public class OCRDocumentResponse
    {
        [JsonPropertyName("labelAnnotations")]
        public List<LabelAnnotation> LabelAnnotations { get; set; }

        [JsonPropertyName("textAnnotations")]
        public List<TextAnnotation> TextAnnotations { get; set; }

        [JsonPropertyName("fullTextAnnotation")]
        public FullTextAnnotation FullTextAnnotation { get; set; }

        [JsonPropertyName("error")]
        public Error Error { get; set; }

        [JsonPropertyName("context")]
        public Context Context { get; set; }
    }
}

