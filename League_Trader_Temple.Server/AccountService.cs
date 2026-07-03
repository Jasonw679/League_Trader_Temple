namespace League_Trader_Temple.Server
{
    public class AccountService(AccountDatabase accountDatabase) : IHostedService
    {
        private readonly AccountDatabase accountDatabase = accountDatabase;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await accountDatabase.EnsureCreatedAsync(cancellationToken);
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
