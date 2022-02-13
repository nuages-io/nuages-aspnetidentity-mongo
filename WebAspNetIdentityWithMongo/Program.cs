using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nuages.AspNetIdentity.Stores.Mongo;
using WebAspNetIdentityWithMongo.Data;

var builder = WebApplication.CreateBuilder(args);

var useMongo = true;

if (useMongo)
{
    builder.Services.
        AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddMongoStores<IdentityUser, IdentityRole, string>(config =>
        {
            config.ConnectionString = "mongodb+srv://nuages:wCFwlSoX4qK200E1@nuages-dev-2.qxak3.mongodb.net/nuages_identity?retryWrites=true&w=majority";
            config.Database = "nuages_identity_sample";
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