using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
// ReSharper disable VirtualMemberCallInConstructor

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityRole : MongoIdentityRole<ObjectId>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public MongoIdentityRole()
    {
        
    }
    
    // ReSharper disable once UnusedMember.Global
    public MongoIdentityRole(string roleName) : this()
    {
        Name = roleName;
    }
}

public class MongoIdentityRole<TKey> : IdentityRole<TKey>  where TKey : IEquatable<TKey>
{
    public MongoIdentityRole()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Id = KeyGenerator<TKey>.Generate();
    }
    
    // ReSharper disable once UnusedMember.Global
    public MongoIdentityRole(string roleName) : this()
    {
        Name = roleName;
    }
}