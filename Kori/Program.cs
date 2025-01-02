using Kori;
using Sparc.Kori;

var builder = BlossomApplication.CreateBuilder(args);

builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddScoped<KoriTranslator>()
        .AddScoped<ITranslator, DeepLTranslator>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>();

builder.Services.AddKori("https://kori.page");

var app = builder.Build();
await app.RunAsync<Html>();