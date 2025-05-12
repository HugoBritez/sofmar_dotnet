using Microsoft.EntityFrameworkCore;
using Api.Data;
using DotNetEnv;
using Api.Auth.Services;
using NSwag.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// Cargar el archivo .env al inicio del programa
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión usando las variables de entorno
builder.Configuration["ConnectionStrings:DefaultConnection"] = 
    $"Server={Env.GetString("MYSQL_HOST")};Database={Env.GetString("MYSQL_DB")};User={Env.GetString("MYSQL_USER")};Password={Env.GetString("MYSQL_PASSWORD")};";

// Add services to the container.
builder.Services.AddControllers();

// Agregar servicios de autenticación
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Env.GetString("JWT_ISSUER"),
            ValidAudience = Env.GetString("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Env.GetString("JWT_KEY")))
        };
    });

// Registrar servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "API de Autenticación";
        document.Info.Description = "API para autenticación de usuarios";
    };
});

// Configurar DbContext antes de Build()
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();      // Para NSwag
    app.UseSwaggerUi();   // Para NSwag
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
