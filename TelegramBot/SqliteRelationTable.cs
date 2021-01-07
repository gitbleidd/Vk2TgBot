namespace TelegramBot
{
    // Класс, который описывает таблицу relation в бд.
    class SqliteRelationTable
    {
        public string VkGroupName { get; set; }
        public int TgChannelId { get; set; }
        public int LastPostId { get; set; }
        public SqliteRelationTable(string vkGroupName, int tgChannelId, int lastPostId)
        {
            VkGroupName = vkGroupName;
            TgChannelId = tgChannelId;
            LastPostId = lastPostId;
        }
    }
}
