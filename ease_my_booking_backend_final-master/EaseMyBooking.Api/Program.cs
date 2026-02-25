using EaseMyBooking.Api.Data;
using EaseMyBooking.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Connection string setup
// ----------------------
// Order of precedence: env DATABASE_URL -> ConnectionStrings:DefaultConnection -> ENV DB_CONNECTION
string rawConn = Environment.GetEnvironmentVariable("DATABASE_URL")
                 ?? builder.Configuration.GetConnectionString("DefaultConnection")
                 ?? Environment.GetEnvironmentVariable("DB_CONNECTION")
                 ?? throw new Exception("Missing DATABASE_URL or ConnectionStrings:DefaultConnection or DB_CONNECTION.");

// Normalize and trim accidental quotes/newlines
rawConn = rawConn.Trim().Trim('"').Trim();

// Some platforms may surface a 'tcp://' prefix — normalize to postgres scheme if needed
if (rawConn.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase))
{
    // If tcp://host:port[/db] is provided, convert scheme so Uri can parse userinfo if present
    rawConn = "postgresql://" + rawConn.Substring("tcp://".Length);
}

// If it's a postgres URI, convert to Npgsql key/value string
if (rawConn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    rawConn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    rawConn = ConvertPostgresUrlToNpgsql(rawConn);
}

// Register DbContext with provider detection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (IsSqlServerConnectionString(rawConn))
    {
        options.UseSqlServer(rawConn);
    }
    else
    {
        options.UseNpgsql(rawConn);
    }
});

// ----------------------
// Controllers + JSON
// ----------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ----------------------
// JWT configuration
// ----------------------
var secret = builder.Configuration["JwtSettings:SecretKey"]
             ?? Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? throw new Exception("Missing JwtSettings:SecretKey (or JWT_SECRET env var).");

// Support both raw and base64: prefixed secrets
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

// ----------------------
// Other services
// ----------------------
builder.Services.AddSingleton<CloudinaryService>();

// ----------------------
// CORS
// ----------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ----------------------
// Swagger / OpenAPI
// ----------------------
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

// ----------------------
// Migrations & seeding (guarded)
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsRelational())
        {
            // Log safe connection metadata (host/db) without sensitive credentials
            try
            {
                var connBuilder = new NpgsqlConnectionStringBuilder(rawConn);
                logger.LogInformation("Attempting DB connection to host={Host}, database={Database}", connBuilder.Host, connBuilder.Database);
            }
            catch
            {
                // not an Npgsql-style connection string (maybe SQL Server); try to parse SQL Server keys safely
                try
                {
                    var parts = rawConn.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    string host = parts.FirstOrDefault(p => p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) || p.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)) ?? "unknown";
                    string database = parts.FirstOrDefault(p => p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase) || p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase)) ?? "unknown";
                    logger.LogInformation("Attempting DB connection (SQL Server hint) host/db: {Host}/{Database}", host, database);
                }
                catch
                {
                    logger.LogInformation("Attempting DB connection (unable to extract host/database safely).");
                }
            }

            logger.LogInformation("Applying pending EF Core migrations (if any).");
            dbContext.Database.Migrate();
            logger.LogInformation("Database migrations applied.");
        }
    }
    catch (Exception ex)
    {
        // Provide a helpful, non-sensitive error message
        var safeMessage = "Database migration failed during startup. Check DATABASE_URL / ConnectionStrings and that DB is reachable from this environment.";
        logger.LogError(ex, safeMessage);
        // Fail startup so the platform can surface the error and you can iterate
        throw;
    }

    // Dev-only seeding (uncomment for local development if needed)
    // var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    // if (env.IsDevelopment())
    // {
    //     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //     DbSeeder.Seed(db);
    // }
}

// ----------------------
// Middleware pipeline
// ----------------------
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

// ----------------------
// Helper methods
// ----------------------

static bool IsSqlServerConnectionString(string conn)
{
    if (string.IsNullOrWhiteSpace(conn)) return false;

    // Detect explicit SQL Server key/value hints
    if (conn.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ||
        conn.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) ||
        conn.IndexOf("Initial Catalog=", StringComparison.OrdinalIgnoreCase) >= 0 ||
        conn.IndexOf("Integrated Security=", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        return true;
    }

    // If it contains Host= it's probably Npgsql-style
    if (conn.IndexOf("Host=", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        return false;
    }

    // Conservative default: if it contains "postgres" (but not key/value Host=) assume Postgres (we converted URIs earlier)
    if (conn.IndexOf("postgres", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        return false;
    }

    // Default to SQL Server only when common SQL Server tokens appear; otherwise assume Postgres
    return false;
}

/// <summary>
/// Converts a postgres:// or postgresql:// URL into a safe Npgsql connection string.
/// Uses NpgsqlConnectionStringBuilder to produce a canonical key=value string.
/// </summary>
static string ConvertPostgresUrlToNpgsql(string postgresUrl)
{
    if (string.IsNullOrWhiteSpace(postgresUrl))
        throw new ArgumentException("postgresUrl is empty", nameof(postgresUrl));

    try
    {
        var uri = new Uri(postgresUrl);

        // userinfo may contain colon; split into at most 2 parts
        var userInfoParts = uri.UserInfo.Split(new[] { ':' }, 2);
        var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : "";
        var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : "";

        // AbsolutePath avoids query string contamination
        var database = uri.AbsolutePath?.TrimStart('/') ?? "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = database,
            Username = username,
            Password = password,
            SslMode = Npgsql.SslMode.Require,
            TrustServerCertificate = true,
            Pooling = true
        };

        // If the URL includes query params (e.g., sslmode), we could parse them here and apply overrides.
        return builder.ToString();
    }
    catch (Exception ex)
    {
        throw new Exception("Failed to parse PostgreSQL URL. Ensure DATABASE_URL is a valid postgres:// or postgresql:// URI.", ex);
    }
}