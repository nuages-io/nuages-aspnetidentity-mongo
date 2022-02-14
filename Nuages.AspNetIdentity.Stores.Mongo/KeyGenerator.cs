using MongoDB.Bson;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public static class KeyGenerator<TKey>  where TKey : IEquatable<TKey>
{
    public static TKey Generate()
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