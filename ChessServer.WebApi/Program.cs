using ChessServer.WebApi.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBaseSetUp(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notification");

app.Run();