using BlazorSignalRApp.Client.Pages;
using BlazorSignalRApp.Components;
using Microsoft.AspNetCore.ResponseCompression;
using BlazorSignalRApp.Hubs;
using Npgsql;
using BlazorSignalRApp.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

/* SignalRを追加 */
builder.Services.AddSignalR();

/* これがないと、SignalRのHubとURLをマッピングできない */
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

// appsettings.json から接続文字列を読み込み
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("PostgreSqlConnection");

// NpgsqlConnection を依存性注入(DI)コンテナに登録
builder.Services.AddTransient(sp =>
{
    var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    return connection;
});

// 下記サービスも、DIコンテナに登録
builder.Services.AddTransient<BatchService>();

// HttpClient をサービスとして追加
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7250") });

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

/* SignalRのHubとURLをマッピング */
app.MapHub<ChatHub>("/chathub");
app.MapHub<BatchHub>("/batchhub");

app.Run();