using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Nuages.AspNetIdentity.Stores.Mongo;
using WebAspNetIdentityWithMongo.Data;


var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.local.json",
        optional: false,
        reloadOnChange: true);
});

var useMongo = true;

if (useMongo)
{
    builder.Services.
        AddDefaultIdentity<MongoIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddMongoStores<MongoIdentityUser, MongoIdentityRole, ObjectId>(config =>
        {
            config.ConnectionString = builder.Configuration["ConnectionString"];
            config.Database =  builder.Configuration["Database"];
        });
}
else
{
    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.
        AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

    builder.Services.
        AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.
        AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();
}


builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();