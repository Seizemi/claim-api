using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

using (var migrationScope = app.Services.CreateScope())
{
    var claimDbContext = migrationScope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await claimDbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpointModulesFromAssemblyContaining(typeof(DependencyInjection));

app.Run();
