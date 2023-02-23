using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KITAB.GraphQL.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string tokenEndpoint = "https://999.999.999.999:9999/api/token";
            string clientId = "???????????????????????????????????????????????????????";
            string clientSecret = "?????????????????????????";
            string graphqlEndpoint = "https://999.999.999.999:9999/api/graphql";
            string query = "{ fetchAllExtensions { extension { user { id extension name sipname } } } }";
            string token = await GetAccessToken(tokenEndpoint, clientId, clientSecret);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    StringContent content = new StringContent(JsonConvert.SerializeObject(new { query }), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(graphqlEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        // Faz a deserialização da seguinte estrutura JSON
                        // { "data": { "fetchAllExtensions": { "extension": [ { "user": { "id": "abc123", "extension": "9001", "name": "FULANO BELTRANO", "sipname": "3090010001" } } ] } } }
                        ExtensionsResponse result = JsonConvert.DeserializeObject<ExtensionsResponse>(json);

                        foreach (Extension item in result.data.fetchAllExtensions.extension)
                        {
                            Console.WriteLine(item.user.extension);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ERRO: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO: {ex.Message}");
                }
            }
        }

        private static async Task<string> GetAccessToken(string tokenEndpoint, string clientId, string clientSecret)
        {
            using (var client = new HttpClient())
            {
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                });

                HttpResponseMessage response = await client.PostAsync(tokenEndpoint, formContent);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject<dynamic>(json);

                    return result.access_token;
                }
                else
                {
                    throw new Exception("Ocorreu um ERRO ao pegar o TOKEN !");
                }
            }
        }

        public class ExtensionsResponse
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public FetchAllExtensions fetchAllExtensions { get; set; }
        }

        public class FetchAllExtensions
        {
            public List<Extension> extension { get; set; }
        }

        public class Extension
        {
            public User user { get; set; }
        }

        public class User
        {
            public string id { get; set; }
            public string extension { get; set; }
            public string name { get; set; }
            public string sipname { get; set; }
        }
    }
}
