using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatGPTdemo
{
    internal class OpenAIClient
    {
        private readonly string _key;

        private static string _uri = "https://api.openai.com/v1/chat/completions";
        public static string RoleSystem = "system";
        public static string RoleUser = "user";
        public static string RoleAssistant = "assistant";

        public OpenAIClient(string key)
        {
            _key = key;
        }

        public async Task<string> Query(IList<Tuple<string, string>> input)
        {
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _key);

                var query = new ChatQuery(input);

                var result = await client.PostAsJsonAsync(_uri, query);
                result.EnsureSuccessStatusCode();
                var response = await result.Content.ReadFromJsonAsync<ChatCompletion>();
                var msg = response?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
                return msg;
            }
        }

        private class ChatQuery
        {
            public string Model { get; } = "gpt-3.5-turbo";
            public List<Message> Messages { get; set; }

            public ChatQuery(IList<Tuple<string, string>> input)
            {
                Messages = input.Select(x => new Message(x.Item1, x.Item2)).ToList();
            }
        }

        private class Usage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        private class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }

            public Message(string role, string content)
            {
                Role = role;
                Content = content;
            }
        }

        private class Choice
        {
            public Message Message { get; set; }
            public string FinishReason { get; set; }
            public int Index { get; set; }
        }

        private class ChatCompletion
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public int Created { get; set; }
            public string Model { get; set; }
            public Usage Usage { get; set; }
            public List<Choice> Choices { get; set; }
        }
    }
}
