using System.Text.Json;

namespace League_Trader_Temple.Server;

public class RiftcodexCardSyncService(
    CardDatabase cardDatabase,
    ILogger<RiftcodexCardSyncService> logger,
    IHostEnvironment environment) : IHostedService
{

    private const string CardFileName = "cards.json";

    private static readonly JsonSerializerOptions RiftcodexJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly CardDatabase cardDatabase = cardDatabase;
    private readonly ILogger<RiftcodexCardSyncService> logger = logger;
    private readonly string cardFilePath = Path.Combine(environment.ContentRootPath, CardFileName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await cardDatabase.EnsureCreatedAsync(cancellationToken);

        if (!File.Exists(cardFilePath))
        {
            throw new FileNotFoundException("cards.json not found", cardFilePath);
        }
        var json = await File.ReadAllTextAsync(cardFilePath, cancellationToken);

        var cards = JsonSerializer.Deserialize<RiftboundCard[]>(
            json,
            RiftcodexJsonOptions
        ) ?? throw new InvalidOperationException("cards.json deserialization returned null.");

        await cardDatabase.UpsertCardsAsync(cards, cancellationToken);

        logger.LogInformation(
            "Synced {CardCount} Riftbound cards from local cards.json.",
            cards.Length
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
