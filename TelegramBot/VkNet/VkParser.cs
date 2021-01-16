using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model;

namespace VkNet
{
    static class VkParser
    {
        public static async Task<List<ParsedPost>> GetNewPosts(string vkGroupName, int lastId)
        {
            List<ParsedPost> parsedPosts = new List<ParsedPost>();

            // Получаем десериализованный объект стены вк и начинаем обрабатывать его.
            WallAttachments wallAttachments = await VkClient.GetVkWallResponseAsync(vkGroupName); 
            if (wallAttachments == null)
            {
                return parsedPosts;
            }

            var posts = wallAttachments.response.items.ToList();
            posts.Reverse();

            // Удаляем закрепленный пост.
            if (posts.Last().is_pinned == 1)
            {
                posts.RemoveAt(posts.Count - 1);
            }

            // Иницилазируем lastId. В итоге вернется только последний пост группы.
            if (lastId == 0)
            {
                lastId = posts[posts.Count - 2].id;
            }

            // Сверяем с последнием постом, если новых постов нет, выходим.
            if (posts[posts.Count - 1].id <= lastId)
            {
                return parsedPosts;
            }

            foreach (var post in posts)
            {
                // Если пост новый, то парсим.
                if (post.id > lastId)
                {
                    // Получаем ссылку на пост группы вк.
                    int vkGroupId = post.owner_id;
                    string vkPostUrl = $"https://vk.com/{vkGroupName}?w=wall{vkGroupId}_{post.id}";

                    parsedPosts.Add(new ParsedPost(post.id, vkPostUrl, post.text));
                    var lastPost = parsedPosts.Last();

                    // Пытаемся получить ссылку на пост из твиттера из поля copyright, либо ссылку на другой источник.
                    if (post.copyright != null)
                    {
                        // Проверяем указывает ли ссылка на пост в твиттере 
                        // (может указывать просто на пользователя).
                        if (post.copyright.name == "twitter.com" && 
                            TelegramBot.StringExtensions.Contains(post.copyright.link, "status", StringComparison.OrdinalIgnoreCase))
                        {
                            lastPost.TwitterPostUrl = post.copyright.link;
                            continue;
                        }

                        else
                        {
                            lastPost.OtherSites.Add(post.copyright.link);
                        }
                    }

                    // Здесь обрабатываем приложения к посту.
                    if (post.attachments != null)
                    {
                        foreach (var attachment in post.attachments)
                        {
                            switch (attachment.type)
                            {
                                case "photo":
                                    {
                                        // Ищем наибольшее фото.
                                        string largestPhotoUrl = attachment.photo.sizes[0].url;

                                        for (int i = 1; i < attachment.photo.sizes.Length; i++)
                                        {
                                            var currentPhoto = attachment.photo.sizes[i];
                                            var prevPhoto = attachment.photo.sizes[i - 1];

                                            if (currentPhoto.width * currentPhoto.height > prevPhoto.width * prevPhoto.height)
                                            {
                                                largestPhotoUrl = currentPhoto.url;
                                            }
                                        }
                                        lastPost.VkImagesUrl.Add(largestPhotoUrl);
                                        break;
                                    }

                                case "video":
                                    {

                                        //TODO преобразование в YouTube ссылку
                                        //TODO подумать над VK видео
                                        break;
                                    }

                                case "link":
                                    {
                                        // Пробуем найти ссылку на пост в twitter.
                                        if (TelegramBot.StringExtensions.Contains(attachment.link.url, "twitter", StringComparison.OrdinalIgnoreCase) &&
                                            TelegramBot.StringExtensions.Contains(attachment.link.url, "status", StringComparison.OrdinalIgnoreCase))
                                        {
                                            lastPost.TwitterPostUrl = attachment.link.url;
                                        }
                                        else
                                        {
                                            lastPost.OtherSites.Add(attachment.link.url);
                                        }
                                        break;
                                    }

                                /*
                                case "doc":
                                    {
                                        //TODO Парсинг поста с приложением формата doc.
                                        break;
                                    }
                                */

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            return parsedPosts;
        }
    }
}
