using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;


namespace SarasKitchenApp
{
    public class RecipeGeneratorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly ILogger<RecipeGeneratorService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _recipePrompt;
        private readonly string _model;
        private readonly string _url;
        private readonly int _maxTokens;


        public RecipeGeneratorService(IHttpClientFactory httpClientFactory, 
                                      ILogger<RecipeGeneratorService> logger,
                                      IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _apiKey = _configuration["OpenAI:ApiKey"]
                ?? throw new ArgumentNullException(nameof(_apiKey), "OpenAI:ApiKey is missing from configuration.");
            _recipePrompt = _configuration["OpenAI:RecipePrompt"] 
                ?? throw new ArgumentNullException(nameof(_recipePrompt), "RecipePrompt is missing from configuration.");
            _model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
            _url = _configuration["OpenAI:Url"] ?? "https://api.openai.com/v1/chat/completions";
            if (!int.TryParse(_configuration["OpenAI:MaxTokens"], out _maxTokens) || _maxTokens <= 0)
            {
                _maxTokens = 1000; 
            }
        }

        public async Task<RecipeFromAI?> GetRecipeJsonAsync(string userPrompt)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
            
            var body = new
            {
                model = _model,
                stream = false,
                max_tokens = _maxTokens,
                messages = new[]
                {
                    new { role = "system", content = _recipePrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            try
            {
                var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_url, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var parsed = JsonDocument.Parse(responseString);
                var json = parsed.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

            
                return JsonSerializer.Deserialize<RecipeFromAI>(json ?? string.Empty);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to retrieve recipe.");
                return null;
            }
        }

        public class RecipeFromAI
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("ingredients")]
            public List<string> Ingredients { get; set; } = [];

            [JsonPropertyName("instructions")]
            public List<string> Instructions { get; set; } = [];
        }
    }
}
