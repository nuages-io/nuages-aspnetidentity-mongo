using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public static class ModelMapper<TKey>
    where TKey : IEquatable<TKey>
{
    public static void MapModel()
    {
        InternalMapModel();
    }

    private static void InternalMapModel()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserClaim<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserClaim<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.UserId);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserLogin<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserLogin<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserRole<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserRole<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId);
                cm.MapMember(c => c.RoleId);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityRole<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserToken<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserToken<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId);
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUser<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUser<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });
    }
}