using Microsoft.AspNetCore.Identity;
using iFood.Models;
using iFood.Data.Enum;

namespace iFood.Data
{
    public class Seed
    {
        public static void SeedData(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDBContext>();

                if(context == null)
                    return;
                context.Database.EnsureCreated();
                if(!context.Products.Any())
                {
                    context.Products.AddRange(new List<Product>()
                    {
                        new Product()
                        {
                            Name = "Mi trung",
                            Image = "https://www.eatthis.com/wp-content/uploads/sites/4/2020/05/running.jpg?quality=82&strip=1&resize=640%2C360",
                            Description ="mi chung hao han",
                            SoldOut = 0,
                            Price = 12000,
                            Quantity =2,
                        },
                        new Product()
                        {
                            Name = "Mi tom",
                            Image = "https://www.eatthis.com/wp-content/uploads/sites/4/2020/05/running.jpg?quality=82&strip=1&resize=640%2C360",
                            Description ="mi tom hao han",
                            SoldOut = 0,
                            Price = 10000,
                            Quantity =5,
                        },
                        new Product()
                        {
                            Name = "Mi goi",
                            Image = "https://www.eatthis.com/wp-content/uploads/sites/4/2020/05/running.jpg?quality=82&strip=1&resize=640%2C360",
                            Description ="mi goi haohao",
                            SoldOut = 0,
                            Price = 5000,
                            Quantity = 10,
                        }
                    });
                    context.SaveChanges();
                }
            }
        }

        public static async Task SeedUsersAndRolesAsync(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                //Roles
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                if (!await roleManager.RoleExistsAsync(UserRoles.Seller))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Seller));
                //Users
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                
                string adminUserEmail = "Admin@gmail.com";

                var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
                if (adminUser == null)
                {
                    var newAdminUser = new AppUser()
                    {
                        UserName = "Admin",
                        Email = adminUserEmail,
                        EmailConfirmed = true,
                        Address = new Address()
                        {
                            Street = "123 Main St",
                            City = "Charlotte",
                            State = "NC"
                        }
                        
                    };
                    await userManager.CreateAsync(newAdminUser, "Coding@1234");
                    await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
                }

                string appUserEmail = "user@gmail.com";

                var appUser = await userManager.FindByEmailAsync(appUserEmail);
                if (appUser == null)
                {
                    var newAppUser = new AppUser()
                    {
                        UserName = "app-user",
                        Email = appUserEmail,
                        EmailConfirmed = true,
                        Address = new Address()
                        {
                            Street = "123 Main St",
                            City = "Charlotte",
                            State = "NC"
                        }
                    };
                    await userManager.CreateAsync(newAppUser, "Coding@1234");
                    await userManager.AddToRoleAsync(newAppUser, UserRoles.User);
                }
                string sellerUserEmail = "Seller@gmail.com";

                var sellerUser = await userManager.FindByEmailAsync(sellerUserEmail);
                if (sellerUser == null)
                {
                    var newSellerUser = new AppUser()
                    {
                        UserName = "Seller",
                        Email = sellerUserEmail,
                        EmailConfirmed = true,
                        Address = new Address()
                        {
                            Street = "123 Main St",
                            City = "Charlotte",
                            State = "NC"
                        }
                        
                    };
                    await userManager.CreateAsync(newSellerUser, "Coding@1234");
                    await userManager.AddToRoleAsync(newSellerUser, UserRoles.Seller);
                }
            }
        }
    }
}