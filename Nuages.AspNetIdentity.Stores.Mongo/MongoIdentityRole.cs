using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityRole : MongoIdentityRole<ObjectId>
{
    public MongoIdentityRole()
    {
        
    }
    
    public MongoIdentityRole(string roleName) : this()
    {
        Name = roleName;
    }
}

public class MongoIdentityRole<TKey> : IdentityRole<TKey>  where TKey : IEquatable<TKey>
{
    public MongoIdentityRole()
    {
        Id = GenerateNewKey();
    }
    
    public MongoIdentityRole(string roleName) : this()
    {
        Name = roleName;
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