using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VkNet.Model;

namespace VkNet
{
    class VkClient
    {
        public static string VkAccessToken { get; } = TelegramBot.Config.VkAccessToken;

        public static string VkApiVersion { get; } = TelegramBot.Config.VkApiVersion;

        public static string PostsCountAtOne { get; } = TelegramBot.Config.PostsCountAtOne;

        public static async Task<WallAttachments> GetVkWallResponseAsync(string vkGroupName)
        {
            string responseBody = await GetVkWallHttpResponseAsync(vkGroupName);

            if (responseBody != null)
                return Deserialize<WallAttachments>(responseBody);
            else
                return null;
        }

        //TODO написать класс для VkVideoResponse
        /*
        public static async Task<VkWallResponse> GetVkVideoResponseAsync(string vkGroupName, Video video)
        {
            string responseBody = await GetVkVideoHttpResponseAsync(vkGroupName, video.owner_id, video.id, video.access_key);
            
            if (responseBody != null)
                return Deserialize<VkWallResponse>(responseBody);
            else
                return null;
           
            return null;
        }
         */
        private static async Task<string> GetVkWallHttpResponseAsync(string vkGroupName)
        {
            string getVkWallHttpQuery = $"https://api.vk.com/method/wall.get?v={VkApiVersion}" +
                $"&domain={vkGroupName}&count={PostsCountAtOne}&filter=owner&access_token={VkAccessToken}";

            try
            {
                return await HttpGet(getVkWallHttpQuery);
            }

            catch(HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static async Task<string> GetVkVideoHttpResponseAsync(string vkGroupName, int ownerId, int id, string accessKey)
        {
            string getVkVideoHttpQuery = $"https://api.vk.com/method/video.get?v={VkApiVersion}&" +
                $"videos={ownerId}_{id}_{accessKey}&filter=owner&access_token={VkAccessToken}";

            try
            {
                return await HttpGet(getVkVideoHttpQuery);
            }

            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static private async Task<string> HttpGet(string query)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(query);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        static private T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
    }
}
