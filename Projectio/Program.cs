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
using Projectio.Security.Interfaces.OAuth;
using Projectio.Security.Tools;
using System.IdentityModel.Tokens.Jwt;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if(connectionString == null)
    throw new InvalidOperationException("Connection string is not configured properly. Please check the appsettings.json file.");

builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((options) =>
{
    options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
builder.Services.AddSingleton<IOAuthProvider, GoogleProvider>();

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
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
})
.AddCookie()
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

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

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
