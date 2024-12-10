using ASPNetCoreMVCSample.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsAggregator.Web.Models.Domain;
using NewsAggregator.Web.Services;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure EF Core with SQL Server and connection string from appsettings.json
builder.Services.AddDbContext<NewsAggregateDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NewsAggregateDbConnectionString")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<NewsAggregateDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(option =>
{
    // Default Settings
    option.Password.RequireDigit = true;
    option.Password.RequireLowercase = true;
    option.Password.RequireUppercase = true;
    option.Password.RequiredLength = 6;
    option.Password.RequireNonAlphanumeric = true;
    option.Password.RequiredUniqueChars = 1;
});

// Retrieve the API key from the appsettings.json or environment variables
var apiKey = builder.Configuration["NewsApiSettings:ApiKey"];

// Configure HttpClient for NewsService
builder.Services.AddHttpClient<NewsService>((sp, client) =>
{
    client.BaseAddress = new Uri("https://newsapi.org/v2/");
    client.DefaultRequestHeaders.Add("User-Agent", "NewsAggregatorApp/1.0"); // Set your User-Agent header
    client.DefaultRequestHeaders.Add("x-api-key", apiKey); // Add API key in header
});

// Register EmailService and ArticleService
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IArticleService, ArticleService>();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "SuperAdmin"));
});

// Add Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles();
app.UseRouting();

// Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Set up the default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages
app.MapRazorPages();

app.Run();
