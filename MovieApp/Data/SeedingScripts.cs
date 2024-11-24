using Microsoft.AspNetCore.Identity;

namespace MovieApp.Data
{
    public class SeedingScripts
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedingScripts(IConfiguration iConfig, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager) 
        {
            _configuration = iConfig;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedRolesAndUsersAsync()
        {

            string[] roles = { "Customer", "StoreManager" };

            // SEED ROLES
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // SEED DEFAULT ADMIN USER
            var userEmail = _configuration.GetValue<string>("AdminUser:Email");
            var userPassword = _configuration.GetValue<string>("AdminUser:Password");

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, userPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to seed user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // JOIN ADMIN TO ROLE
            if (!await _userManager.IsInRoleAsync(user, "StoreManager"))
            {
                await _userManager.AddToRoleAsync(user, "StoreManager");
            }
        }
    }
}