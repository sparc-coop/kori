using Kori;
using Microsoft.AspNetCore.Authentication;
using Sparc.Kori;

var builder = BlossomApplication.CreateBuilder(args);

builder.AddAuthentication<KoriUser>();
builder.Services.AddTransient<IClaimsTransformation, KoriClaimsTransformation>();


builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddScoped<KoriTranslatorProvider>()
        .AddScoped<ITranslator, DeepLTranslator>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>();

var app = builder.Build();
await app.RunAsync();