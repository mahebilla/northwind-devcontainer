using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Features.BasicQueries.Queries;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Infrastructure.Persistence.Read;
using NorthwindCqrs.Infrastructure.Persistence.Write;

var builder = WebApplication.CreateBuilder(args);

// ── MediatR ───────────────────────────────────────────────────────────────────
// RegisterServicesFromAssembly scans the Application project for all
// IRequestHandler<,> implementations and wires them up automatically.
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetProductsWhereQuery).Assembly));

// ── Write DbContext → Northwind (normalized) ──────────────────────────────────
var writeConnStr = builder.Configuration.GetConnectionString("NorthwindWriteConnection");
builder.Services.AddDbContext<WriteDbContext>(options =>
    options.UseSqlServer(writeConnStr));

// ── Read DbContext → NorthwindRead (denormalized projections) ─────────────────
var readConnStr = builder.Configuration.GetConnectionString("NorthwindReadConnection");
builder.Services.AddDbContext<ReadDbContext>(options =>
    options.UseSqlServer(readConnStr));

// ── Dependency Inversion: bind interfaces to concrete implementations ──────────
// Application layer depends on IWriteDbContext and IReadDbContext — never on the concrete classes.
// This is the Clean Architecture composition root.
builder.Services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<WriteDbContext>());
builder.Services.AddScoped<IReadDbContext>(sp  => sp.GetRequiredService<ReadDbContext>());

// ── Controllers + JSON ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Northwind CQRS API", Version = "v1",
        Description = "Clean Architecture + CQRS with MediatR. Read DB: NorthwindRead, Write DB: Northwind." });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}

app.MapControllers();
app.Run();
