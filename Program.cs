using Microsoft.EntityFrameworkCore;
using Api.Data;
using DotNetEnv;
using Api.Auth.Services;
using NSwag.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Repositories.Interfaces;
using Api.Repositories.Implementations;
using Api.Middlewares;
using Api.Audit.Services;
using Api.Services.Interfaces;
using Api.Services.Implementations;
// Cargar el archivo .env al inicio del programa
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión usando las variables de entorno
builder.Configuration["ConnectionStrings:DefaultConnection"] = 
    $"Server={Env.GetString("MYSQL_HOST")};Database={Env.GetString("MYSQL_DB")};User={Env.GetString("MYSQL_USER")};Password={Env.GetString("MYSQL_PASSWORD")};";

// Add services to the container.
builder.Services.AddControllers();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

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
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IDepositosRepository, DepositoRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IListaPrecioRepository, ListaPrecioRepository>();
builder.Services.AddScoped<IMonedaRepository, MonedaRepository>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IDetalleVentaRepository, DetalleVentaRepository>();
builder.Services.AddScoped<IDetalleBonificacionRepository, DetalleBonificacionRepository>();
builder.Services.AddScoped<IDetalleArticulosEditadoRepository, DetalleArticuloEditadoRepository>();
builder.Services.AddScoped<IDetalleVentaVencimientoRepository, DetalleVencimientoRepository>();
builder.Services.AddScoped<IArticuloLoteRepository, ArticuloLoteRepository>();
builder.Services.AddScoped<IVentaService, VentaService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "API de Sofmar";
        document.Info.Description = "API para el proyecto web de sofmar";
    };

    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y su token JWT en el campo de texto.\nEjemplo: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
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

app.UseCors("AllowAll");  // Habilitar CORS
app.UseHttpsRedirection();
app.UseAuthentication();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
