
using Newtonsoft.Json;

namespace translator.classes
{
    public class Rootobject
    {
        [JsonProperty(PropertyName = "total_rows")]
        public int TotalRows { get; set; }

        [JsonProperty(PropertyName = "offset")]
        public int Offset { get; set; }

        [JsonProperty(PropertyName = "rows")]
        public Row[] Rows { get; set; }
    }

    public class Row
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "key")]
        public string key { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public Value Value { get; set; }
    }

    public class Value
    {
        [JsonProperty(PropertyName = "_id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "_rev")]
        public string Rev { get; set; }

        public string Word { get; set; }

        public string Text { get; set; }

        [JsonProperty(PropertyName = "wordID")]
        public object WordId { get; set; }

        public string DictName { get; set; }

        public int DictType { get; set; }

        [JsonProperty(PropertyName = "soundcode")]
        public int SoundCode { get; set; }

        [JsonProperty(PropertyName = "contributedby")]
        public string ContributedBy { get; set; }
    }
}
