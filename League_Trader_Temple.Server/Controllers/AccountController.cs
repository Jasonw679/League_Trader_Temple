using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace League_Trader_Temple.Server.Controllers
{
    [ApiController]
    [Route("Account")]
    public class AccountController(NpgsqlDataSource dataSource) : ControllerBase
    {
        private readonly NpgsqlDataSource _dataSource = dataSource;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
            {
                return BadRequest("Username and password are required.");
            }

            const string sql = @"SELECT id, username
            FROM account
            WHERE username = @username
            AND password_hash = @password;
            ";

            await using var cmd = _dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue("username", account.Username.Trim());
            cmd.Parameters.AddWithValue("password", account.Password);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return Ok(new { exists = false });
            }

            var user = new
            {
                id = reader.GetString(0),
                username = reader.GetString(1)
            };

            return Ok(new
            {
                exists = true,
                user
            });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Account account)
        {
            if (string.IsNullOrWhiteSpace(account.Username) || string.IsNullOrWhiteSpace(account.Password))
            {
                return BadRequest("Username and password are required.");
            }
            const string sql = @"INSERT INTO account (id, name, username, email, password_hash)
            VALUES (@id, @username, @username, @username, @password);
            ";
            await using var cmd = _dataSource.CreateCommand(sql);
            var id = Guid.NewGuid().ToString();
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("username", account.Username.Trim());
            cmd.Parameters.AddWithValue("password", account.Password);
            try
            {
                await cmd.ExecuteNonQueryAsync();
                var user = new
                {
                    id,
                    username = account.Username
                };
                return Ok(new { success = true, user });
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // unique_violation
            {
                return Conflict(new { success = false, message = "Username already exists." });
            }
        }
    }
}
