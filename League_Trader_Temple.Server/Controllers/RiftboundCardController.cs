using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace League_Trader_Temple.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RiftboundCardsController : ControllerBase
    {
        private static readonly JsonSerializerOptions RiftcodexJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private readonly IHttpClientFactory httpClientFactory;

        public RiftboundCardsController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetRiftboundCards")]
        public async Task<ActionResult<RiftboundCardPage>> Get(
            [FromQuery] string? search = null,
            [FromQuery] string? setId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 6,
            [FromQuery] string sort = "collector_number",
            [FromQuery] int dir = 1,
            CancellationToken cancellationToken = default)
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than zero.");
            }

            if (size is < 1 or > 100)
            {
                return BadRequest("Size must be between 1 and 100.");
            }

            if (dir is not 1 and not -1)
            {
                return BadRequest("Dir must be 1 for ascending or -1 for descending.");
            }

            var path = string.IsNullOrWhiteSpace(search) ? "cards" : "cards/search";
            var query = new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["size"] = size.ToString(),
                ["sort"] = sort,
                ["dir"] = dir.ToString()
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query["query"] = search.Trim();
            }

            if (!string.IsNullOrWhiteSpace(setId))
            {
                query["set_id"] = setId.Trim();
            }

            var requestUri = BuildUri(path, query);
            var client = httpClientFactory.CreateClient("Riftcodex");
            using var response = await client.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    "Unable to load Riftbound cards from Riftcodex right now.");
            }

            var cardPage = await response.Content.ReadFromJsonAsync<RiftboundCardPage>(
                RiftcodexJsonOptions,
                cancellationToken);

            return cardPage is null
                ? StatusCode(StatusCodes.Status502BadGateway, "Riftcodex returned an empty response.")
                : Ok(cardPage);
        }

        private static string BuildUri(string path, Dictionary<string, string?> query)
        {
            var queryString = string.Join(
                "&",
                query
                    .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                    .Select(item =>
                        $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));

            return string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
        }
    }
}
