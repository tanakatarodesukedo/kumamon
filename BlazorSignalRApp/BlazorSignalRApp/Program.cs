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

/* SignalR��ǉ� */
builder.Services.AddSignalR();

/* ���ꂪ�Ȃ��ƁASignalR��Hub��URL���}�b�s���O�ł��Ȃ� */
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

// appsettings.json ����ڑ��������ǂݍ���
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("PostgreSqlConnection");

// NpgsqlConnection ���ˑ�������(DI)�R���e�i�ɓo�^
builder.Services.AddTransient(sp =>
{
    var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    return connection;
});

// ���L�T�[�r�X���ADI�R���e�i�ɓo�^
builder.Services.AddTransient<BatchService>();

// HttpClient ���T�[�r�X�Ƃ��Ēǉ�
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

/* SignalR��Hub��URL���}�b�s���O */
app.MapHub<ChatHub>("/chathub");
app.MapHub<BatchHub>("/batchhub");

app.Run();