namespace TelegramBot
{
    public class VkJson
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public int count { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public int from_id { get; set; }
        public int owner_id { get; set; }
        public int date { get; set; }
        public int marked_as_ads { get; set; }
        public int is_pinned { get; set; }
        public string post_type { get; set; }
        public string text { get; set; }

        public Attachment[] attachments { get; set; }
        public Post_Source post_source { get; set; }
        public Comments comments { get; set; }
        public Copy_History[] copy_history { get; set; }
    }

    public class Post_Source
    {
        public string type { get; set; }
        public string platform { get; set; }
    }

    public class Comments
    {
        public int count { get; set; }
        public int can_post { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public Photo photo { get; set; }
        public Link link { get; set; }
        public Doc doc { get; set; }
    }

    public class Photo
    {
        public int album_id { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public bool has_tags { get; set; }
        public string access_key { get; set; }
        public int height { get; set; }
        public string photo_130 { get; set; }
        public string photo_604 { get; set; }
        public string photo_75 { get; set; }
        public string photo_807 { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
        public int width { get; set; }
        public string photo_1280 { get; set; }
        public int post_id { get; set; }
        public string photo_2560 { get; set; }
    }

    public class Link
    {
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string target { get; set; }
        public string button_text { get; set; }
        public string button_action { get; set; }
        public string image_src { get; set; }
        public string image_big { get; set; }
    }

    public class Doc
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public int size { get; set; }
        public string ext { get; set; }
        public int date { get; set; }
        public int type { get; set; }
        public string url { get; set; }
        public string photo_100 { get; set; }
        public string photo_130 { get; set; }
        public string access_key { get; set; }
    }

    public class Copy_History
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public int from_id { get; set; }
        public int date { get; set; }
        public string post_type { get; set; }
        public string text { get; set; }
        public Attachment2[] attachments { get; set; }
        public Post_Source2 post_source { get; set; }
    }

    public class Post_Source2
    {
        public string type { get; set; }
    }

    public class Attachment2
    {
        public string type { get; set; }
        public Photo1 photo { get; set; }
    }

    public class Photo1
    {
        public int album_id { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public bool has_tags { get; set; }
        public string access_key { get; set; }
        public int height { get; set; }
        public string photo_1280 { get; set; }
        public string photo_130 { get; set; }
        public string photo_2560 { get; set; }
        public string photo_604 { get; set; }
        public string photo_75 { get; set; }
        public string photo_807 { get; set; }
        public int post_id { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
        public int width { get; set; }
    }

}