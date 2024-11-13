using Kori;
using Scalar.AspNetCore;

BlossomApplication.Run<Html>(args, builder =>
{
    builder.Services.AddCosmos<KoriContext>(builder.Configuration.GetConnectionString("CosmosDb")!, "kori", ServiceLifetime.Scoped);
},
app =>
{
    if (app.Environment.IsDevelopment())
        app.MapScalarApiReference();
});