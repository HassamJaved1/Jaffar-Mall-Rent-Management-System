using Jaffar_Mall_Rent_Management_System.Backend_Logics;
using Jaffar_Mall_Rent_Management_System.Repositories;
using Jaffar_Mall_Rent_Management_System.Services;
using Jaffar_Mall_Rent_Management_System.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
