using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Infrastructure;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
// builder.Services.AddMudServices();
builder.Services.AddClientServices(builder.Configuration);
var host = builder.Build();
// var storageService = host.Services.GetRequiredService<IClientPreferenceManager>();


await host.RunAsync();