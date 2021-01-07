using System.Collections.Generic;

namespace TelegramBot
{
    // Класс, который описывает связь из какой группы vk в какие tg каналы нужно отправить новые посты.
    class VkToTgRelation
    {
        public string VkGroupName { get; set; }
        public List<int> TgChannels { get; set; }
        public int LastPostId { get; set; }
        public VkToTgRelation(string vkGroupName, int tgChannelId, int lastPostId)
        {
            VkGroupName = vkGroupName;
            TgChannels = new List<int>();
            TgChannels.Add(tgChannelId);
            LastPostId = lastPostId;
        }

        public void AddTgChannelId(int tgChannelId)
        {
            TgChannels.Add(tgChannelId);
        }
    }
}
