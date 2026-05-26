using System.Text;
using ConclaseProject.Data;
using ConclaseProject.Interfaces;
using ConclaseProject.Repositories; // 👈 Added to find EventRepository and EventPassRepository
using ConclaseProject.Services;     // 👈 Corrected namespace to find your core services
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


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

builder.Services.AddScoped<AuthService>(); // Register the AuthService

var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "GTHR_SUPER_SECRET_SECURITY_KEY_LONG_STRING_2026";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

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
// 5. Build and Apply Database Migrations Automatically on Start
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();

    // This automatically checks your migrations folder and creates 
    // any missing tables (like 'Users') inside SQL Server safely!
    if (context.Database.IsRelational())
    {
        //context.Database.Migrate();
    }
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
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();