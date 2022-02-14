using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

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
        // var keyType = typeof(TKey);

        IIdGenerator? idGenerator = null;
        // if (keyType == typeof(Guid))
        // {
        //     idGenerator = GuidGenerator.Instance;
        // }
        // else
        // {
        //     if (keyType == typeof(ObjectId))
        //     {
        //         idGenerator = ObjectIdGenerator.Instance;
        //     }
        // }
        
        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserClaim<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserClaim<TKey>>(cm =>
            {
                cm.AutoMap();
                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
                
                var userId = cm.MapMember(c => c.UserId);
                if (idGenerator != null)
                {
                    userId.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserLogin<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();
                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserLogin<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();

                var userId = cm.MapMember(c => c.UserId);
                if (idGenerator != null)
                {
                    userId.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserRole<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserRole<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                var userId = cm.MapMember(c => c.UserId);
                var roleId = cm.MapMember(c => c.RoleId);
                if (idGenerator != null)
                {
                    userId.SetIdGenerator(idGenerator);
                    roleId.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityRole<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityRole<TKey>>(cm =>
            {
                cm.AutoMap();
                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserToken<TKey>)))
            BsonClassMap.RegisterClassMap<MongoIdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserToken<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                var userId = cm.MapMember(c => c.UserId);
                if (idGenerator != null)
                {
                    userId.SetIdGenerator(idGenerator);
                }
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUser<TKey>)))
            BsonClassMap.RegisterClassMap<IdentityUser<TKey>>(cm =>
            {
                cm.AutoMap();

                var id = cm.MapIdMember(c => c.Id);
                if (idGenerator != null)
                {
                    id.SetIdGenerator(idGenerator);
                }
            });
    }
}