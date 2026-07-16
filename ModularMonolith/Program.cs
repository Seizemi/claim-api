using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features;
using Modules.Claims.Infrastructure.Database;
using Modules.Common.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClaimsModule(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var migrationScope = app.Services.CreateScope())
{
    migrationScope.ServiceProvider.GetRequiredService<ClaimsDbContext>().Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpointModulesFromAssemblyContaining(typeof(DependencyInjection));

app.Run();
