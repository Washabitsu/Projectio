using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projectio.Core.Enums;
using Projectio.Core.Interfaces;
using Projectio.Core.Models;
using Projectio.Helpers;
using Projectio.Migrations;
using Projectio.Persistence;
using Projectio.Security;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using Projectio.Core.Dtos;

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

builder.Services.AddSingleton<IJWTConfiguration>((jwt) =>
{
    return builder.Configuration.GetSection("JWT_settings").Get<JWTConfiguration>();
});

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
builder.Services.AddCors();

var jwtIssuer = builder.Configuration.GetSection("JWT_settings")["Issuer"];
var jwtAudience = builder.Configuration.GetSection("JWT_settings")["Audience"];
var jwtSigningKey = builder.Configuration.GetSection("JWT_settings")["SigningKey"];
var jwtTokenTImeout = builder.Configuration.GetSection("JWT_settings")["TokenTimeoutMinutes"];



builder.Services.AddSingleton<IJWTProvider>(provider =>
{
    return new AppSettingsJwtProvider(jwtSigningKey);
});


builder.Services.AddAuthentication(auth =>
{
    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new AppSettingsJwtProvider(jwtSigningKey).GetPublicKey()
    };
});



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