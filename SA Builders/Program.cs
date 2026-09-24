using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SA_Builders.Data;
using SA_Builders.Models;
using SA_Builders.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core + Supabase PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        // Enables auto-retry for transient Supabase pooler drops
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// 2. Services Registration
// Fixed: Use SmtpEmailService in production/development directly (removed duplicate ConsoleEmailService)
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IClientAccessGuard, ClientAccessGuard>();

// 3. JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();

// 4. CORS Setup - Updated with Vercel Production Fallbacks & Credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Frontend:AllowedOrigins")
            .Get<string[]>() ?? new[]
            {
                "http://localhost:5173",
                "https://sabuilders-zeta.vercel.app"
            };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Make sure UseCors comes after UseRouting/UseStaticFiles and before UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 6. DB Migration & Admin Seeder Execution
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Automatically apply pending EF Core migrations to Supabase on startup
    db.Database.Migrate();

    DbSeeder.SeedAdmin(db, app.Configuration);
}

app.Run();