using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Projectio.Core.Dtos;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Migrations;
using Projectio.Persistence;
using Projectio.Security.Authorization.OAuthProvider;
using Projectio.Security.Authorization.OAuthSetting;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Interfaces.OAuth.OAuthSettngs.Google;
using Projectio.Security.Tools;
using System.IdentityModel.Tokens.Jwt;

DotNetEnv.Env.NoClobber().Load(".env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if(connectionString == null)
    throw new InvalidOperationException("Connection string is not configured properly. Please check the appsettings.json file.");

builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen((options) =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste only the access token. Swagger adds the word Bearer itself."
    });
});


builder.Services.Configure<JWTConfiguration>(
    builder.Configuration.GetSection("JWT_settings"));

var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JWTConfiguration>()
    ?? throw new InvalidOperationException("Jwt settings are not configured properly.");

builder.Services.AddSingleton<IJWTConfiguration>(jwtConfig);

var gOAuthSettings = builder.Configuration.GetSection("Google").Get<GoogleSettings>()
    ?? throw new InvalidOperationException("Google OAuth settings are not configured properly.");

var keys = typeof(GoogleSettings).GetProperties();
foreach (var key in keys)
    if (string.IsNullOrEmpty(key.GetValue(gOAuthSettings)?.ToString()))
        throw new Exception($"Google OAuth settings are not configured properly. Missing value for {key.Name}.");

builder.Services.AddSingleton<IGoogleSettings>(gOAuthSettings);

// Applications that may log users in through Projectio (e.g. the Portio BFF)
var clients = builder.Configuration.GetSection("Clients").Get<List<OAuthClient>>()
    ?? throw new InvalidOperationException("OAuth clients are not configured properly. Add Clients__0__ClientId, Clients__0__ClientSecret and Clients__0__RedirectUri.");
builder.Services.AddSingleton(new ClientStore(clients));

var frontendUrl = builder.Configuration["Frontend:Url"]
    ?? throw new InvalidOperationException("Frontend:Url is not configured properly.");

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<ApplicationUser, UserDto>();
    cfg.CreateMap<ApplicationUser, UserOutDTO>();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<ApplicationUser>>();



builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

var isDevelopment = builder.Configuration["IsDevelopment"];
if(isDevelopment == null)
    throw new InvalidOperationException("IsDevelopment setting is not configured properly. Check secret variables");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Keep claim names as they are in the token ("sub", "role", "email")
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidateAudience = true,
        ValidAudiences = jwtConfig.Audience,
        ValidateIssuerSigningKey = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        IssuerSigningKey = KeyToolset.GetPublicKey(jwtConfig.SigningKey!),
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = "role"
    };
})
.AddGoogleOpenIdConnect(options =>
{
    options.ClientId = gOAuthSettings.ClientId;
    options.ClientSecret = gOAuthSettings.ClientSecret;
    options.CallbackPath = gOAuthSettings.RedirectUris[0];
    options.SignInScheme = IdentityConstants.ExternalScheme;
    options.SaveTokens = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});


builder.Services.AddAuthorization();
builder.Services.AddCors();


var app = builder.Build();

Configure(app);

if(isDevelopment.ToLower() == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCookiePolicy();

// In Docker, HTTPS ends at the reverse proxy (Portio) and the services talk plain HTTP on the private network.
// ASPNETCORE_FORWARDEDHEADERS_ENABLED=true tells ASP.NET to trust the proxy's X-Forwarded-Proto header,
// and then there is nothing to redirect. Without that setting (local development) HTTP is redirected to HTTPS
var behindProxy = string.Equals(builder.Configuration["ForwardedHeaders_Enabled"], "true", StringComparison.OrdinalIgnoreCase);
if (!behindProxy)
    app.UseHttpsRedirection();

app.UseRouting();
// Only our own frontend may call the API from a browser
app.UseCors(x => x.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

void Configure(WebApplication host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.Migrate();
        }
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        AppDbContextSeed.SeedData(userManager, roleManager).Wait();
    }
    catch
    {
        throw;
    }
}
