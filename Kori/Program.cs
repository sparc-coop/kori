using Kori;
using Kori.Text.Endpoints;

BlossomApplication.Run<Html>(args, builder =>
{
    builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped)
        .AddScoped<Translator>()
        .AddScoped<PostPageContent>();

}, app =>
{
    app.MapGet("/hello", () => "Hello, Kori!");

    app.MapPost("/api/PostPageContent", async (PostPageContentRequest request, PostPageContent postPageContent) =>
    {
        return await postPageContent.ExecuteAsync(request);
    });

});