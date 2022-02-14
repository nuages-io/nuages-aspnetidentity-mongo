using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
// ReSharper disable VirtualMemberCallInConstructor

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUser : MongoIdentityUser<ObjectId>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public MongoIdentityUser()
    {
        
    }
    
    // ReSharper disable once UnusedMember.Global
    public MongoIdentityUser(string userName) : this()
    {
        UserName = userName;
    }
}
public class MongoIdentityUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public MongoIdentityUser()
    {
        Id = KeyGenerator<TKey>.Generate();
        
        SecurityStamp = Guid.NewGuid().ToString();
    }

    // ReSharper disable once UnusedMember.Global
    public MongoIdentityUser(string userName) : this()
    {
        UserName = userName;
    }
    
    
}