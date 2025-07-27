using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientServices(builder.Configuration);
builder.Services.AddSingleton<Client.Infrastructure.Realtime.IOrderRealtimeService, Client.Infrastructure.Realtime.OrderRealtimeService>();
var host = builder.Build();

await host.RunAsync();