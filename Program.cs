using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using dotenv.net;
using dotenv.net.Utilities;
using censudex_clients_service.src.data;
using censudex_clients_service.src.models;
using censudex_clients_service.src.services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using CensudexUsersService.Services;
using Grpc.AspNetCore.Web;
using MassTransit;
using censudex_clients_service.src.shared;
using System.Security.Cryptography.X509Certificates;
using censudex_clients_service.src.consumers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
// Carga de variables de entorno desde el archivo .env
DotEnv.Load();

builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    options.ListenAnyIP(int.Parse(port), listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
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
SendGridService sendGridService = new SendGridService();
builder.Services.AddSingleton(sendGridService);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
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

// Añade MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registro del consumidor de mensajes de correo electrónico
    x.AddConsumer<EmailMessageConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        // Configuración del host de RabbitMQ utilizando variables de entorno
        cfg.Host(Environment.GetEnvironmentVariable("RABBITMQ_HOST")!, h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")!);
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!);
        });
        // Configuración del endpoint de recepción de mensajes
        cfg.ReceiveEndpoint("email-message-queue", e =>
        {
            // Se añade el deserializador JSON sin formato para manejar los mensajes entrantes al usar un microservicio externo
            e.UseRawJsonDeserializer();
            e.ConfigureConsumer<EmailMessageConsumer>(context);
        });
    });
});

var app = builder.Build();
app.UseCors("AllowAll");
// Aplicación de migraciones pendientes y siembra inicial de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
    _ = DataSeeder.Initialize(services);
    
}
app.UseGrpcWeb();
app.MapGrpcService<UserService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();