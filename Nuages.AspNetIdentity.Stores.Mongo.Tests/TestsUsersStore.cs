using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Nuages.AspNetIdentity.Stores.Mongo.Tests;

[Collection("Mongo")]
public class TestsUsersStore
{
    private readonly MongoRoleStore<IdentityRole<string>, string> _roleStore;

    private readonly MongoUserStore<IdentityUser<string>, IdentityRole<string>, string>
        _userStore;

    public TestsUsersStore()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();
        
        var options = new MongoIdentityOptions
        {
            ConnectionString = configuration["ConnectionString"],
            Database = configuration["Database"],
            Locale = "en"
        };
        
        ModelMapper.MapModel<string>();
        
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var identityBuilder = new IdentityBuilder(typeof(IdentityUser<string>), typeof(IdentityRole<string>), serviceCollection);
        identityBuilder.AddMongoStores<IdentityUser<string>, IdentityRole<string>, string>(configure =>
        {
            configure.ConnectionString = options.ConnectionString;
            configure.Database = options.Database;
        });
        
        var client = new MongoClient(options.ConnectionString);
        client.DropDatabase(options.Database);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
       
        Task.Run(async () =>
        {
            var service = serviceProvider
                    .GetRequiredService<IHostedService>() as
                MongoSchemaInitializer<IdentityUser<string>, IdentityRole<string>, string>;

            await service!.StartAsync(CancellationToken.None);
        });

       

