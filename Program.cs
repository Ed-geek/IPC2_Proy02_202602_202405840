using Proyecto2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<CatalogService>();
builder.Services.AddSingleton<GraphvizService>();
builder.Services.AddSingleton<XmlService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();