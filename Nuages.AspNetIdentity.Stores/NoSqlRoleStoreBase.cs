using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores;

public abstract class NoSqlRoleStoreBase<TRole, TKey> : IDisposable where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private bool _disposed;
    public abstract IQueryable<TRole> Roles { get; }
   

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _disposed = true;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    [ExcludeFromCodeCoverage]
    protected void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
    }

    protected virtual TKey StringToKey(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var key = StringToKey(roleId);
        
        var role = Roles.SingleOrDefault(t => t.Id.Equals(key));

        return Task.FromResult(role)!;
    }

    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        // ReSharper disable once SpecifyStringComparison
        var role = Roles.SingleOrDefault(t => t.NormalizedName.ToUpper() == normalizedRoleName.ToUpper());

        return Task.FromResult(role)!;
    }

    public Task<string?> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.Name);
    }

    public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(role.NormalizedName);
    }

    public abstract Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = new());

    public async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        role.NormalizedName = normalizedName;

        await UpdateAsync(role, cancellationToken);
    }

    public async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        role.Name = roleName;

        await UpdateAsync(role, cancellationToken);
    }


    public abstract Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken);
}