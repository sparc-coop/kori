using Kori;
using Scalar.AspNetCore;

BlossomApplication.Run<Html>(args, builder =>
{
    builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddScoped<Translator>()
        .AddScoped<ITranslator, DeepLTranslator>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>();

    // builder.AddKori(new Uri("https://kori.app"));
},
app =>
{
    if (app.Environment.IsDevelopment())
        app.MapScalarApiReference();
});