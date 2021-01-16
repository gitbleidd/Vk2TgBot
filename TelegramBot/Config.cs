using System;

namespace TelegramBot
{
    static class Config
    {
        public static bool IsBotOn { get; set; } = true;

        public static string TgBotToken { get; } = "";

        public static string TgRootUser { get; } = "gitbleidd"; // Имя пользователя в tg, которому будет доступен бот.

        public static string VkAccessToken { get; } = "";

        public static string VkApiVersion { get; } = "5.126";

        public static string PostsCountAtOne { get; } = "15";

        public static TimeSpan BotSleepTime { get; } = TimeSpan.FromMinutes(4.0); // Время сна в минутах.
    }
}
