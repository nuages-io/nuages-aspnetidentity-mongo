using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public static class AspNetIdentityMongoExtensions
{
    public static void AddMongoStores<TUser, TRole, TKey>(this IdentityBuilder builder,
        Action<MongoIdentityOptions> configure)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        builder.Services.Configure(configure);

        ModelMapper<TKey>.MapModel();
        
        AddStores(builder.Services, typeof(TUser), typeof(TRole));
        
        builder.Services.AddHostedService<MongoSchemaInitializer<TUser, TRole, TKey>>();
        
       // builder.Services.TryAddSingleton(((IHostedService) Activator.CreateInstance(typeof(MongoSchemaInitializer<,,>).MakeGenericType(typeof(TUser), typeof(TRole), typeof(TKey)))!)!);
    }
    
    // public static void AddMongoStores(this IdentityBuilder builder,
    //     Action<MongoIdentityOptions> configure)
    // {
    //     builder.Services.Configure(configure);
    //
    //     var keyType = FindGenericBaseType( builder.UserType, typeof(IdentityUser<>))!.GenericTypeArguments[0]; 
    //
    //     var t = typeof(ModelMapper<>).MakeGenericType(keyType);
    //     t.GetMethod("MapModel")!.Invoke(null, new object[]{});
    //
    //     AddStores(builder.Services, typeof(IdentityUser), typeof(IdentityRole));
    //
    //     builder.Services.TryAddSingleton( typeof(MongoSchemaInitializer<,,>).MakeGenericType(builder.UserType, builder.RoleType, keyType));
    // }
    
    private static void AddStores(IServiceCollection services, Type userType, Type? roleType)
        {
            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException("NotIdentityUser");
            }

            var keyType = identityUserType.GenericTypeArguments[0];

            if (roleType != null)
            {
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
            // else
            // {
            //     var userStoreType = typeof(MongoUserStore<,,>).MakeGenericType(userType, roleType, keyType);
            //
            //     services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            // }

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