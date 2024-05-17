using ChessServer.WebApi.Common;
using MapsterMapper;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBaseSetUp(builder.Configuration);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}   

app.UseHttpsRedirection();
app.MapHub<NotificationHub>("notification");

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/yep", async context => await context.Response.WriteAsync("huy"));

app.Run();