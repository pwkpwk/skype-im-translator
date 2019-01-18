namespace SkypeImTranslator.WebRole.Data
{
    using Newtonsoft.Json;

    [JsonObject]
    public class TextTranslationRequestData
    {
        [JsonProperty(PropertyName = "text", Required = Required.Always)]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "tl", Required = Required.Always)]
        public string[] TargetLocales { get; set; }
    }
}