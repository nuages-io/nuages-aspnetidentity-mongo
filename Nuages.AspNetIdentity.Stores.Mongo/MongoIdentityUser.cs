using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUser : MongoIdentityUser<ObjectId>
{
    public MongoIdentityUser()
    {
        
    }
    
    public MongoIdentityUser(string userName) : this()
    {
        UserName = userName;
    }
}
public class MongoIdentityUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public MongoIdentityUser()
    {
        Id = GenerateNewKey();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public MongoIdentityUser(string userName) : this()
    {
        UserName = userName;
    }
    
    TKey GenerateNewKey()
    {
        object? newId;

        var keyType = typeof(TKey);
        if (keyType == typeof(Guid))
        {
            newId = Guid.NewGuid();
        }
        else
        {
            if (keyType == typeof(ObjectId))
            {
                newId = ObjectId.GenerateNewId();
            }
            else
            {
                newId = Guid.NewGuid().ToString();
            }
        }

        return (TKey)newId;
    }
    
}