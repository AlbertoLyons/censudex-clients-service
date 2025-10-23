using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dotenv.net;
using dotenv.net.Utilities;
using censudex_clients_service.src.data;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;
using CensudexUsersService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
// Carga de variables de entorno desde el archivo .env
DotEnv.Load();

builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    options.ListenAnyIP(int.Parse(port), listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Configuración de la base de datos PostgreSQL con Entity Framework Core
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(
        EnvReader.GetStringValue("PostgreSQLConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        )
    );
});
// Configuración del servicio de hashing de contraseñas con el algoritmo BCrypt
builder.Services.AddScoped<IPasswordHasher<User>, BCryptService<User>>();
// Registro del servicio de SendGrid para el envío de correos electrónicos
builder.Services.AddSingleton<SendGridService>();
// Configuración de Identity para la gestión de usuarios y roles
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Configuración de las opciones de contraseña
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();


var app = builder.Build();
// Aplicación de migraciones pendientes y siembra inicial de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
    _ = DataSeeder.Initialize(services);
    
}

app.MapGrpcService<UserService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();