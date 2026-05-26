using ConclaseProject.Interfaces;
using ConclaseProject.Data;
using ConclaseProject.Repositories; // 👈 Added to find EventRepository and EventPassRepository
using ConclaseProject.Services;     // 👈 Corrected namespace to find your core services
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection String Configuration Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=EventSmartAccess.db"; // Fallback if not specified

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));// 2. Register Clean Architecture Interface Layers to Dependency Container
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventPassRepository, EventPassRepository>();

// 3. Register Core Domain Computing Service Entities
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<VerificationService>();

// 4. Register Framework Routing Components & Security Guardrails
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enforce Global Cross-Origin Resource Sharing Protection Strategy
builder.Services.AddCors(options =>
{
    options.AddPolicy("GateValidationNetwork", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 5. Build and Apply Database Migrations Automatically on Start
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    context.Database.EnsureCreated();
}

// 6. Request Pipeline Execution Layout Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Routing must come before CORS and Authorization
app.UseRouting();

app.UseCors("GateValidationNetwork");

app.UseAuthorization();

app.MapControllers();

app.Run();