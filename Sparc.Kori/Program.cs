using Sparc.Kori.Components;

var builder = BlossomApplication.CreateBuilder<App>(args);

builder.AddBlossomCloud();

var app = builder.Build();

await app.RunAsync<App>();

