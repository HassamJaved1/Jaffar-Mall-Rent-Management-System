using Jaffar_Mall_Rent_Management_System.Backend_Logics;
using Jaffar_Mall_Rent_Management_System.Repositories;
using Jaffar_Mall_Rent_Management_System.Services;
using Jaffar_Mall_Rent_Management_System.Utilities;
using System.Globalization;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

// Set QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

var cultureInfo = new CultureInfo("en-PK");
cultureInfo.NumberFormat.CurrencySymbol = "PKR";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });


// Inject connection string from appsettings.json
builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new UserAuthRepository(connString!);
});

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new PropertyRepository(connString!);
});

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new TenantRepository(connString!);
});

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new LeasesRepository(connString!);
});

// Inject the service
builder.Services.AddScoped<UserAuthService>();
builder.Services.AddScoped<PropertyServices>();
builder.Services.AddScoped<TenantServices>();
builder.Services.AddScoped<LeaseServices>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new RentRepository(connString!);
});
builder.Services.AddScoped<RentServices>();

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new MaintenanceRepository(connString!);
});
builder.Services.AddScoped<MaintenanceServices>();

builder.Services.AddScoped(provider =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new DocumentRepository(connString!);
});
builder.Services.AddScoped<DocumentServices>();

builder.Services.AddScoped<DashboardServices>();

// Automated rent reminder background service (runs every 24 hours)
builder.Services.AddHostedService<RentReminderService>();

var discordWebhook = builder.Configuration["DiscordWebhookUrl"];
if (!string.IsNullOrEmpty(discordWebhook))
{
    builder.Logging.AddProvider(new DiscordLoggerProvider(discordWebhook));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
