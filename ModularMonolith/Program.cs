using Modules.Claims.Features;
using Modules.Common.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClaimsModule(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpointModulesFromAssemblyContaining(typeof(DependencyInjection));

app.Run();
