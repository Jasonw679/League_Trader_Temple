using System.Text.Json;

namespace League_Trader_Temple.Server;

public class RiftcodexCardSyncService(
    CardDatabase cardDatabase,
    ILogger<RiftcodexCardSyncService> logger) : IHostedService
{

    private const string CardFilePath = "cards.json";

    private static readonly JsonSerializerOptions RiftcodexJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly CardDatabase cardDatabase = cardDatabase;
    private readonly ILogger<RiftcodexCardSyncService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await cardDatabase.EnsureCreatedAsync(cancellationToken);

        if (!File.Exists(CardFilePath))
        {
            throw new FileNotFoundException("cards.json not found", CardFilePath);
        }
        var json = await File.ReadAllTextAsync(CardFilePath, cancellationToken);

        var cardPage = JsonSerializer.Deserialize<RiftboundCardPage>(
            json,
            RiftcodexJsonOptions
        ) ?? throw new InvalidOperationException("cards.json deserialization returned null.");

        await cardDatabase.UpsertCardsAsync(cardPage.Items, cancellationToken);

        logger.LogInformation(
            "Synced {CardCount} Riftbound cards from local cards.json.",
            cardPage.Items.Length
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
