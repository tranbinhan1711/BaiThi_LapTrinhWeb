using SV22T1020536.Admin;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using SV22T1020536.Models.Common;
using System.Globalization;

static string ResolveContentRoot(string[] appArgs)
{
    for (var i = 0; i < appArgs.Length - 1; i++)
    {
        var a = appArgs[i];
        if (a is "--contentRoot" or "/contentRoot")
        {
            var p = appArgs[i + 1];
            if (Directory.Exists(p))
                return Path.GetFullPath(p);
        }
    }

    for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "Views")))
            return dir.FullName;
    }

    return Directory.GetCurrentDirectory();
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = ResolveContentRoot(args),
});

// Add services to the container.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

builder.Services.AddAuthorization();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020536.Admin"; 
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7); 
                    option.SlidingExpiration = true;
                    option.Cookie.HttpOnly = true;
                    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

// Configure Session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2); 
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var root = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
    return new ProductPhotoPathResolver(root);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

//Configure Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Configure default format
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

//Configure Application Context
ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

//Get Connection String from appsettings.json
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");

// Initialize Business Layer Configuration
SV22T1020536.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();