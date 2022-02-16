namespace Nuages.AspNetIdentity.Stores.Mongo;

public class MongoIdentityRoleClaim<TKey>  where TKey : IEquatable<TKey>
{
    public MongoIdentityRoleClaim()
    {
        Id = KeyGenerator<TKey>.Generate();
    }
    
    public TKey Id { get; set; } 
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";

    public TKey RoleId { get; set; } = default!;
}