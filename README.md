# ASP.Net Identity Stores for MongoDB  <img alt="Nuget" src="https://img.shields.io/nuget/v/Nuages.AspNetIdentity.Stores.Mongo?style=flat-square">

Nuages.AspNetIdentity.Store.Mongo provide support for MongoDB as a storage mechanism for ASP.NET Core Identity.

The library is design with the following requirements in mind.

- It should work out of the box. No need to use specific IdentityUser implementation.
- No changes requires to existing IdentityUser implementations. MongoDB mapping is done using code, not attributes.
- The database schema should be as close as possible as the default Entity Framework schema. There should be a one to one maping between tables and collections. 

  - The following collections will be created
    - AspNetUsers
    - AspNetUserTokens
    - AspNetUserRoles
    - AspNetUserLogins
    - AspNetUserClaims
    - AspNetRoles
    - AspNetRoleClaims
- The schema should be well indexes to optimize search. No ToList on any collection, it always use indexed query.
- We should be able to change index creations.
- Minimal startup bootstrapping code
- Default implementation (IdentityUser and descendants) should use string as Id
- It should support all interface implemented by UserStore<> and RoleStore<>

### Restrictions:
- No support for UserOnlyStore<>. So you will always have collection for roles created.

## Installation

Add a reference to nuget package Nuages.AspNetIdentity.Store.Mongo in your project.

```csharp
dotnet add package Nuages.AspNetIdentity.Store.Mongo
```

## Basic usage with IdentityUser

Modify your Program.cs code and add a call to AddMongoStores() to your IdentityBuilder. Remove unused Entity Framework code.

Add this line.

```csharp
 builder.Services.
    AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddMongoStores(); //Connection string and database in appsettings.json
```

That's it. 

You want an Id of type ObjectId? Use MongoIdentityUser.

```csharp

 builder.Services.
    AddDefaultIdentity<MongoIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddMongoStores(); //Connection string and database in appsettings.json

```

## Use a custom IdentityUser class

Let's say you have the following IdentityUser class;

```csharp
public class MyIdentityUser: IdentityUser
{
    public string Language {get; set;}
}
```

You will need to change the configuration a bit...

```csharp
 builder.Services.
    AddDefaultIdentity<MyIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddMongoStores<MyIdentityUser>(); //Connection string and database in appsettings.json
```

## Use a custom IdentityUser class with ObjectId as Id

By default, a Guid serialized as a string is used as Id member. You may use another Id type by following those steps

First, you must inherit from MongoIdentityUser

```csharp
public class MyIdentityUser: MongoIdentityUser
{
    public string Language {get; set;}
}
```

You will need to change the configuration a bit more...

```csharp
 builder.Services.
    AddDefaultIdentity<MyIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddMongoStores<MyIdentityUser, MongoIdentityRole, ObjectId>(); //Connection string and database in appsettings.json
```


## How to customize index creation?

Indexes creation is done when the application start using  MongoSchemaInitializer<TUser, TRole, TKey> class.

You can overide the behavior by creating a new class that inherits from MongoSchemaInitializer<TUser, TRole, TKey>.

Given you are using IdentityUser and IdentityRole, the new class should look like this.

```csharp
public class MyMongoSchemaInitializer : MongoSchemaInitializer<IdentityUser,IdentityRole,string>
{
    public MyMongoSchemaInitializer(IOptions<MongoIdentityOptions> options, IOptions<IdentityOptions> identityOptions) :
        base (options, identityOptions)
    {
        
    }
    
    protected override Task CreateRolesIndexes()
    {
        return base.CreateRolesIndexes();
    }

    protected override Task CreateUsersIndexes()
    {
        return base.CreateUsersIndexes();
    }
    
    //Etc...
}
```

Then you will need to change the bootstrapping code

```csharp
builder.Services.
    AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddMongoStores<IdentityUser, IdentityUser, string, MyMongoSchemaInitializer>(); //Connection string and database in appsettings.json
```