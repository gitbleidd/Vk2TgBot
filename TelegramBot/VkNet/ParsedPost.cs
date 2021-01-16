using System.Collections.Generic;

namespace VkNet
{
    class ParsedPost
    {
        public int Id { get; set; }

        public string VkPostUrl { get; set; }

        public string Text { get; set; } = "";

        public string TwitterPostUrl { get; set; }

        public List<string> YouTubeUrl { get; set; } = new List<string>();

        public List<string> OtherSites { get; set; } = new List<string>();

        public List<string> VkImagesUrl { get; set; } = new List<string>();

        public ParsedPost(int lastId, string vkPostUrl, string text)
        {
            Id = lastId;
            VkPostUrl = vkPostUrl;
            Text = text;
        }

    }
}
