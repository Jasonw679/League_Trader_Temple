using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace League_Trader_Temple.Server;

public sealed class CardDatabase(NpgsqlDataSource dataSource)
{
    private static readonly JsonSerializerOptions CardJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly NpgsqlDataSource dataSource = dataSource;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS riftbound_cards (
                id text PRIMARY KEY,
                name text NOT NULL,
                riftbound_id text NOT NULL,
                public_code text NOT NULL,
                collector_number integer NOT NULL,
                set_id text NOT NULL,
                card jsonb NOT NULL,
                synced_at timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE INDEX IF NOT EXISTS ix_riftbound_cards_name
                ON riftbound_cards (lower(name));

            CREATE INDEX IF NOT EXISTS ix_riftbound_cards_public_code
                ON riftbound_cards (lower(public_code));

            CREATE INDEX IF NOT EXISTS ix_riftbound_cards_set_id
                ON riftbound_cards (set_id);

            CREATE INDEX IF NOT EXISTS ix_riftbound_cards_collector_number
                ON riftbound_cards (collector_number);
            """;

        await using var command = dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertCardsAsync(IEnumerable<RiftboundCard> cards, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO riftbound_cards (
                id,
                name,
                riftbound_id,
                public_code,
                collector_number,
                set_id,
                card,
                synced_at
            )
            VALUES (
                @id,
                @name,
                @riftbound_id,
                @public_code,
                @collector_number,
                @set_id,
                @card,
                now()
            )
            ON CONFLICT (id) DO UPDATE SET
                name = EXCLUDED.name,
                riftbound_id = EXCLUDED.riftbound_id,
                public_code = EXCLUDED.public_code,
                collector_number = EXCLUDED.collector_number,
                set_id = EXCLUDED.set_id,
                card = EXCLUDED.card,
                synced_at = now();
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var card in cards)
        {
            await using var command = new NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("id", card.Id);
            command.Parameters.AddWithValue("name", card.Name);
            command.Parameters.AddWithValue("riftbound_id", card.RiftboundId);
            command.Parameters.AddWithValue("public_code", card.PublicCode);
            command.Parameters.AddWithValue("collector_number", card.CollectorNumber);
            command.Parameters.AddWithValue("set_id", card.Set.SetId);
            command.Parameters.Add("card", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(card, CardJsonOptions);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<RiftboundCardPage> GetCardsAsync(
        string? search,
        string? id,
        string? setId,
        int page,
        int size,
        string sort,
        int dir,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<string>();
        var sortColumn = GetSortColumn(sort);
        var direction = dir == -1 ? "DESC" : "ASC";
        var offset = (page - 1) * size;

        if (!string.IsNullOrWhiteSpace(id))
        {
            filters.Add("id = @id");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add("""
                (
                    name ILIKE @search
                    OR public_code ILIKE @search
                    OR riftbound_id ILIKE @search
                    OR card -> 'text' ->> 'plain' ILIKE @search
                )
                """);
        }

        if (!string.IsNullOrWhiteSpace(setId))
        {
            filters.Add("set_id = @set_id");
        }

        var whereClause = filters.Count == 0 ? "" : $"WHERE {string.Join(" AND ", filters)}";
        var countSql = $"SELECT count(*) FROM riftbound_cards {whereClause};";
        var selectSql = $"""
            SELECT card::text
            FROM riftbound_cards
            {whereClause}
            ORDER BY {sortColumn} {direction}, collector_number ASC, name ASC
            OFFSET @offset
            LIMIT @limit;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var total = await CountCardsAsync(connection, countSql, search, id, setId, cancellationToken);
        var items = await SelectCardsAsync(connection, selectSql, search, id, setId, offset, size, cancellationToken);

        return new RiftboundCardPage
        {
            Items = [.. items],
            Total = total,
            Page = page,
            Size = size,
            Pages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
        };
    }

    private static async Task<int> CountCardsAsync(
        NpgsqlConnection connection,
        string sql,
        string? search,
        string? id,
        string? setId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        AddFilterParameters(command, search, id, setId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static async Task<List<RiftboundCard>> SelectCardsAsync(
        NpgsqlConnection connection,
        string sql,
        string? search,
        string? id,
        string? setId,
        int offset,
        int size,
        CancellationToken cancellationToken)
    {
        var cards = new List<RiftboundCard>();

        await using var command = new NpgsqlCommand(sql, connection);
        AddFilterParameters(command, search, id, setId);
        command.Parameters.AddWithValue("offset", offset);
        command.Parameters.AddWithValue("limit", size);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var json = reader.GetString(0);
            var card = JsonSerializer.Deserialize<RiftboundCard>(json, CardJsonOptions);

            if (card is not null)
            {
                cards.Add(card);
            }
        }

        return cards;
    }

    private static void AddFilterParameters(NpgsqlCommand command, string? search, string? id, string? setId)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            command.Parameters.AddWithValue("id", id.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            command.Parameters.AddWithValue("search", $"%{search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(setId))
        {
            command.Parameters.AddWithValue("set_id", setId.Trim());
        }
    }

    private static string GetSortColumn(string sort)
    {
        return sort switch
        {
            "name" => "name",
            "public_code" => "public_code",
            "riftbound_id" => "riftbound_id",
            "set_id" => "set_id",
            "collector_number" => "collector_number",
            _ => "collector_number"
        };
    }
}
