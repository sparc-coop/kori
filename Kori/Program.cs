using Kori;
using Kori.PageCommands;
using Scalar.AspNetCore;

BlossomApplication.Run<Html>(args, builder =>
{
    builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped)
        .AddAzureStorage(builder.Configuration.GetConnectionString("Storage")!)
        .AddScoped<Translator>()
        .AddScoped<ITranslator, DeepLTranslator>()
        .AddScoped<ITranslator, AzureTranslator>()
        .AddScoped<ISpeaker, AzureSpeaker>()
        .AddScoped<AddLanguage>();
},
app =>
{
    if (app.Environment.IsDevelopment())
        app.MapScalarApiReference();

    app.MapPut("/pages/AddLanguage", async(string pageId, string languageId, AddLanguage command) =>
    {
        await command.ExecuteAsync(pageId, languageId);
        return Results.Ok("ok");
    });
});