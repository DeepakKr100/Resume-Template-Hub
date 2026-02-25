using EaseMyBooking.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using EaseMyBooking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// DB - Support both SQL Server (dev) and PostgreSQL (production)
// Check DATABASE_URL env var first (Render), then fall back to appsettings
var conn = Environment.GetEnvironmentVariable("DATABASE_URL")
           ?? builder.Configuration.GetConnectionString("DefaultConnection")
           ?? throw new Exception("Missing DATABASE_URL or ConnectionStrings:DefaultConnection");

// Convert postgresql:// URL to Npgsql format if needed
if (conn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    conn = ConvertPostgresUrlToNpgsql(conn);
}

builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    if (conn.Contains("Host=", StringComparison.OrdinalIgnoreCase) || conn.Contains("postgres", StringComparison.OrdinalIgnoreCase))
        o.UseNpgsql(conn);
    else
        o.UseSqlServer(conn);
});

// Controllers + JSON
builder.Services
  .AddControllers()
  .AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
      o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  });

// JWT
var secret = builder.Configuration["JwtSettings:SecretKey"]
             ?? throw new Exception("Missing JwtSettings:SecretKey");

// Support both raw and base64-prefixed secrets
byte[] keyBytes = secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase)
    ? Convert.FromBase64String(secret["base64:".Length..])
    : Encoding.UTF8.GetBytes(secret);

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services
  .AddAuthentication(o =>
  {
      o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(o =>
  {
      o.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = "ResumeTemplateHubAPI",
          ValidAudience = "ResumeTemplateHubAPI",
          IssuerSigningKey = signingKey,
          RoleClaimType = ClaimTypes.Role
      };
  });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<CloudinaryService>();

// CORS (local dev frontends + prod)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Resume Template Hub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed (dev only)
using (var scope = app.Services.CreateScope())
{   
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }

    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    // if (env.IsDevelopment())
    // {
    //     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //     DbSeeder.Seed(db);
    // }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapControllers();

app.Run();

/// <summary>
/// Converts PostgreSQL connection URL (postgresql://user:pass@host:port/db) to Npgsql format
/// </summary>
static string ConvertPostgresUrlToNpgsql(string postgresUrl)
{
    try
    {
        var uri = new Uri(postgresUrl);
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.PathAndQuery.TrimStart('/');
        var username = uri.UserInfo.Split(':')[0];
        var password = uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[1] : "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to parse PostgreSQL URL: {postgresUrl}", ex);
    }
}
