using Bogus;
using censudex_clients_service.src.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace censudex_clients_service.src.data
{
    /// <summary>
    /// Clase responsable de poblar con datos iniciales a la base de datos.
    /// </summary>
    public class DataSeeder
    {
        /// <summary>
        /// Inicializa la base de datos con usuarios de prueba y un administrador con datos fijos.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Crear un scope para obtener los servicios necesarios.
            using var scope = serviceProvider.CreateScope();
            // Obtener los servicios del scope.
            var services = scope.ServiceProvider;
            // Obtener el contexto de datos y el UserManager.
            var context = services.GetRequiredService<DataContext>();
            // Asegurarse de que la base de datos esté creada.
            var userManager = services.GetRequiredService<UserManager<User>>();
            // Generador de datos falsos en español.
            var faker = new Faker("es");
            // Verificar si ya existen usuarios en la base de datos.
            if (!await userManager.Users.AnyAsync())
            {
                // Crear 100 usuarios de prueba.
                for (int i = 0; i < 100; i++)
                {
                    var name = faker.Name.FirstName();
                    var lastName = faker.Name.LastName();
                    // Generar un correo electrónico basado en el nombre y apellido que termine en @censudex.cl.
                    var email = $"{name.ToLower()}.{lastName.ToLower()}@censudex.cl";
                    // Verificar si el correo ya existe.
                    if (await userManager.FindByEmailAsync(email) != null)
                        continue;
                    // Crear un nuevo usuario con datos falsos.
                    var user = new User
                    {
                        FullName = $"{name} {lastName}",
                        Email = email,
                        UserName = faker.Internet.UserName(name, lastName),
                        Status = true,
                        BirthDate = DateOnly.FromDateTime(faker.Date.Past(50, DateTime.UtcNow.AddYears(-18))),
                        Address = faker.Address.FullAddress(),
                        PhoneNumber = faker.Phone.PhoneNumber("+569########"),
                        CreatedAt = DateTime.UtcNow
                    };
                    // Contraseña predeterminada para todos los usuarios de prueba.
                    string password = "Passw0rd2!";
                    // Crear el usuario en la base de datos.
                    var createUser = await userManager.CreateAsync(user, password);
                    // Manejar errores en la creación del usuario.
                    if (!createUser.Succeeded)
                    {
                        foreach (var error in createUser.Errors)
                        {
                            Console.WriteLine($"Error al crear {email}: {error.Description}");
                        }
                        continue;
                    }
                    // Asignar el rol de "Client" al usuario creado.
                    var roleResult = await userManager.AddToRoleAsync(user, "Client");
                    if (roleResult.Succeeded)
                    {
                        Console.WriteLine($"Cliente {user.Email} creado exitosamente");
                    }
                    else
                    {
                        Console.WriteLine($"Error al asignar rol a {user.Email}");
                    }
                }
                // Crear un usuario administrador con datos fijos.
                var adminEmail = "admin@censudex.cl";
                // Verificar si el administrador ya existe.
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    // Crear el usuario administrador.
                    var admin = new User
                    {
                        FullName = "Administrador Censudex",
                        Email = adminEmail,
                        UserName = "adminCensudex",
                        Status = true,
                        BirthDate = new DateOnly(2000, 1, 1),
                        Address = "Censudex Headquarters",
                        PhoneNumber = "+56912345678",
                        CreatedAt = DateTime.UtcNow
                    };
                    // Contraseña fija para el administrador.
                    var adminResult = await userManager.CreateAsync(admin, "Admin1234!");
                    // Manejar errores en la creación del administrador.
                    if (adminResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                        Console.WriteLine("Administrador creado correctamente");
                    }
                    else
                    {
                        foreach (var error in adminResult.Errors)
                        {
                            Console.WriteLine($"Error Admin: {error.Description}");
                        }
                    }
                }
            }
        }
    }
}