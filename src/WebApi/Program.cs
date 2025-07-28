using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Client",
        p => p
            .WithOrigins("http://localhost:5289") // адрес вашего Blazor WASM клиента
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // нужно для SignalR с токеном
});

var app = builder.Build();




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
app.UseRouting();

// app.UseHttpsRedirection();

app.UseCors("Client");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/hubs/orders");

app.Run();

public abstract partial class Program;