using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.Collections.Generic;
using ConsoleTables;

using VkNet;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient botClient;
        static bool isBotOn = Config.IsBotOn;
        static readonly string tgBotToken = Config.TgBotToken;
        static readonly string tgRootUser = Config.TgRootUser; // Имя пользователя в tg, которому будет доступен бот.

        static readonly TimeSpan botSleepTime = Config.BotSleepTime; // Время сна в минутах.

        public static SqliteHandler sqliteHandler;
        static async Task Main()
        {
            botClient = new TelegramBotClient(tgBotToken);

            sqliteHandler = new SqliteHandler();

            botClient.OnMessage += BotOnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Bot started!");

            while (isBotOn)
            {
                List<SqliteRelationTable> relationTable = SqliteHandler.GetFullSqliteTable();

                // Если бот не содержит групп для обработки, то засыпает на заданное время.
                if (relationTable.Count == 0)
                {
                    await Task.Delay(botSleepTime);
                    return;
                }

                // Формируем список групп vk и соответвующих им телеграм каналов для
                // последующей рассылки постов.
                List<VkToTgRelation> VkToTgRelationTable = new List<VkToTgRelation>();
                VkToTgRelationTable.Add(new VkToTgRelation(relationTable[0].VkGroupName, relationTable[0].TgChannelId, relationTable[0].LastPostId));
                for (int i = 1; i < relationTable.Count; i++)
                {
                    // Если текущей vk группы нет, добавить в список.
                    if(relationTable[i].VkGroupName != VkToTgRelationTable.Last().VkGroupName)
                    {
                        VkToTgRelationTable.Add(new VkToTgRelation(relationTable[i].VkGroupName, relationTable[i].TgChannelId, relationTable[i].LastPostId));
                    }
                    // Иначе добавить еще один канал для рассылки к этой vk группе.
                    else
                    {
                        VkToTgRelationTable.Last().AddTgChannelId(relationTable[i].TgChannelId);
                    }
                }

                // Делаем рассылку из всех групп vk, если есть новые посты.
                foreach (var item in VkToTgRelationTable)
                {
                    await CheckNewPosts(item);
                }

                // Спим и ждем новых постов.
                Thread.Sleep(botSleepTime);
            }

            botClient.StopReceiving();
        }

        static async Task SendPostToTg(ParsedPost post, int tgChannelId)
        {
            string botText = post.Text != null ? post.Text + "\n": ""; // Инициализируем с текстом, либо пустую строку.

            // 1. Отправляем твиттерский пост. 
            // Телега сама создаст хорошее превью.
            if (post.TwitterPostUrl != null)
            {
                botText += $"<a href=\"{post.TwitterPostUrl}\">Twitter orignal post</a>\n";
                botText += $"<a href=\"{post.VkPostUrl}\">Vk Post</a>";

                await botClient.SendTextMessageAsync(
                        "-100" + tgChannelId,
                        text: botText,
                        parseMode: ParseMode.Html
                        );
            }

            // 2. Отправляем только текст.
            else if (post.VkImagesUrl.Count == 0)
            {
                if (post.YouTubeUrl.Count > 0 )
                    for (int i = 0; i < post.YouTubeUrl.Count; i++)
                    {
                        botText += $"<a href=\"{post.YouTubeUrl[i]}\">YouTube Video {i + 1}</a>\n";
                    }
                if (post.OtherSites.Count > 0)
                    foreach (string siteUrl in post.OtherSites)
                    {
                        botText += $"{siteUrl}\n";
                    }
                botText += $"<a href=\"{post.VkPostUrl}\">Vk Post</a>";

                await botClient.SendTextMessageAsync(
                       "-100" + tgChannelId,
                       text: botText,
                       parseMode: ParseMode.Html
                       );
            }

            // 3. Отправляем картинки.
            else
            {
                if (post.VkImagesUrl.Count > 0)
                {
                    List<InputMediaPhoto> mediaPhoto = new List<InputMediaPhoto>();

                    // Загружаем все картинки из поста.
                    foreach (string img in post.VkImagesUrl)
                    {
                        mediaPhoto.Add(new InputMediaPhoto(img));
                    }

                    if (post.YouTubeUrl.Count > 0)
                        for (int i = 0; i < post.YouTubeUrl.Count; i++)
                        {
                            botText += $"<a href=\"{post.YouTubeUrl[i]}\">YouTube Video {i + 1}</a>\n";
                        }
                    if (post.OtherSites.Count > 0)
                        foreach (string siteUrl in post.OtherSites)
                        {
                            botText += $"{siteUrl}\n";
                        }
                    botText += $"<a href=\"{post.VkPostUrl}\">Vk Post</a>";

                    mediaPhoto.First().Caption += botText;
                    //TODO send group photo + links: Original post link, Attachment link 1 (2, 3, ... n)
                    
                    mediaPhoto.First().ParseMode = ParseMode.Html;
                    await botClient.SendMediaGroupAsync(
                            chatId: "-100" + tgChannelId,
                            inputMedia: mediaPhoto
                            );
                }
            }
            await Task.Delay(10);
        }

        // Проверяем новые посты в одной из ВК групп и отпрвляем их по tg каналам.
        static async Task CheckNewPosts(VkToTgRelation vkToTgRelationItem)
        {
            int lastId = vkToTgRelationItem.LastPostId;
            string vkGroupName = vkToTgRelationItem.VkGroupName;

            // Пробуем получить новые посты и отправить их.
            try
            {
                List<ParsedPost> parsedPosts = await VkParser.GetNewPosts(vkGroupName, lastId);

                if (parsedPosts.Count > 0)
                {
                    foreach (int tgChannelId in vkToTgRelationItem.TgChannels)
                    {
                        foreach (var post in parsedPosts)
                        {
                            await SendPostToTg(post, tgChannelId);

                            SqliteHandler.UpdateLastId(vkGroupName, post.Id); // Записываем новый last id в бд.
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static string GetChannelName(int tgChannelId)
        {
            string channelName = null;
            try
            {
                channelName = botClient.GetChatAsync(long.Parse("-100" + tgChannelId)).Result.Title;
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }

            return channelName;
        }

        static async void BotOnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text || message.From.Username != tgRootUser)
                return;

            switch (message.Text.Split(' ').First())
            {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text:
                        "You send me Vk public name and Telegram channel id. " +
                        "You should add bot as administrator to your channel before start!\n" +
                        "For example: /add meme_ntos 1234567890");
                    break;

                case "/add":
                    {
                        string[] splittedMessage = message.Text.Split(' ');
                        if (splittedMessage.Length != 3) return;

                        string vkGroupName = splittedMessage[1];
                        int tgChannelId;
                        try
                        {
                            tgChannelId = int.Parse(splittedMessage[2]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                text: "Telegram channel id should contain only numbers!");
                            return;
                        }

                        // Проверка на существование tg канала.
                        if (GetChannelName(tgChannelId) == null)
                        {
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                text: "Telegram channel id doesn't exit or bot hasn't been added to channel as administrator!");
                            return;
                        }

                        SqliteHandler.AddVkToTgRelation(vkGroupName, tgChannelId);

                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                text: "Relation list updated.");

                        break;
                    }

                case "/remove":
                    {
                        string[] splittedMessage = message.Text.Split(' ');
                        if (splittedMessage.Length != 3) return;

                        string vkGroupName = splittedMessage[1];
                        int tgChannelId;
                        try
                        {
                            tgChannelId = int.Parse(splittedMessage[2]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Telegram channel id should contain only numbers!");
                            return;
                        }

                        SqliteHandler.RemoveVkToTgRelation(vkGroupName, tgChannelId);

                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                text: "Relation list updated.");

                        break;
                    }

                case "/listpc":
                    {
                        List<SqliteRelationTable> relationTable = SqliteHandler.GetFullSqliteTable();
                        if (relationTable.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Bot doesn't have any Vk, Telegram records!");
                            return;
                        }

                        var table = new ConsoleTable("Vk", "TgChannel", "TgId", "LastPost");
                        foreach (var item in relationTable)
                        {
                            string channelName = GetChannelName(item.TgChannelId);
                            if (channelName == null)
                                channelName = "Channel doesn't exist";

                            table.AddRow(item.VkGroupName, channelName, item.TgChannelId, item.LastPostId);
                        }
                        string messageText = table.ToMarkDownString();
                        messageText = "```\r" + messageText + "\r```";
                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: messageText, parseMode: ParseMode.MarkdownV2);
                        break;
                    }

                case "/list":
                    {
                        List<SqliteRelationTable> relationTable = SqliteHandler.GetFullSqliteTable();
                        if (relationTable.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Bot doesn't have any Vk, Telegram records!");
                            return;
                        }

                        string messageText = "";
                        int i = 1;
                        foreach (var item in relationTable)
                        {
                            string channelName = GetChannelName(item.TgChannelId);
                            if (channelName == null)
                                channelName = "Channel doesn't exist";

                            messageText +=
                                $"{i}:\n" +
                                $"Vk Group: {item.VkGroupName}\n" +
                                $"Tg Channel Name: {channelName}\n" +
                                $"Tg Channel Id: {item.TgChannelId}\n" +
                                $"Vk Last Post Id: {item.LastPostId}\n\n";
                            i++;
                        }

                        messageText = "```\r" + messageText + "\r```";
                        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: messageText, parseMode: ParseMode.MarkdownV2);
                        break;
                    }

                default:
                    break;
            }
        }

    }
}
