using BusinessLogic.Interfaces;
using BusinessLogic.Services;
using Microsoft.EntityFrameworkCore;
using Persistance.Data;
using Persistance.Repositories;
using Persistence.Interfaces;
using Persistence.Repositories;
using Votify.BusinessLogic.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Forzar explícitamente los puertos conocidos por si falla launchSettings sólo en local
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://localhost:7185", "http://localhost:5059");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Añadido para el Patrón Observador

// Registramos el Sujeto Concreto como Singleton para mantener el diccionario de estado vivo
builder.Services.AddSingleton<API.Hubs.ISujetoVotacion, API.Hubs.GestorVotosRealTime>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IDAL, EntityFrameworkDAL>();

// Habilitar CORS para permitir peticiones desde HTML
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurado.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar repositorios
builder.Services.AddScoped<IEventoRepository, EventoRepository>();

// Registrar servicios
builder.Services.AddHttpClient<IACommentSynthesisService>();
builder.Services.AddScoped<IIACommentSynthesisService>(provider => 
{
    var realService = provider.GetRequiredService<IACommentSynthesisService>();
    var dal = provider.GetRequiredService<IDAL>();
    return new CacheProxyIAService(realService, dal);
});
builder.Services.AddHttpClient<IIARoadmapService, IARoadmapService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<BusinessLogic.Interfaces.IVotacionService, Votify.BusinessLogic.Services.VotacionService>();

var app = builder.Build();

// Ensure database schema compatibility for development (adds missing columns if needed).
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // ¡Importante para Render! Aplicar las migraciones de EF Core automáticamente.
        // Esto creará las tablas ("Votaciones", "Events", etc.) en tu base de datos de producción recién nacida.
        db.Database.Migrate();

        var seedEnv = Environment.GetEnvironmentVariable("SEED_DATABASE");
        if (args.Contains("--seed") || seedEnv == "true")
        {
            await API.DbSeeder.SeedAsync(db);
            // ¡Hemos quitado el return; para que la API NO se apague!
        }
        
        db.EnsureSchemaCompat();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: could not ensure DB schema: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

// Aplicar política CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();
app.MapHub<API.Hubs.VotacionHub>("/votacionhub"); // Mapear el Hub de SignalR para el patrón Observador

// Redirigir la raíz ("/") a la interfaz de Swagger automáticamente
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();

// Se utiliza para poder realizar pruebas para de aceptación API HTTP
public partial class Program { }