        _roleStore = new MongoRoleStore<IdentityRole<string>, string>(Options.Create(options));
        _userStore =
            new MongoUserStore<IdentityUser<string>, IdentityRole<string>, string>(
                Options.Create(options));
    }

    private async Task<IdentityUser<string>> CreateDefaultUser()
    {
        const string email = "user@example.com";

        var user = new IdentityUser<string>
        {
            Email = email,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper()
        };

        var res = await _userStore.CreateAsync(user, CancellationToken.None);

        Assert.True(res.Succeeded);

        return user;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private async Task<IdentityUser<string>> CreateDefaultUser2()
    {
        var user = new IdentityUser<string>
        {
            Email = "user2@example.com"
        };

        var res = await _userStore.CreateAsync(user, CancellationToken.None);

        Assert.True(res.Succeeded);


        return user;
    }

    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var user = await CreateDefaultUser();

        Assert.NotNull(await _userStore.FindByEmailAsync(user.Email, CancellationToken.None));
        Assert.NotNull(await _userStore.FindByIdAsync(user.Id, CancellationToken.None));

        await _userStore.SetSecurityStampAsync(user, "stamp", CancellationToken.None);
        Assert.Equal("stamp", await _userStore.GetSecurityStampAsync(user, CancellationToken.None));

        user = await _userStore.FindByNameAsync(user.UserName, CancellationToken.None);
        Assert.NotNull(user);
        var id = await _userStore.GetUserIdAsync(user, CancellationToken.None);
        Assert.Equal(user.Id, id);

        await _userStore.DeleteAsync(user, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Null(user);
    }

    [Fact]
    public async Task ShouldIncrementAccessFailedCount()
    {
        var user = await CreateDefaultUser();

        Assert.Equal(0, await _userStore.GetAccessFailedCountAsync(user, CancellationToken.None));

        Assert.Equal(1, await _userStore.IncrementAccessFailedCountAsync(user, CancellationToken.None));

        user = await ReloadAsync(user);

        Assert.Equal(1, await _userStore.GetAccessFailedCountAsync(user, CancellationToken.None));

        await _userStore.ResetAccessFailedCountAsync(user, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(0, await _userStore.GetAccessFailedCountAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetEmailWithSuccess()
    {
        var user = await CreateDefaultUser();

        const string newUserName = "test2@example.com";

        await _userStore.SetEmailAsync(user, newUserName, CancellationToken.None);
        await _userStore.SetNormalizedEmailAsync(user, newUserName.ToUpper(), CancellationToken.None);

        await _userStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(newUserName, await _userStore.GetEmailAsync(user, CancellationToken.None));

        Assert.True(await _userStore.GetEmailConfirmedAsync(user, CancellationToken.None));

        Assert.Equal(user.Email.ToUpper(), await _userStore.GetNormalizedEmailAsync(user, CancellationToken.None));
        Assert.Equal(user.UserName.ToUpper(),
            await _userStore.GetNormalizedUserNameAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetPhoneNumberWithSuccess()
    {
        var user = await CreateDefaultUser();

        const string newPhoneNUmber = "9999999999";

        await _userStore.SetPhoneNumberAsync(user, newPhoneNUmber, CancellationToken.None);
        await _userStore.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(newPhoneNUmber, await _userStore.GetPhoneNumberAsync(user, CancellationToken.None));
        Assert.True(await _userStore.GetPhoneNumberConfirmedAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetTwoFactorEnabledWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _userStore.SetTwoFactorEnabledAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _userStore.GetTwoFactorEnabledAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetLockoutEnabledWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _userStore.SetLockoutEnabledAsync(user, true, CancellationToken.None);
        await _userStore.SetLockoutEndDateAsync(user, DateTimeOffset.Now, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _userStore.GetLockoutEnabledAsync(user, CancellationToken.None));
        Assert.NotNull(await _userStore.GetLockoutEndDateAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetPasswordHashWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _userStore.SetPasswordHashAsync(user, "hash", CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _userStore.HasPasswordAsync(user, CancellationToken.None));
        Assert.NotNull(await _userStore.GetPasswordHashAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldReplaceCodesWithSuccess()
    {
        var user = await CreateDefaultUser();

        Assert.Equal(0, await _userStore.CountCodesAsync(user, CancellationToken.None));

        var codes = new[] { "a", "b", "c" };

        await _userStore.ReplaceCodesAsync(user, codes, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(3, await _userStore.CountCodesAsync(user, CancellationToken.None));

        Assert.False(await _userStore.RedeemCodeAsync(user, "bad_code", CancellationToken.None));
        Assert.True(await _userStore.RedeemCodeAsync(user, codes.First(), CancellationToken.None));

        user = await ReloadAsync(user);

        Assert.Equal(2, await _userStore.CountCodesAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetAuthenticatorWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _userStore.SetAuthenticatorKeyAsync(user, "key", CancellationToken.None);

        user = await _userStore.FindByIdAsync(user.Id, CancellationToken.None);

        Assert.Equal("key", await _userStore.GetAuthenticatorKeyAsync(user, CancellationToken.None));

        await _userStore.RemoveTokenAsync(user,
            AuthenticatorInfo.AuthenticatorStoreLoginProvider,
            AuthenticatorInfo.AuthenticatorKeyTokenName,
            CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Null(await _userStore.GetAuthenticatorKeyAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldAddCLaimWithSuccess()
    {
        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        var claim = new Claim("claim", "value");

        await _userStore.AddClaimsAsync(user, new[] { claim }, CancellationToken.None);

        user = await ReloadAsync(user);

        var claims = await _userStore.GetClaimsAsync(user, CancellationToken.None);
        Assert.Equal(1, claims.Count);

        var users = await _userStore.GetUsersForClaimAsync(claim, CancellationToken.None);

        Assert.Equal(1, users.Count);

        var claim2 = new Claim("claim2", "value2");

        await _userStore.ReplaceClaimAsync(user, claim, claim2, CancellationToken.None);

        users = await _userStore.GetUsersForClaimAsync(claim, CancellationToken.None);

        Assert.Equal(0, users.Count);

        users = await _userStore.GetUsersForClaimAsync(claim2, CancellationToken.None);

        Assert.Equal(1, users.Count);

        await _userStore.RemoveClaimsAsync(user, new[] { claim2 }, CancellationToken.None);

        user = await ReloadAsync(user);

        claims = await _userStore.GetClaimsAsync(user, CancellationToken.None);
        Assert.Equal(0, claims.Count);
    }

    private async Task<IdentityUser<string>> ReloadAsync(IdentityUser<string> user)
    {
        return await _userStore.FindByIdAsync(user.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ShouldAddToRoleWithSuccess()
    {
        const string roleName = "Role";

        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        await _roleStore.CreateAsync(new IdentityRole<string>
        {
            Name = roleName
        }, CancellationToken.None);

        await _userStore.AddToRoleAsync(user, roleName, CancellationToken.None);

        user = await ReloadAsync(user);

        var roles = await _userStore.GetRolesAsync(user, CancellationToken.None);

        Assert.Equal(1, roles.Count);

        Assert.True(await _userStore.IsInRoleAsync(user, roleName, CancellationToken.None));
        Assert.False(await _userStore.IsInRoleAsync(user, "bad_role", CancellationToken.None));

        var users = await _userStore.GetUsersInRoleAsync(roleName.ToUpper(), CancellationToken.None);

        Assert.Equal(1, users.Count);

        Assert.Equal(0, (await _userStore.GetUsersInRoleAsync("bad_role", CancellationToken.None)).Count);

        await _userStore.RemoveFromRoleAsync(user, roleName, CancellationToken.None);

        roles = await _userStore.GetRolesAsync(user, CancellationToken.None);

        Assert.Equal(0, roles.Count);
    }

    [Fact]
    public async Task ShouldAddLoginWithSuccess()
    {
        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        await _userStore.AddLoginAsync(user, new UserLoginInfo("provider", "key", "display"),
            CancellationToken.None);

        user = await ReloadAsync(user);

        var logins = await _userStore.GetLoginsAsync(user, CancellationToken.None);
        Assert.Equal(1, logins.Count);

        Assert.NotNull(await _userStore.FindByLoginAsync("provider", "key", CancellationToken.None));
        Assert.Null(await _userStore.FindByLoginAsync("provider", "bad_key", CancellationToken.None));

        await _userStore.RemoveLoginAsync(user, "provider", "key", CancellationToken.None);

        user = await ReloadAsync(user);

        logins = await _userStore.GetLoginsAsync(user, CancellationToken.None);
        Assert.Equal(0, logins.Count);
    }
}