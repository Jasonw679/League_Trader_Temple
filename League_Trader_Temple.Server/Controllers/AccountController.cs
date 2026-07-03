using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace League_Trader_Temple.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly NpgsqlDataSource _dataSource;

        public AccountController(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        [HttpPost(Name = "account")]
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
    }
}
