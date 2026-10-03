using Dsw2025Tpi.Api.Configurations;
using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Api.Middlewares;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Application.Validators;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;


namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers()
            .AddValidationErrorResponse();
      
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Desarrollo de Software",
                Version = "v1",
            });
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingresar el token",
                Type = SecuritySchemeType.ApiKey
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            o.ExampleFilters();
        });

        builder.Services.AddHealthChecks();

        // ========= Identity Configuration =========
        builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredUniqueChars = 1
            };

            // Bloqueo temporal tras intentos fallidos (frena la fuerza bruta).
            // Se aplica porque el login usa PasswordSignInAsync(lockoutOnFailure: true).
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<Dsw2025TpiDbContext>()
        .AddDefaultTokenProviders();

        // ========= JWT Configuration =========
        // Jwt:Key es un secreto: no esta en appsettings.json (ver README).
        // HMAC-SHA256 exige una clave de al menos 256 bits (32 bytes).
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"];
        if (string.IsNullOrWhiteSpace(keyText))
            throw new InvalidOperationException(
                "Falta la configuracion 'Jwt:Key'. En desarrollo: dotnet user-secrets set \"Jwt:Key\" \"<clave>\" --project Dsw2025Tpi.Api");

        var key = Encoding.UTF8.GetBytes(keyText);
        if (key.Length < 32)
            throw new InvalidOperationException("'Jwt:Key' debe tener al menos 32 bytes.");

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
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        // ========= Application Services =========
        // Registra el DbContext (dominio + Identity), los repositorios y los
        // servicios de aplicacion.
        builder.Services.AddDomainServices(builder.Configuration);

        // ========= JWT Token Service =========
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        // ========= FluentValidation =========
        builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<ProductUpdateRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderItemRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderStatusRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<FilterOrderValidator>();

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();

        // ========= AutoMapper =========
        builder.Services.AddAutoMapper(typeof(Dsw2025Tpi.Application.Mappings.MappingProfiles));

        // ========= Swagger Examples =========
        builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

        // ========= Authorization =========
        builder.Services.AddAuthorization();

        // ========= Rate Limiting =========
        // Limite global por IP y politica "auth" para login/registro (ver
        // RateLimitingExtensions y la seccion RateLimiting de appsettings.json).
        builder.Services.AddApiRateLimiting(builder.Configuration);
        
        // ========= CORS Configuration =========
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirFrontend", policy =>
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        var app = builder.Build();

        // Roles y primer administrador (desde la configuracion 'SeedAdmin').
        await IdentitySeeder.SeedAsync(app.Services);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // ========= Middleware Pipeline =========
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();
        app.UseCors("PermitirFrontend");
        // Despues de CORS: el 429 lleva los headers CORS y el frontend puede leer
        // el mensaje. Antes de la autenticacion: el exceso se rechaza sin gastar
        // en validar el JWT ni en hashear contraseñas.
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/healthcheck");

        await app.RunAsync();
    }
}
