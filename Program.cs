using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dotenv.net;
using dotenv.net.Utilities;
using censudex_clients_service.src.data;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;

var builder = WebApplication.CreateBuilder(args);
// Carga de variables de entorno desde el archivo .env
DotEnv.Load();
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
// Configuración de servicios y dependencias
builder.Services.AddOpenApi();
builder.Services.AddControllers();
// Configuración del servicio de hashing de contraseñas con el algoritmo BCrypt
builder.Services.AddScoped<IPasswordHasher<User>, BCryptService<User>>();
// Registro del servicio de SendGrid para el envío de correos electrónicos
builder.Services.AddSingleton<SendGridService>();
builder.Services.AddEndpointsApiExplorer();
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();