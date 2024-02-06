using Application;
using Infrastructure;
using Infrastructure.Persistence;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();

var app = builder.Build();

// Получаем экземпляр ApplicationDbContextInitializer из контейнера зависимостей
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContextInitializer = services.GetRequiredService<ApplicationDbContextInitializer>();

    // Вызываем метод SeedAsync для инициализации базы данных
    await dbContextInitializer.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();

app.Run();

public abstract partial class Program;