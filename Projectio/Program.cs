using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projectio.Core.Dtos;
using Projectio.Core.Enums;
using Projectio.Core.Interfaces.Logging;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Logs;
using Projectio.Migrations;
using Projectio.Persistence;
using Projectio.Security;
using Projectio.Security.Authorization.Handlers;
using Projectio.Security.Interfaces.JWT;
using Projectio.Security.Interfaces.KeyManagement;
using Projectio.Security.Interfaces.Signing;
using Projectio.Security.KeyManagement.Algorithms;
using Projectio.Security.KeyManagement.SKProviders;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("connectionString");

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


var jwtIssuer = builder.Configuration.GetSection("JWT_settings")["Issuer"];
var jwtAudience = builder.Configuration.GetSection("JWT_settings")["Audience"];
var jwtSigningKey = builder.Configuration.GetSection("JWT_settings")["SigningKey"];
var jwtTokenTImeout = builder.Configuration.GetSection("JWT_settings")["TokenTimeoutMinutes"];


builder.Services.AddSingleton<IEncryptionProvider>(provider =>
{
    var signer = provider.GetRequiredService<ISigner>();
    return new ASKeyProvider(signer, jwtSigningKey);
});


builder.Services.AddSingleton<IJWTConfiguration>((jwt) =>
{
    return builder.Configuration.GetSection("JWT_settings").Get<JWTConfiguration>();
});

builder.Services.AddSingleton<ILogEntryFactory, LogEntryFactory>();

builder.Services.AddScoped<IJWT, JWT>();


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
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // or any period you want
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});




builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>(
    JwtBearerDefaults.AuthenticationScheme,
    options => { }
);



builder.Services.AddCors();



var app = builder.Build();

Configure(app);






// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<Projectio.MiddleWare.ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());
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
    catch (Exception ex)
    {
        throw;
    }
}