using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Seeding;
using Modules.Claims.Features;
using Modules.Claims.Infrastructure.Database;
using Modules.Common.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClaimsModule(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<SeedService>();

var app = builder.Build();

using (var migrationScope = app.Services.CreateScope())
{
    var claimDbContext = migrationScope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await claimDbContext.Database.MigrateAsync();
}

if (app.Configuration.GetValue("Seeding:Enabled", defaultValue: true))
{
    using var seedScope = app.Services.CreateScope();
    var seedService = seedScope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.SeedDataAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpointModulesFromAssemblyContaining(typeof(DependencyInjection));
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
