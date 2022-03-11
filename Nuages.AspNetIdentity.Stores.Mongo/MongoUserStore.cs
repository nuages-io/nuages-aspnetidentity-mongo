using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Nuages.AspNetIdentity.Stores.Mongo;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class MongoUserStore<TUser, TRole, TKey> : NoSqlUserStoreBase<TUser, TRole, TKey,
        MongoIdentityUserLogin<TKey>, MongoIdentityUserToken<TKey>, MongoIdentityUserRole<TKey>>,
    IUserClaimStore<TUser>,
    IUserLoginStore<TUser>,
    IUserRoleStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserSecurityStampStore<TUser>,
    IUserEmailStore<TUser>,
    IUserPhoneNumberStore<TUser>,
    IQueryableUserStore<TUser>,
    IUserTwoFactorStore<TUser>,
    IUserLockoutStore<TUser>,
    IUserAuthenticatorKeyStore<TUser>,
    IUserAuthenticationTokenStore<TUser>,
    IUserTwoFactorRecoveryCodeStore<TUser>,
    IProtectedUserStore<TUser>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey :  IEquatable<TKey>
{
    private readonly IdentityErrorDescriber _errorDescriber = new();
    // ReSharper disable once CollectionNeverUpdated.Local

    public MongoUserStore(IOptions<MongoIdentityOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.Database);

        UsersCollection = database.GetCollection<TUser>("AspNetUsers");
        RolesCollection = database.GetCollection<TRole>("AspNetRoles");
        UsersClaimsCollection = database.GetCollection<MongoIdentityUserClaim<TKey>>("AspNetUserClaims");
        UsersLoginsCollection = database.GetCollection<MongoIdentityUserLogin<TKey>>("AspNetUserLogins");
        UsersTokensCollection = database.GetCollection<MongoIdentityUserToken<TKey>>("AspNetUserTokens");
        UsersRolesCollection = database.GetCollection<MongoIdentityUserRole<TKey>>("AspNetUserRoles");
    }

    // ReSharper disable once StaticMemberInGenericType
    public static ReplaceOptions ReplaceOptions { get; } = new();

    // ReSharper disable once StaticMemberInGenericType
    public static DeleteOptions DeleteOptions { get; } = new();

    // ReSharper disable once StaticMemberInGenericType
    public static InsertOneOptions InsertOneOptions { get; } = new();

    private IMongoCollection<TUser> UsersCollection { get; }
    private IMongoCollection<TRole> RolesCollection { get; }
    private IMongoCollection<MongoIdentityUserClaim<TKey>> UsersClaimsCollection { get; }
    private IMongoCollection<MongoIdentityUserLogin<TKey>> UsersLoginsCollection { get; }
    private IMongoCollection<MongoIdentityUserToken<TKey>> UsersTokensCollection { get; }
    private IMongoCollection<MongoIdentityUserRole<TKey>> UsersRolesCollection { get; }
    protected override IQueryable<TRole> Roles => RolesCollection.AsQueryable();
    public IQueryable<MongoIdentityUserClaim<TKey>> UsersClaims => UsersClaimsCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserLogin<TKey>> UsersLogins => UsersLoginsCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserToken<TKey>> UsersTokens => UsersTokensCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserRole<TKey>> UsersRoles => UsersRolesCollection.AsQueryable();

    public override IQueryable<TUser> Users => UsersCollection.AsQueryable();

    protected override TKey StringToKey(string id)
    {
        if (typeof(TKey) == typeof(ObjectId))
        {
            return (TKey) (object) ObjectId.Parse(id) ;
        }

        return base.StringToKey(id);
    }
    
    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var email = await GetEmailAsync(user, cancellationToken);
        await SetNormalizedEmailAsync(user, email.ToUpper(), cancellationToken);

        var userName = await GetUserNameAsync(user, cancellationToken);
        if (string.IsNullOrEmpty(userName))
        {
            await SetUserNameAsync(user, email, cancellationToken);
            userName = email;
        }

        await SetNormalizedUserNameAsync(user, userName.ToUpper(), cancellationToken);

        await UsersCollection.InsertOneAsync(user, null, cancellationToken);

        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var currentConcurrencyStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        var result = await UsersCollection.ReplaceOneAsync(x => x.Id.Equals(user.Id) &&
                                                                x.ConcurrencyStamp.Equals(currentConcurrencyStamp),
            user, ReplaceOptions, cancellationToken);

        return ReturnUpdateResult(result);
    }

    public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var deleteResult =
            await UsersCollection.DeleteOneAsync(u => u.Id.Equals(user.Id), DeleteOptions, cancellationToken);

        return ReturnDeleteResult(deleteResult);
    }

    public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var query = UsersClaims.Where(c => c.UserId.Equals(user.Id)).Select(c =>
            new { c.Type , c.Value}
        ).ToList();

        var list = query.Select(c => new Claim(c.Type, c.Value)).ToList();
        
        return Task.FromResult((IList<Claim>)list);
    }

    public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var list = claims.Select(claim => new MongoIdentityUserClaim<TKey>
        {
            UserId = user.Id,
            Type = claim.Type,
            Value = claim.Value
        }).ToList();

        await UsersClaimsCollection.InsertManyAsync(list, null, cancellationToken);
    }

    public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var matchedClaims = UsersClaims.Where(c =>
            c.Type == claim.Type && c.Value == claim.Value && user.Id.Equals(user.Id));

        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.Value = newClaim.Value;
            matchedClaim.Type = newClaim.Type;

            await UsersClaimsCollection.ReplaceOneAsync(c => c.Id.Equals( matchedClaim.Id), matchedClaim, ReplaceOptions,
                cancellationToken);
        }
    }

    public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        foreach (var claim in claims)
            await UsersClaimsCollection.DeleteOneAsync(
                uc => uc.UserId.Equals(user.Id) && uc.Type == claim.Type && uc.Value == claim.Value, cancellationToken);
    }

    public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var query = from p in
                UsersClaims.AsQueryable().Where(u => u.Type == claim.Type)
            join o in Users on p.UserId equals o.Id
            select o;

        return Task.FromResult((IList<TUser>)query.ToList());
    }

    public async Task AddLoginAsync(TUser user, UserLoginInfo loginInfo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var login = new MongoIdentityUserLogin<TKey>
        {
            UserId = user.Id,
            ProviderKey = loginInfo.ProviderKey,
            LoginProvider = loginInfo.LoginProvider,
            ProviderDisplayName = loginInfo.ProviderDisplayName
        };

        await UsersLoginsCollection.InsertOneAsync(login, InsertOneOptions, cancellationToken);
    }

    public async Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.SecurityStamp = stamp;

        await UpdateAsync(user, cancellationToken);
    }

    public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.SecurityStamp);
    }

    protected override Task<TRole?> FindRoleByNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        // ReSharper disable once SpecifyStringComparison
        return Task.FromResult(Roles.SingleOrDefault(r => r.NormalizedName.ToUpper() == normalizedName.ToUpper()));
    }

    protected override async Task AddUserTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        //token.Id = KeyGenerator<TKey>.Generate();
        await UsersTokensCollection.InsertOneAsync(token);
    }

    protected override async Task UpdateUserTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        await UsersTokensCollection.ReplaceOneAsync(t => t.Id.Equals(token.Id), token);
    }

    protected override async Task RemoveTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        await UsersTokensCollection.DeleteOneAsync(t => t.Id.Equals(token.Id));
    }

    protected override async Task RemoveUserRoleAsync(MongoIdentityUserRole<TKey> role)
    {
        await UsersRolesCollection.DeleteOneAsync(r => r.Id.Equals(role.Id));
    }

    protected override async Task AddUserRoleAsync(MongoIdentityUserRole<TKey> role)
    {
        await UsersRolesCollection.InsertOneAsync(role);
    }

    protected override async Task RemoveUserLoginAsync(MongoIdentityUserLogin<TKey> login)
    {
        await UsersLoginsCollection.DeleteOneAsync(l => l.Id.Equals(login.Id));
    }

    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnUpdateResult(ReplaceOneResult result)
    {
        if (!result.IsAcknowledged || result.ModifiedCount == 0)
        {
            return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnDeleteResult(DeleteResult result)
    {
        if (result.IsAcknowledged || result.DeletedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }
}