using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

var builder = WebApplication.CreateBuilder(args);

// === Services ===

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular reference errors (Northwind has many circular nav properties)
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext — standard (used by most controllers)
var connectionString = builder.Configuration.GetConnectionString("NorthwindConnection");
builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(connectionString));

// Register a second DbContext with lazy loading proxies (for RelatedDataController)
builder.Services.AddDbContext<NorthwindLazyContext>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlServer(connectionString));

// CORS for frontend dev servers (React :5173, Blazor WASM :5200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// === Middleware Pipeline ===

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}

app.UseHttpsRedirection();

// Serve React build output as static files
// In dev: from ../northwind-client/dist (Vite output)
// In production (Azure): from wwwroot/ (copied by PublishReactApp MSBuild target)
var spaPath = Path.Combine(app.Environment.ContentRootPath, "..", "northwind-client", "dist");
var spaProvider = Directory.Exists(spaPath)
    ? new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(spaPath))
    : app.Environment.WebRootFileProvider;

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = spaProvider,
    RequestPath = ""
});

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// SPA fallback — serve index.html for non-API routes (client-side routing)
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    FileProvider = spaProvider
});

app.Run();
