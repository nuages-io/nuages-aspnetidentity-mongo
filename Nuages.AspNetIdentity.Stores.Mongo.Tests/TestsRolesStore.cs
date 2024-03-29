using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Nuages.AspNetIdentity.Stores.Mongo.Tests;

[Collection("Mongo")]

#nullable disable

public class TestsRolesStore
{
    private readonly MongoRoleStore<IdentityRole<string>, string> _roleStore;

    public TestsRolesStore()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .AddEnvironmentVariables()
            .Build();
        
        var options = new MongoIdentityOptions
        {
            ConnectionString = configuration["ConnectionString"],
            Locale = "en"
        };
        
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var identityBuilder = new IdentityBuilder(typeof(IdentityUser<ObjectId>), typeof(IdentityRole<ObjectId>), serviceCollection);
        identityBuilder.AddMongoStores<IdentityUser<ObjectId>, IdentityRole<ObjectId>, ObjectId>(configure =>
        {
            configure.ConnectionString = options.ConnectionString;
        });
        
        var client = new MongoClient(options.ConnectionString);

        var url = new MongoUrl(options.ConnectionString);
        client.DropDatabase(url.DatabaseName);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
       
        Task.Run(async () =>
        {
            var service = serviceProvider
                    .GetRequiredService<IHostedService>() as
                MongoSchemaInitializer<IdentityUser<string>, IdentityRole<string>, string>;

            await service!.StartAsync(CancellationToken.None);
        });

        _roleStore = new MongoRoleStore<IdentityRole<string>, string>(Options.Create(options));
    }

    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var role = new IdentityRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        await _roleStore.UpdateAsync(role, CancellationToken.None);

        var name = await _roleStore.GetRoleNameAsync(role, CancellationToken.None);
        Assert.Equal(role.Name, name);
    }

    [Fact]
    public async Task ShouldDeleteWithSuccess()
    {
        var role = new MongoIdentityRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        var id = await _roleStore.GetRoleIdAsync(role, CancellationToken.None);
        Assert.NotNull(id);
        Assert.NotNull(await _roleStore.FindByIdAsync(id!, CancellationToken.None));

        await _roleStore.DeleteAsync(role, CancellationToken.None);

        Assert.Null(await _roleStore.FindByIdAsync(id!, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldFindByNameWithSuccess()
    {
        var role = new MongoIdentityRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);


        var name = await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None);

        Assert.NotNull(await _roleStore.FindByNameAsync(name, CancellationToken.None));
        const string newName = "NewName";

        await _roleStore.SetRoleNameAsync(role, newName, CancellationToken.None);
        await _roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper(), CancellationToken.None);

        Assert.NotNull(await _roleStore.FindByNameAsync(newName, CancellationToken.None));
        Assert.NotNull(await _roleStore.FindByNameAsync(newName.ToUpper(), CancellationToken.None));
    }


    [Fact]
    public async Task ShouldAddClaimsWithSuccess()
    {
        var role = new MongoIdentityRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        var claim = new Claim("name", "value");

        await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);

        var claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);

        Assert.Single(claims);

        await _roleStore.RemoveClaimAsync(role, claim, CancellationToken.None);

        claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);

        Assert.Empty(claims);
    }
}