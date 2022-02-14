using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUserRole<TKey> : IdentityUserRole<TKey> where TKey : IEquatable<TKey>
{
    public MongoIdentityUserRole()
    {
        Id = KeyGenerator<TKey>.Generate();
    }

    public TKey Id { get; set; } 
}