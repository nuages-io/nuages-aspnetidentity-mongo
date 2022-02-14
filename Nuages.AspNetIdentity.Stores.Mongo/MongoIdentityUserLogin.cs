using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityUserLogin<TKey> : IdentityUserLogin<TKey> where TKey : IEquatable<TKey>
{
    public MongoIdentityUserLogin()
    {
        Id = KeyGenerator<TKey>.Generate();
    }

    public TKey Id { get; set; } 
}