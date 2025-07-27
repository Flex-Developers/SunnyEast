using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();
builder.Services.AddSignalR();
builder.Services.AddScoped<Application.Contract.Realtime.IOrderRealtimeNotifier, WebApi.Services.OrderRealtimeNotifier>();
var app = builder.Build();

app.UseCors(s => s.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().Build());

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContextInitializer = services.GetRequiredService<ApplicationDbContextInitializer>();

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }

    await dbContextInitializer.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<WebApi.Hubs.OrderHub>("/hubs/orders");

app.Run();

public abstract partial class Program;