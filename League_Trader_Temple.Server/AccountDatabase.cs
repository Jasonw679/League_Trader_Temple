using Npgsql;

namespace League_Trader_Temple.Server
{
    public class AccountDatabase(NpgsqlDataSource dataSource)
    {
        private readonly NpgsqlDataSource dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

        public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
            CREATE TABLE IF NOT EXISTS account(
                id text PRIMARY KEY,
                name text NOT NULL,
                username text NOT NULL UNIQUE,
                email text NOT NULL,
                password_hash text NOT NULL,
                created_at timestamp with time zone NOT NULL DEFAULT now()
            );";

            await using var command = dataSource.CreateCommand(sql);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
