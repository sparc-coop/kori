using Kori;

BlossomApplication.Run<Html>(args, builder =>
{
    builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped);
});