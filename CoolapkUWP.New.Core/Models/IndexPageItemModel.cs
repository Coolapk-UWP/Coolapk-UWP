using Newtonsoft.Json;

namespace CoolapkUWP.New.Core.Models.IndexPageItemModel
{
    public class Item
    {
        public Item(string uri) 
        {
            Uri = uri;
        }
        public Item(Entity entity)
        {
            Title = entity.title;
            Uri = entity.url;
            if (Title == "关注" && !string.IsNullOrEmpty(entity.extraData))
            {
                ExtraData = JsonConvert.DeserializeObject<FollowPageExtraData>(entity.extraData, new JsonSerializerSettings { Error = (__, ee) => ee.ErrorContext.Handled = true });
            }
            if (entity.entities.Length > 0)
            {
                Entities = new Item[entity.entities.Length];
                for (int i = 0; i < entity.entities.Length; i++)
                {
                    Entities[i] = new Item(entity.entities[i]);
                }
            }
        }

        public Item(Entity1 entity)
        {
            Title = entity.title;
            Uri = entity.url;
            IsInGroup = true;
        }

        public string Title { get; private set; }
        public string Uri { get; private set; }
        public Item[] Entities { get; private set; }
        public FollowPageExtraData ExtraData { get; private set; }
        public bool IsInGroup { get; private set; }
    }

    public class Rootobject
    {
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public string entityType { get; set; }
        public string entityTemplate { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public Entity[] entities { get; set; }
        public int entityId { get; set; }
        public int entityFixed { get; set; }
        public string pic { get; set; }
        public int lastupdate { get; set; }
        public string extraData { get; set; }
    }

    public class Entity
    {
        public string entityType { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public object pic { get; set; }
        public int id { get; set; }
        public string page_name { get; set; }
        public string logo { get; set; }
        public string banner { get; set; }
        public string description { get; set; }
        public string content { get; set; }
        public string page_extras { get; set; }
        public int status { get; set; }
        public int page_type { get; set; }
        public int order { get; set; }
        public int is_head_card { get; set; }
        public int uid { get; set; }
        public string username { get; set; }
        public int hitnum { get; set; }
        public int dateline { get; set; }
        public int lastupdate { get; set; }
        public int entityId { get; set; }
        public Entity1[] entities { get; set; }
        public string extraData { get; set; }
        public int page_visibility { get; set; }
        public int page_fixed { get; set; }
    }

    public class Entity1
    {
        public string entityType { get; set; }
        public string entityId { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string pic { get; set; }
        public string page_name { get; set; }
    }

    public class FollowPageExtraData
    {
        public string saveSubTabSelectedState { get; set; }
    }
}