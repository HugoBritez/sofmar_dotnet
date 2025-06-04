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
using Dapper;
using Api.Models.ViewModels;
using Serilog;

// Cargar el archivo .env al inicio del programa
Env.Load();

var builder = WebApplication.CreateBuilder(args);


SqlMapper.AddTypeHandler(new JsonTypeHandler<List<VentaDetalle>>());
SqlMapper.AddTypeHandler(new JsonTypeHandler<List<SucursalData>>());
SqlMapper.AddTypeHandler(new JsonTypeHandler<List<ConfiguracionFacturaElectronica>>());

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
builder.Services.AddScoped<IAreaSecuenciaRepository, AreaSecuenciaRepository>();
builder.Services.AddScoped<IPedidosRepository, PedidosRepository>();
builder.Services.AddScoped<IDetallePedidoRepository, DetallePedidoRepository>();
builder.Services.AddScoped<IDetallePedidoFaltanteRepository, DetallePedidoFaltanteRepository>();
builder.Services.AddScoped<IPedidoEstadoRepository, PedidoEstadoRepository>();
builder.Services.AddScoped<IPresupuestosRepository, PresupuestosRepository>();
builder.Services.AddScoped<IDetallePresupuestoRepository, DetallePresupuestoRepository>();
builder.Services.AddScoped<IPedidosService, PedidoService>();
builder.Services.AddScoped<IPresupuestoObservacionRepository, PresupuestoObservacionRepository>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IMarcaRepository, MarcaRepository>();
builder.Services.AddScoped<IUbicacionesRepository, UbicacionesRepository>();
builder.Services.AddScoped<IProveedoresRepository, ProveedoresRepository>();
builder.Services.AddScoped<IUnidadMedidaRepository, UnidadesMedidaRepository>();
builder.Services.AddScoped<ISububicacionRepository, SububicacionRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IDetalleInventarioRepository, DetalleInventarioRepository>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IComprasRepository, ComprasRepository>();
builder.Services.AddScoped<IDetalleComprasRepository, DetalleComprasRepository>();
builder.Services.AddScoped<IControlIngresoRepository, ControlIngresoRepository>();
builder.Services.AddScoped<ITransferenciasRepository, TransferenciasRepository>();
builder.Services.AddScoped<ITransferenciasItemsRepository, TransferenciasItemsRepository>();
builder.Services.AddScoped<ITransferenciasItemsVencimientoRepository, TransferenciaItemsVencimientoRepository>();
builder.Services.AddScoped<IControlIngresoService, ControlIngresoService>();
builder.Services.AddScoped<IAgendasRepository, AgendasRepository>();
builder.Services.AddScoped<IAgendasNotasRepository, AgendasNotasRepository>();
builder.Services.AddScoped<IAgendaSubvisitaRepository, AgendaSubvisitaRepository>();
builder.Services.AddScoped<ILocalizacionesRepository, LocalizacionesRepository>();
builder.Services.AddScoped<IAgendasService, AgendaService>();
builder.Services.AddScoped<IContabilidadService, ContabilidadService>();
builder.Services.AddScoped<IContabilidadRepository, ContabilidadRepository>();
builder.Services.AddScoped<ICotizacionRepository, CotizacionRepository>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
builder.Services.AddScoped<IFinancieroRepository, FinancieroRepository>();
builder.Services.AddScoped<IPersonalService, PersonalService>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IZonaRepository, ZonaRepository>();
builder.Services.AddScoped<ICiudadesRepository, CiudadesRepository>();
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<ITipoDocumentoRepository, TipoDocumentoRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();

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


Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .WriteTo.Console()
             .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
             .WriteTo.Seq("http://localhost:5341")
             .CreateLogger();

builder.Host.UseSerilog();

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
app.UseMiddleware<ResponseWrapperMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
