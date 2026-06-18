using Task_Management_Project.Helpers;
using Task_Management_Project.Models;

namespace Task_Management_Project.Data
{
    public static class DBSeeder
    {
        public static void SeedData(Context context)
        {
            SeedAdminUser(context);

        }

        private static void SeedAdminUser(Context context)
        {
            if (!context.Users.Any(u => u.RoleName == "Admin"))
            {
                var admin = new User
                {
                    Username = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = PasswordHasher.HashPassword("123456"),
                    RoleName = "Admin",
                    IsActive = true
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}