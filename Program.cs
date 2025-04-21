using Microsoft.EntityFrameworkCore;
using MediumMvc.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using MediumMvc.Services;
using MediumMvc.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found."); ;

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseSqlite(connectionString,
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
);

// builder.Services.AddDbContext<ApplicationDbContext>(options => options.
// UseSqlite(connectionString).
// ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Seeders
builder.Services.AddScoped<Seeder>();
builder.Services.AddScoped<AuthorSeeder>();
builder.Services.AddScoped<PostSeeder>();
builder.Services.AddScoped<UserSeeder>();

builder.Services.AddAuthorization(
    options => AuthorizationPolicies.AddPolicies(options)
);

builder.Services.AddScoped<IAuthorizationHandler, PostAuthorizationHandler>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseStaticFiles();
}
else
{
    app.UseStaticFiles(new StaticFileOptions
    {
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream"
    });
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
    await seeder.SeedAsync();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
