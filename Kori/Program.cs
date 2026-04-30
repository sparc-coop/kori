using Kori;
using Sparc.Blossom;
using Sparc.Blossom.Platforms.Server;

var builder = BlossomApplication.CreateBuilder<Html>(args);

builder.AddSparcEngine(builder.Configuration["SparcEngine"]);
//builder.Services.AddDataProtection()
//    .SetApplicationName("Kori")
//    .PersistKeysToAzureBlobStorage(builder.Configuration.GetConnectionString("Storage")!, "dataprotection", "Kori.xml");

var app = builder.Build();

//if (app is BlossomServerApplication server)
//    server.Host.Map
await app.RunAsync<Html>();