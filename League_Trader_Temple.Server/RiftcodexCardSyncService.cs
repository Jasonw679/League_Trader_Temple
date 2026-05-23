using System.Text.Json;

namespace League_Trader_Temple.Server;

public sealed class RiftcodexCardSyncService(
    CardDatabase cardDatabase,
    IHttpClientFactory httpClientFactory,
    ILogger<RiftcodexCardSyncService> logger) : IHostedService
{
    private const int PageSize = 100;

    private static readonly JsonSerializerOptions RiftcodexJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly CardDatabase cardDatabase = cardDatabase;
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
    private readonly ILogger<RiftcodexCardSyncService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await cardDatabase.EnsureCreatedAsync(cancellationToken);

        var client = httpClientFactory.CreateClient("Riftcodex");
        var firstPage = await GetPageAsync(client, 1, cancellationToken);
        await cardDatabase.UpsertCardsAsync(firstPage.Items, cancellationToken);

        var syncedCards = firstPage.Items.Length;
        for (var page = 2; page <= firstPage.Pages; page++)
        {
            var cardPage = await GetPageAsync(client, page, cancellationToken);
            await cardDatabase.UpsertCardsAsync(cardPage.Items, cancellationToken);
            syncedCards += cardPage.Items.Length;
        }

        logger.LogInformation("Synced {CardCount} Riftbound cards from Riftcodex.", syncedCards);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task<RiftboundCardPage> GetPageAsync(
        HttpClient client,
        int page,
        CancellationToken cancellationToken)
    {
        var path = $"cards?page={page}&size={PageSize}&sort=collector_number&dir=1";
        var cardPage = await client.GetFromJsonAsync<RiftboundCardPage>(
            path,
            RiftcodexJsonOptions,
            cancellationToken);

        return cardPage ?? throw new InvalidOperationException("Riftcodex returned an empty card page.");
    }
}
