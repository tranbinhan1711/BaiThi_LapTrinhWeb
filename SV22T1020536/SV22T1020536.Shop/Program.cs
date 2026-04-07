using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using SV22T1020536.Models.Common;

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
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");
SV22T1020536.BusinessLayers.Configuration.Initialize(connectionString);

app.Run();
