using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SalesOrders.Services
{
    public class DogImageService
    {
        private readonly HttpClient _httpClient;
        public DogImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetRandomDogImageUrlAsync()
        {
            var response = await _httpClient.GetAsync("https://dog.ceo/api/breeds/image/random");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<DogApiResponse>(responseContent);
            return data?.Message;
        }

        private class DogApiResponse
        {
            public string Message { get; set; }
            public string Status { get; set; }
        }
    }
}
