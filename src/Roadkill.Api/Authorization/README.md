# Authorization in Roadkill

As Identity, Authentication, and Authorization  are a backbone of the Roadkill API it is worthy of its own README. 
This has some details on how it all hangs together but won't go into implementation details unless needed.

## Identity

Identity is the subsystem for finding users, groups, roles and claims. This is done via the ASP.NET Core 
Identity package. This package gives you a UserManager, RoleManager, SigninManager.

It also handles sign-ins in conjunction with JWT described below.

Sitting on top of Microsoft.AspNetCore.Identity is a custom package written for Roadkill, called 
[Marten.AspNetIdentity](https://github.com/roadkillwiki/Marten.AspNetIdentity). As Roadkill uses 
Marten for its datastore (which uses Postgres), this package uses the same connection string and Marten 
as the data store for the Microsoft.AspNetCore.Identity, storing all its users and claims inside the 
Postgres-based document store.

## Authorization

Authorization in responsible for checking if a user exists and their password is correct. 
In ASP.NET Core this is a piece of middleware. In Roadkill the API uses JWT with Bearer authentication 
for its authorization. This is is simply the "Authorization" HTTP header, with a value of "Bearer <jwt token>".

### JWT

#### A brief introduction
JWT is [JSON Web Tokens](https://jwt.io/introduction/). You generate a Base64'd string that contains JSON. 
Inside the JSON are three sections: header, payload, signature. The set of properties about the user, 
known as the payload, contain the user email and their role.

This token is tamper proof, because it's generated on the server using symmetrical encryption (HMAC Sha256). 
Symmetrical encryption is simply key based encryption, this key is set in Roadkill in the settings file.

The JWT token signature section is an encrypted signature of the payload, so the server can check the JWT token 
is correct.

#### Implementation

JWT is built into ASP.NET Core via its inbuilt Dependency Injection package, and `AddJwtBearer`. This ties into 
`IApplicationBuilder.UseAuthentication()` - the token is read using a Middleware implementation inside ASP.NET Core. 

### Policy-based authorization

A lot of time was spent building a scalable policy-based role system into Roadkill. It uses [policy-based authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1#authorization-handlers) that's built into .NET Core 3.x

Here's a brief explanation:

- A set of JWT roles, or policies are [declared](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Extensions/ServiceCollectionExtensions.cs#L120)
- These policies all use a [RoadkillRequirement](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Policies/RoadkillPolicyRequirement.cs)
- ASP.NET will use Roadkill's [custom AuthorizationHandler](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Roles/RolesAuthorizationHandler.cs) to handle the RoadkillRequirement that each policy uses.
  - This handler has all `IUserRoleDefinition` injected into it. It goes through each of these definitions, to see if the incoming role name, such as "admin" has the claim, or policy name in the IUserRoleDefinition. It succeeds if it does.
- Each controller action is decorated with an `[Authorize]` attribute, or to be exact a [custom attribute](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Roles/AuthorizeWithBearerAttribute.cs) that sets the scheme to be Bearer otherwise ASP.NET uses "Basic". For example to create an admin use: `[Authorize(Policy = PolicyNames.CreateAdminUser)]`

There are only Editor and Admin role definitions for now, but it is trivial to create more in future. Definitions could even be retrieved from the database, however PolicyNames will always be part of the C# code.
