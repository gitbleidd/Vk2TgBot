using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Net.Http;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.Collections.Generic;
using ConsoleTables;

namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient botClient;
        static bool isBotOn = true;
        static readonly string tgBotToken = "";
        static readonly string tgRootUser = "gitbleidd"; // Имя пользователя в tg, которому будет доступен бот.

        static readonly string vkAccessToken = "";
        static readonly string vkApiVersion = "5.21";
        static readonly string postsCountAtOne = "15";

        static readonly TimeSpan botSleepTime = TimeSpan.FromMinutes(5.0); // Время сна в минутах.

        static SqliteHandler sqliteHandler;
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

                if (relationTable.Count == 0)
                {
                    Thread.Sleep(botSleepTime);
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

        static async Task<string> GetVkJsonAsync(string vkGroupName)
        {
            string vkWallURL = $"https://api.vk.com/method/wall.get?v={vkApiVersion}" + 
                $"&domain={vkGroupName}&count={postsCountAtOne}&filter=owner&access_token={vkAccessToken}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(vkWallURL);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        static void SendPostsToTg(VkJson vkJson, string vkGroupName, ref int lastId, List<int> tgChannels)
        {
            var posts = vkJson.response.items.ToList();
            posts.Reverse();

            // Удаляем закрепленный пост.
            if (posts.Last().is_pinned == 1)
            {
                posts.RemoveAt(posts.Count - 1);
            }

            // Иницилазируем lastId.
            if(lastId == 0)
            {
                lastId = posts.Last().id;
                return;
            }

            // Если новых постов нет, выходим.
            int n = posts.Count;
            if (posts[n - 1].id <= lastId)
            {
                return;
            }

            // Отправляем новые посты во все tg каналы из списка.
            for (int i = 0; i < n; i++)
            {
                if (posts[i].id > lastId)
                {
                    int vkGroupId = Math.Abs(posts[i].from_id); // id с минусом, поэтому берем модуль. (id на самом деле string в json)
                    string vkWallUrl = $"https://vk.com/{vkGroupName}?w=wall-{vkGroupId}_{posts[i].id}";

                    foreach (var tgChannelId in tgChannels)
                    {
                        int attachments = 0;
                        if (posts[i].attachments != null)
                            attachments = posts[i].attachments.Length;

                        botClient.SendTextMessageAsync(
                            chatId: long.Parse("-100" + tgChannelId),
                            text: 
                            $"<a href=\"{vkWallUrl}\">VK Link</a>\n" +
                            $"Attachments: {attachments} | Comments: {posts[i].comments.count}",
                            parseMode: ParseMode.Html);
                    }
                    lastId = posts[i].id;
                    Thread.Sleep(50);
                }
            }
        }

        static async Task CheckNewPosts(VkToTgRelation vkToTgRelationItem)
        {
            int lastId = vkToTgRelationItem.LastPostId;
            string vkGroupName = vkToTgRelationItem.VkGroupName;

            // Пробуем получить json от vk и распарсить его.
            try
            {
                string responseBody = await GetVkJsonAsync(vkGroupName); // Получаем json ответ.
                
                // Парсим json файл и делаем рассылку.
                if (responseBody != null)
                {
                    VkJson vkJson = JsonConvert.DeserializeObject<VkJson>(responseBody);
                    SendPostsToTg(vkJson, vkGroupName, ref lastId, vkToTgRelationItem.TgChannels);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Записываем новый last id в бд.
            SqliteHandler.UpdateLastId(vkGroupName, lastId);
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
