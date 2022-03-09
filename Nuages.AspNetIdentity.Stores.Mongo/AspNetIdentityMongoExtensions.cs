using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.AspNetIdentity.Stores.Mongo;

public static class AspNetIdentityMongoExtensions
{
    public static IdentityBuilder AddMongoStores(this IdentityBuilder builder,
        Action<MongoIdentityOptions> configure)
    {
        if (builder.UserType == typeof(MongoIdentityUser))
            return AddMongoStores<MongoIdentityUser, MongoIdentityRole, ObjectId>(builder, configure);
        else
        {
            return AddMongoStores<IdentityUser, IdentityRole, string>(builder, configure);
        }
    }

    public static IdentityBuilder AddMongoStores<TKey>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> configure)
        where TKey : IEquatable<TKey>
    {
        return AddMongoStores<MongoIdentityUser<TKey>, MongoIdentityRole<TKey>, TKey, MongoSchemaInitializer<
            MongoIdentityUser<TKey>,
            MongoIdentityRole<TKey>, TKey>>(builder, configure);
    }

    public static IdentityBuilder AddMongoStores<TUser, TRole, TKey>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> configure)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        return AddMongoStores<TUser, TRole, TKey, MongoSchemaInitializer<TUser, TRole, TKey>>(builder, configure);
    }

    public static IdentityBuilder AddMongoStores<TUser, TRole, TKey, TInitializer>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> configure)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TInitializer : MongoSchemaInitializer<TUser, TRole, TKey>
    {
        if (typeof(TUser) != builder.UserType)
            throw new Exception(
                $"Store type {typeof(TUser).FullName} does not match Identity UserType {builder.UserType.FullName} ");

        if (builder.RoleType != null && typeof(TRole) != builder.RoleType)
            throw new Exception(
                $"Role type {typeof(TRole).FullName} does not match Identity RoleType {builder.RoleType?.FullName} ");

        builder.Services.Configure(configure);

        ModelMapper<TKey>.MapModel();

        AddStores(builder.Services, typeof(TUser), typeof(TRole));

        builder.Services.AddHostedService<TInitializer>();

        return builder;
    }

    private static void AddStores(IServiceCollection services, Type userType, Type roleType)
    {
        var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
        if (identityUserType == null)
        {
            throw new InvalidOperationException("NotIdentityUser");
        }

        var keyType = identityUserType.GenericTypeArguments[0];

        var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
        if (identityRoleType == null)
        {
            throw new InvalidOperationException("NotIdentityRole");
        }

        var userStoreType = typeof(MongoUserStore<,,>).MakeGenericType(userType, roleType, keyType);
        var roleStoreType = typeof(MongoRoleStore<,>).MakeGenericType(roleType, keyType);

        services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
        services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
    }

    private static Type? FindGenericBaseType(Type currentType, Type genericBaseType)
    {
        var type = currentType;
        while (type != null)
        {
            var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericType != null && genericType == genericBaseType)
            {
                return type;
            }

            type = type.BaseType;
        }

        return null;
    }
}