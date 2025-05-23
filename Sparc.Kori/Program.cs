using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;
using Sparc.Blossom.Cloud.Api;
using Sparc.Kori;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddRefitClient<IBlossomCloudApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7185"));

await builder.Build().RunAsync();
