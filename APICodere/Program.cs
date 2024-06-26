using Repository;
using Services;
using System.Globalization;
using Utils;


var builder = WebApplication.CreateBuilder(args);

// Configuración de AutoMapper
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddDbContext<ShowsRepository>(ServiceLifetime.Scoped);

// Agregar el servicio de la capa de servicio
builder.Services.AddScoped<ShowsService>();

builder.Services.AddControllers();

// Aprender más sobre la configuración de Swagger/OpenAPI en https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Inicializa la base de datos
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

// Rutas públicas
var publicPaths = new List<string>
{
    "/api/Shows/showAllData"
};

// Middleware de seguridad (Validación de la clave de API)
app.Use((context, next) =>
{
    var showsService = context.RequestServices.GetRequiredService<ShowsService>();

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

// Middleware para manejar las operaciones de la capa de servicio
app.Use((context, next) =>
{
    var showsService = context.RequestServices.GetRequiredService<ShowsService>();

    //Aquí podemos insertar algun Middleware de la capa de servicio

    return next();
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
