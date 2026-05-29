using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace League_Trader_Temple.Server;

public class CardDatabase(NpgsqlDataSource dataSource)
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

            CREATE TABLE IF NOT EXISTS riftbound_card_visits (
                card_id text NOT NULL REFERENCES riftbound_cards (id) ON DELETE CASCADE,
                user_id text NOT NULL,
                visit_count integer NOT NULL DEFAULT 0,
                first_visited_at timestamp with time zone NOT NULL DEFAULT now(),
                last_visited_at timestamp with time zone NOT NULL DEFAULT now(),
                PRIMARY KEY (card_id, user_id)
            );

            CREATE INDEX IF NOT EXISTS ix_riftbound_card_visits_card_id
                ON riftbound_card_visits (card_id);

            CREATE INDEX IF NOT EXISTS ix_riftbound_card_visits_visit_count
                ON riftbound_card_visits (visit_count DESC);
            """;

        await using var command = dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertCardsAsync(IEnumerable<RiftboundCard> cards, CancellationToken cancellationToken = default)
    {
        var cardList = cards.ToArray();

        const string sql = """
            INSERT INTO riftbound_cards (
                id,
                name,
                public_code,
                collector_number,
                set_id,
                card,
                synced_at
            )
            VALUES (
                @id,
                @name,
                @public_code,
                @collector_number,
                @set_id,
                @card,
                now()
            )
            ON CONFLICT (id) DO UPDATE SET
                name = EXCLUDED.name,
                public_code = EXCLUDED.public_code,
                collector_number = EXCLUDED.collector_number,
                set_id = EXCLUDED.set_id,
                card = EXCLUDED.card,
                synced_at = now();
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        foreach (var card in cardList)
        {
            await using var command = new NpgsqlCommand(sql, connection, transaction);
            var publicCode = GetPublicCode(card);

            command.Parameters.AddWithValue("id", card.Id);
            command.Parameters.AddWithValue("name", card.Name);
            command.Parameters.AddWithValue("public_code", publicCode);
            command.Parameters.AddWithValue("collector_number", card.CollectorNumber);
            command.Parameters.AddWithValue("set_id", card.SetId);
            command.Parameters.Add("card", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(card, CardJsonOptions);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await DeleteCardsNotInSyncAsync(connection, transaction, cardList, cancellationToken);

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
            filters.Add("c.id = @id");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add("""
                (
                    c.name ILIKE @search
                    OR c.public_code ILIKE @search
                    OR c.card ->> 'type' ILIKE @search
                    OR c.card ->> 'rarity' ILIKE @search
                    OR c.card ->> 'faction' ILIKE @search
                )
                """);
        }

        if (!string.IsNullOrWhiteSpace(setId))
        {
            filters.Add("lower(c.set_id) = lower(@set_id)");
        }

        var whereClause = filters.Count == 0 ? "" : $"WHERE {string.Join(" AND ", filters)}";
        var countSql = $"SELECT count(*) FROM riftbound_cards c {whereClause};";
        var selectSql = $"""
            SELECT (c.card || jsonb_build_object('visitCount', COALESCE(visits.visit_count, 0)))::text
            FROM riftbound_cards c
            LEFT JOIN (
                SELECT card_id, SUM(visit_count) AS visit_count
                FROM riftbound_card_visits
                GROUP BY card_id
            ) visits ON visits.card_id = c.id
            {whereClause}
            ORDER BY {sortColumn} {direction}, c.collector_number ASC, c.name ASC
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

    public async Task<bool> RecordVisitAsync(
        string cardId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO riftbound_card_visits (
                card_id,
                user_id,
                visit_count,
                first_visited_at,
                last_visited_at
            )
            SELECT
                id,
                @user_id,
                1,
                now(),
                now()
            FROM riftbound_cards
            WHERE id = @card_id
            ON CONFLICT (card_id, user_id) DO UPDATE SET
                visit_count = riftbound_card_visits.visit_count + 1,
                last_visited_at = now();
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("card_id", cardId);
        command.Parameters.AddWithValue("user_id", userId);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
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

    private static async Task DeleteCardsNotInSyncAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        RiftboundCard[] cards,
        CancellationToken cancellationToken)
    {
        if (cards.Length == 0)
        {
            return;
        }

        await using var command = new NpgsqlCommand(
            "DELETE FROM riftbound_cards WHERE NOT (id = ANY(@ids));",
            connection,
            transaction);

        command.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Text).Value =
            cards.Select(card => card.Id).ToArray();
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string GetSortColumn(string sort)
    {
        return sort switch
        {
            "name" => "c.name",
            "public_code" => "c.public_code",
            "set_id" => "c.set_id",
            "collector_number" => "c.collector_number",
            "rarity" => "c.card ->> 'rarity'",
            "faction" => "c.card ->> 'faction'",
            "type" => "c.card ->> 'type'",
            "visits" => "COALESCE(visits.visit_count, 0)",
            _ => "c.collector_number"
        };
    }

    private static string GetPublicCode(RiftboundCard card)
    {
        return card.Id.ToUpperInvariant();
    }
}
