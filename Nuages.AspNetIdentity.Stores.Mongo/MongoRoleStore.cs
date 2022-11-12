using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.AspNetIdentity.Stores.Mongo;

#nullable disable
// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class MongoRoleStore<TRole, TKey> : NoSqlRoleStoreBase<TRole, TKey>,
    IRoleClaimStore<TRole>,
    IQueryableRoleStore<TRole>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly IdentityErrorDescriber _errorDescriber = new();

    public MongoRoleStore(IOptions<MongoIdentityOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);

        var url = new MongoUrl(options.Value.ConnectionString);
        var database = client.GetDatabase(url.DatabaseName);

        RolesCollection = database.GetCollection<TRole>("AspNetRoles");
        RoleClaimsCollection = database.GetCollection<MongoIdentityRoleClaim<TKey>>("AspNetRoleClaims");
    }

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once StaticMemberInGenericType
    public static ReplaceOptions ReplaceOptions { get; } = new();

    // ReSharper disable once StaticMemberInGenericType
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static DeleteOptions DeleteOptions { get; } = new();

    private IMongoCollection<TRole> RolesCollection { get; }
    private IMongoCollection<MongoIdentityRoleClaim<TKey>> RoleClaimsCollection { get; }
    protected IQueryable<MongoIdentityRoleClaim<TKey>> RolesClaims => RoleClaimsCollection.AsQueryable();

    public override IQueryable<TRole> Roles => RolesCollection.AsQueryable();

    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        await SetNormalizedRoleNameAsync(role, role.Name.ToUpper(), cancellationToken);

        await RolesCollection.InsertOneAsync(role, null, cancellationToken);

        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var replaceOneResult =
            await RolesCollection.ReplaceOneAsync(r => r.Id.Equals(role.Id), role, ReplaceOptions, cancellationToken);

        return ReturnUpdateResult(replaceOneResult);
    }

    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var result = await RolesCollection.DeleteOneAsync(m => m.Id.Equals(role.Id), DeleteOptions, cancellationToken);

        return ReturnDeleteResult(result);
    }
    
    public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        await RoleClaimsCollection.InsertOneAsync(new MongoIdentityRoleClaim<TKey>
        {
            RoleId = role.Id,
            Type = claim.Type,
            Value = claim.Value
        }, null, cancellationToken);
    }

    public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new())
    {
        var list = RolesClaims.Where(c => c.RoleId.Equals(role.Id)).Select(c =>
            new Claim(c.Type, c.Value)
        ).ToList();

        return Task.FromResult((IList<Claim>)list);
    }
    
    public async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = new())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var entity =
            RoleClaimsCollection.AsQueryable().FirstOrDefault(
                uc => uc.RoleId.Equals(role.Id) && uc.Type == claim.Type && uc.Value == claim.Value);
        if (entity != null)
            await RoleClaimsCollection.DeleteOneAsync(c => c.Id.Equals(entity.Id), DeleteOptions, cancellationToken);
    }

    protected override TKey StringToKey(string id)
    {
        if (typeof(TKey) == typeof(ObjectId))
        {
            return (TKey) (object) ObjectId.Parse(id) ;
        }

        return base.StringToKey(id);
    }
    
    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnUpdateResult(ReplaceOneResult replaceOneResult)
    {
        if (replaceOneResult.IsAcknowledged || replaceOneResult.ModifiedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }

    [ExcludeFromCodeCoverage]
    private IdentityResult ReturnDeleteResult(DeleteResult result)
    {
        if (result.IsAcknowledged || result.DeletedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }
    
    
}