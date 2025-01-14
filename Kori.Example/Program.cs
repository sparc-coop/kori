using Kori;
using Kori.Example;
using Sparc.Kori;

var builder = BlossomApplication.CreateBuilder(args);
builder.Services.AddKori("https://kori.page", "https://localhost:53579");
var app = builder.Build();
await app.RunAsync<Html>();