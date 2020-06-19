using Newtonsoft.Json;
using System.Linq;

namespace CoolapkUWP.Models.Json.IndexPageHeaderItemModel
{
    public class Item
    {
        public Item(Entity entity)
        {
            Title = entity.Title;
            Uri = entity.Uri;
            if (entity.Entities.Length > 0)
            {
                Entities = (from e in entity.Entities
                            where e.EntityType == "page"
                            select new Item(e)).ToArray();
            }
        }

        private Item(Entity1 entity)
        {
            Title = entity.Title;
            Uri = entity.Uri;
            IsInGroup = true;
        }

        public string Title { get; private set; }
        public string Uri { get; private set; }
        public Item[] Entities { get; private set; }
        public bool IsInGroup { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Rootobject
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Datum
    {
        [JsonProperty("entityTemplate")]
        public string EntityTemplate { get; set; }
        [JsonProperty("entities")]
        public Entity[] Entities { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Entity
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Uri { get; set; }
        [JsonProperty("page_name")]
        public string Page_name { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }
        [JsonProperty("entities")]
        public Entity1[] Entities { get; set; }
        //[JsonProperty("extraData")]
        //public string ExtraData { get; set; }
        [JsonProperty("page_visibility")]
        public int Page_visibility { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Entity1
    {
        [JsonProperty("entityType")]
        public string EntityType { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Uri { get; set; }
    }
}