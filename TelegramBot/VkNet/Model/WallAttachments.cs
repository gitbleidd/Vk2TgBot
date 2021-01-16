namespace VkNet.Model
{
    public class WallAttachments
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
        public Likes likes { get; set; }
        public Reposts reposts { get; set; }
        public Views views { get; set; }
        public bool is_favorite { get; set; }
        public Donut donut { get; set; }
        public float short_text_rate { get; set; }
        public int carousel_offset { get; set; }
        public Copyright copyright { get; set; }
        public int signer_id { get; set; }
        public Copy_History[] copy_history { get; set; }
        public int edited { get; set; }
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
        public bool groups_can_post { get; set; }
    }

    public class Likes
    {
        public int count { get; set; }
        public int user_likes { get; set; }
        public int can_like { get; set; }
        public int can_publish { get; set; }
    }

    public class Reposts
    {
        public int count { get; set; }
        public int user_reposted { get; set; }
    }

    public class Views
    {
        public int count { get; set; }
    }

    public class Donut
    {
        public bool is_donut { get; set; }
    }

    public class Copyright
    {
        public string link { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public Photo photo { get; set; }
        public Link link { get; set; }
        public Video video { get; set; }
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
        public int post_id { get; set; }
        public Size[] sizes { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
    }

    public class Size
    {
        public int height { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public int width { get; set; }
    }

    public class Link
    {
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string target { get; set; }
        public bool is_favorite { get; set; }
    }

    public class Video
    {
        public string access_key { get; set; }
        public int can_comment { get; set; }
        public int can_like { get; set; }
        public int can_repost { get; set; }
        public int can_subscribe { get; set; }
        public int can_add_to_faves { get; set; }
        public int can_add { get; set; }
        public int comments { get; set; }
        public int date { get; set; }
        public string description { get; set; }
        public int duration { get; set; }
        public Image[] image { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public bool is_favorite { get; set; }
        public string track_code { get; set; }
        public string type { get; set; }
        public int views { get; set; }
        public int local_views { get; set; }
        public string platform { get; set; }
        public First_Frame[] first_frame { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int repeat { get; set; }
    }

    public class Image
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int with_padding { get; set; }
    }

    public class First_Frame
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
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
        public Preview preview { get; set; }
        public string access_key { get; set; }
    }

    public class Preview
    {
        public Photo1 photo { get; set; }
        public Video1 video { get; set; }
    }

    public class Photo1
    {
        public Size1[] sizes { get; set; }
    }

    public class Size1
    {
        public string src { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string type { get; set; }
    }

    public class Video1
    {
        public string src { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int file_size { get; set; }
    }

    public class Copy_History
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public int from_id { get; set; }
        public int date { get; set; }
        public string post_type { get; set; }
        public string text { get; set; }
        public Attachment1[] attachments { get; set; }
        public Post_Source1 post_source { get; set; }
        public int signer_id { get; set; }
    }

    public class Post_Source1
    {
        public string type { get; set; }
    }

    public class Attachment1
    {
        public string type { get; set; }
        public Photo2 photo { get; set; }
    }

    public class Photo2
    {
        public int album_id { get; set; }
        public int date { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public bool has_tags { get; set; }
        public string access_key { get; set; }
        public int post_id { get; set; }
        public Size2[] sizes { get; set; }
        public string text { get; set; }
        public int user_id { get; set; }
    }

    public class Size2
    {
        public int height { get; set; }
        public string url { get; set; }
        public string type { get; set; }
        public int width { get; set; }
    }

}
