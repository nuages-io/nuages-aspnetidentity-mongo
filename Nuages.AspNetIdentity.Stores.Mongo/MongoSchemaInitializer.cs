using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.AspNetIdentity.Stores.Mongo;

// ReSharper disable once ClassNeverInstantiated.Global
public class MongoSchemaInitializer<TUser, TRole, TKey> : IHostedService
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly Collation _caseInsensitiveCollation;
    private readonly IdentityOptions _identityOptions;

    public MongoSchemaInitializer(IOptions<MongoIdentityOptions> options, IOptions<IdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;

        var client = new MongoClient(options.Value.ConnectionString);
        var url = new MongoUrl(options.Value.ConnectionString);
        var database = client.GetDatabase(url.DatabaseName);

        RolesCollection = database.GetCollection<TRole>("AspNetRoles");
        RolesClaimsCollection = database.GetCollection<MongoIdentityRoleClaim<TKey>>("AspNetRoleClaims");
        UsersCollection = database.GetCollection<TUser>("AspNetUsers");
        UsersClaimsCollection = database.GetCollection<MongoIdentityUserClaim<TKey>>("AspNetUserClaims");
        UsersLoginsCollection = database.GetCollection<MongoIdentityUserLogin<TKey>>("AspNetUserLogins");
        UsersTokensCollection = database.GetCollection<MongoIdentityUserToken<TKey>>("AspNetUserTokens");
        UsersRolesCollection = database.GetCollection<IdentityUserRole<TKey>>("AspNetUserRoles");

        _caseInsensitiveCollation = new Collation(options.Value.Locale, strength: CollationStrength.Primary);
    }

    protected IMongoCollection<TRole> RolesCollection { get; }
    protected IMongoCollection<TUser> UsersCollection { get; }
    protected IMongoCollection<IdentityUserRole<TKey>> UsersRolesCollection { get; }
    protected IMongoCollection<MongoIdentityUserToken<TKey>> UsersTokensCollection { get; }
    protected IMongoCollection<MongoIdentityUserLogin<TKey>> UsersLoginsCollection { get; }
    protected IMongoCollection<MongoIdentityUserClaim<TKey>> UsersClaimsCollection { get; }
    protected IMongoCollection<MongoIdentityRoleClaim<TKey>> RolesClaimsCollection { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Start();
    }

    protected virtual async Task Start()
    {
        await CreateRolesIndexes();
        await CreateUsersIndexes();
        await CreateUsersRolesIndexes();
        await CreateUsersTokensIndexes();
        await CreateUsersLoginsIndexes();
        await CreateUsersClaimsIndexes();
        await CreateRolesClaimsIndexes();
    }

    [ExcludeFromCodeCoverage]
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual  async Task CreateRolesClaimsIndexes()
    {
        await RolesClaimsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityRoleClaim<TKey>>(
                Builders<MongoIdentityRoleClaim<TKey>>.IndexKeys
                    .Ascending(p => p.RoleId)
                , new CreateIndexOptions
                {
                    Name = "IX_RolesClaims_RoleId",
                    Unique = false
                })
        );

        await RolesClaimsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityRoleClaim<TKey>>(
                Builders<MongoIdentityRoleClaim<TKey>>.IndexKeys
                    .Ascending(p => p.RoleId)
                    .Ascending(p => p.Type)
                    .Ascending(p => p.Value)
                , new CreateIndexOptions
                {
                    Name = "UX_RolesClaims_RoleIdClaimTypeClaimValue",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );
    }

    protected virtual  async Task CreateUsersClaimsIndexes()
    {
        await UsersClaimsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityUserClaim<TKey>>(
                Builders<MongoIdentityUserClaim<TKey>>.IndexKeys
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_UserClaims_UserId",
                    Unique = false
                })
        );

        await UsersClaimsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityUserClaim<TKey>>(
                Builders<MongoIdentityUserClaim<TKey>>.IndexKeys
                    .Ascending(p => p.UserId)
                    .Ascending(p => p.Type)
                    .Ascending(p => p.Value)
                , new CreateIndexOptions
                {
                    Name = "UX_UserClaims_UserIdTypeValue",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );
    }

    protected virtual  async Task CreateUsersLoginsIndexes()
    {
        await UsersLoginsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityUserLogin<TKey>>(
                Builders<MongoIdentityUserLogin<TKey>>.IndexKeys
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_UserLogin_UserId",
                    Unique = false
                })
        );

        await UsersLoginsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityUserLogin<TKey>>(
                Builders<MongoIdentityUserLogin<TKey>>.IndexKeys
                    .Ascending(p => p.ProviderKey)
                    .Ascending(p => p.LoginProvider)
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "UX_UserLogin_ProviderKeyLoginProviderUserId",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );
    }

    protected virtual  async Task CreateUsersTokensIndexes()
    {
        await UsersTokensCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoIdentityUserToken<TKey>>(
                Builders<MongoIdentityUserToken<TKey>>.IndexKeys
                    .Ascending(p => p.Name)
                    .Ascending(p => p.LoginProvider)
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "UX_UserToken_NameLoginProviderUserId",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );
    }

    protected virtual  async Task CreateUsersRolesIndexes()
    {
        await UsersRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<IdentityUserRole<TKey>>(
                Builders<IdentityUserRole<TKey>>.IndexKeys
                    .Ascending(p => p.UserId)
                    .Ascending(p => p.RoleId)
                , new CreateIndexOptions
                {
                    Name = "UX_UserRole_UserIdRoleId",
                    Unique = true
                })
        );

        await UsersRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<IdentityUserRole<TKey>>(
                Builders<IdentityUserRole<TKey>>.IndexKeys
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_UserRole_UserId",
                    Unique = false
                })
        );

        await UsersRolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<IdentityUserRole<TKey>>(
                Builders<IdentityUserRole<TKey>>.IndexKeys
                    .Ascending(p => p.RoleId)
                , new CreateIndexOptions
                {
                    Name = "IX_UserRole_RoleId",
                    Unique = false
                })
        );
    }

    protected virtual  async Task CreateRolesIndexes()
    {
        await RolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TRole>(
                Builders<TRole>.IndexKeys
                    .Ascending(p => p.Name)
                , new CreateIndexOptions
                {
                    Name = "UX_Role_Name",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );

        await RolesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TRole>(
                Builders<TRole>.IndexKeys
                    .Ascending(p => p.NormalizedName)
                , new CreateIndexOptions
                {
                    Name = "UX_Role_NormalizedName",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );
    }

    protected virtual  async Task CreateUsersIndexes()
    {
        await UsersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TUser>(
                Builders<TUser>.IndexKeys
                    .Ascending(p => p.UserName)
                , new CreateIndexOptions
                {
                    Name = "UX_User_UserName",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );

        await UsersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TUser>(
                Builders<TUser>.IndexKeys
                    .Ascending(p => p.NormalizedUserName)
                , new CreateIndexOptions
                {
                    Name = "UX_User_NormalizedUserName",
                    Unique = true,
                    Collation = _caseInsensitiveCollation
                })
        );

        await UsersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TUser>(
                Builders<TUser>.IndexKeys
                    .Ascending(p => p.Email)
                , new CreateIndexOptions
                {
                    Name = "IX_User_Email",
                    Unique = _identityOptions.User.RequireUniqueEmail,
                    Collation = _caseInsensitiveCollation
                })
        );

        await UsersCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<TUser>(
                Builders<TUser>.IndexKeys
                    .Ascending(p => p.NormalizedEmail)
                , new CreateIndexOptions
                {
                    Name = "IX_User_NormalizedEmail",
                    Unique = _identityOptions.User.RequireUniqueEmail,
                    Collation = _caseInsensitiveCollation
                })
        );
    }
}