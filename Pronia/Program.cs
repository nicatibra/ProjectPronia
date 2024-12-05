using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Services;
using Pronia.Services.Implementations;
using Pronia.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 8; //parolun min uzunlugu
    opt.Password.RequireNonAlphanumeric = false; //

    opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._@";//username-de istifade oluna bilecek simvollar
    opt.User.RequireUniqueEmail = true; //1 mail 1 profil ucun olur

    opt.Lockout.AllowedForNewUsers = true; //sehv giris edenden sora lock ola bilermi
    opt.Lockout.MaxFailedAccessAttempts = 4; //sehv girislerin max sayi
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3); //nece deq sonra yeniden cehd etmek olar

}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseAuthentication();//login olmaq ucun
app.UseAuthorization();//role elde etmek ucun

app.UseStaticFiles();


app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=home}/{action=index}/{id?}"
    );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=home}/{action=index}/{id?}"
    );

app.Run();
