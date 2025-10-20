using Bogus;
using censudex_clients_service.src.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace censudex_clients_service.src.data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<DataContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
                
            var faker = new Faker("es");

            if (!await userManager.Users.AnyAsync())
            {
                for (int i = 0; i < 100; i++)
                {
                    var name = faker.Name.FirstName();
                    var lastName = faker.Name.LastName();
                    var email = $"{name.ToLower()}.{lastName.ToLower()}@censudex.cl";

                    if (await userManager.FindByEmailAsync(email) != null)
                        continue;

                    var user = new User
                    {
                        FullName = $"{name} {lastName}",
                        Email = email,
                        UserName = faker.Internet.UserName(name, lastName),
                        Status = true,
                        BirthDate = DateOnly.FromDateTime(faker.Date.Past(50, DateTime.UtcNow.AddYears(-18))),
                        Address = faker.Address.FullAddress(),
                        PhoneNumber = faker.Phone.PhoneNumber("9########"),
                        CreatedAt = DateTime.UtcNow
                    };
                    string password = "Passw0rd2!";

                    var createUser = await userManager.CreateAsync(user, password);

                    if (!createUser.Succeeded)
                    {
                        foreach (var error in createUser.Errors)
                        {
                            Console.WriteLine($"Error al crear {email}: {error.Description}");
                        }
                        continue;
                    }

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

                var adminEmail = "admin@censudex.cl";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var admin = new User
                    {
                        FullName = "Administrador Censudex",
                        Email = adminEmail,
                        UserName = "adminCensudex",
                        Status = true,
                        BirthDate = new DateOnly(2000, 1, 1),
                        Address = "Censudex Headquarters",
                        PhoneNumber = "912345678",
                        CreatedAt = DateTime.UtcNow
                    };

                    var adminResult = await userManager.CreateAsync(admin, "Admin1234!");
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