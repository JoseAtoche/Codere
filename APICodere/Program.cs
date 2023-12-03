using APICodere.Repository;

var builder = WebApplication.CreateBuilder(args);

// Configuración de AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<ShowsRepository>(ServiceLifetime.Scoped);

builder.Services.AddControllers();

// Aprender más sobre la configuración de Swagger/OpenAPI en https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Inicializa la base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {


        var context = services.GetRequiredService<ShowsRepository>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al aplicar migraciones: " + ex.Message);
        // Puedes manejar el error de la manera que prefieras.
    }
}

// Configuración del canal de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configuración del registro
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//Rutas públicas
var publicPaths = new List<string>
{
    "/api/Shows/showAllData"
};

// Middleware de seguridad (Validación de la clave de API)
app.Use((context, next) =>
{
    if (publicPaths.Contains(context.Request.Path))
    {
        return next();
    }

    var apiKey = context.Request.Headers["ApiKey"];
    var expectedApiKey = builder.Configuration["ApiSettings:ApiKey"];

    if (apiKey != expectedApiKey)
    {
        context.Response.StatusCode = 401; // Unauthorized
        return context.Response.WriteAsync("INVALID API key");
    }

    return next();
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();