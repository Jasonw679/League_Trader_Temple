using League_Trader_Temple.Server;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("Riftcodex", client =>
{
    client.BaseAddress = new Uri("https://api.riftcodex.com/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Cards")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Cards configuration.");

    return new NpgsqlDataSourceBuilder(connectionString).Build();
});
builder.Services.AddSingleton<CardDatabase>();
builder.Services.AddHostedService<RiftcodexCardSyncService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
