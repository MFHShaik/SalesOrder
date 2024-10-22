using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SalesOrders.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using SalesOrders.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Set the default culture for the application
var cultureInfo = new CultureInfo("en-US"); // Use "en-US" for dot as decimal separator
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
};

builder.Services.AddControllersWithViews(config =>
{
    // Apply global authorization filter so all pages require authentication by default
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Add ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Email confirmation setting
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // Add token providers for reset password, etc.

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Path for login
    options.AccessDeniedPath = "/Account/AccessDenied"; // Path for access denied
});

// Register the OrderService
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddHttpClient();   

// Build the app
var app = builder.Build();

// Use localization
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // Enable authentication middleware
app.UseAuthorization();   // Enable authorization middleware

// Set default route to redirect to Login page for unauthenticated users
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Call method to seed the default admin and additional user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedUsersAsync(services); // Ensure admin and regular users are seeded
}

app.Run();

// Method to seed an admin user and a regular user
static async Task SeedUsersAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var adminRole = "Admin";
    var userRole = "User"; // Define a regular user role

    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    if (!await roleManager.RoleExistsAsync(userRole))
    {
        await roleManager.CreateAsync(new IdentityRole(userRole));
    }

    // Create the admin user if it doesn't exist
    var adminEmail = "admin@admin.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new IdentityUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true // Set to true if email confirmation is not required
        };

        var result = await userManager.CreateAsync(user, "Admin@1234"); // Ensure a strong password

        if (result.Succeeded)
        {
            // Add the admin user to the Admin role
            await userManager.AddToRoleAsync(user, adminRole);
        }
    }

    // Create a regular user if it doesn't exist
    var regularUserEmail = "user1@user.com";
    var regularUser = await userManager.FindByEmailAsync(regularUserEmail);

    if (regularUser == null)
    {
        var user = new IdentityUser
        {
            UserName = "user1",
            Email = regularUserEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "User@1234");

        if (result.Succeeded)
        {
            // Add the regular user to the User role
            await userManager.AddToRoleAsync(user, userRole);
        }
    }
}
