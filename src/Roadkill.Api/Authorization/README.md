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

#### Refresh tokens

Roadkill uses a refresh token to avoid the client from having to continually login when the short-lived JWT token expires.

- When a client logins in, their JWT token is stored in the database along with a Guid refresh token.
- The JWT token lasts for 5 mins, the refresh token 5 days.
- The JWT token and refresh token are stored by the client in local storage.
- 10 minutes elapse.
- The client tries to perform an action with a now expired JWT token. The server returns an error with a specific message (so as not to confuse it with a missing token).
    - The client then calls `/Authentication/refresh` with their stored refresh token. This refresh token hasn't expired and is valid, so the server creates new JWT
    token that lasts 15 minutes again and a new refresh token. It deletes/expires the old token pair and saves the new pair. They are sent back to the client.
    - The client stores both tokens in local storage again.
    - The client then tries to perform an action with the new JWT token which is now valid.

This does means you have 3 requests instead of 1, and spikes of requests every 5 minutes (a prime number is best for this), but has the following benefits:

 - Avoids having users need to re-login every 5 minutes.
 - Avoids having the security threat of storing JWT tokens
 - You can invalidate tokens server side.
 - Restrict people from logging on multiple devices if needed.
 - Can be easily extended to have an audit trail.

#### Implementation

JWT is built into ASP.NET Core via its inbuilt Dependency Injection package, and `AddJwtBearer`. This ties into
`IApplicationBuilder.UseAuthentication()` - the token is read using a Middleware implementation inside ASP.NET Core. A `JwtTokenService` hands token creating.

### Policy-based authorization

A lot of time was spent building a scalable policy-based role system into Roadkill. It uses [policy-based authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1#authorization-handlers) that's built into .NET Core 3.x

Here's a brief explanation:

- A set of JWT claim roles, or policies are [declared](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Extensions/ServiceCollectionExtensions.cs#L120)
- These policies all use a [RoadkillRequirement](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Policies/RoadkillPolicyRequirement.cs)
- ASP.NET will use Roadkill's [custom AuthorizationHandler](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Roles/RolesAuthorizationHandler.cs) to handle the RoadkillRequirement that each policy uses.
  - This handler has all `IUserRoleDefinition` injected into it. It goes through each of these definitions, to see if the incoming role name, such as "admin" has the claim, or policy name in the IUserRoleDefinition. It succeeds if it does.
- Each controller action is decorated with an `[Authorize]` attribute, or to be exact a [custom attribute](https://github.com/roadkillwiki/roadkill_new/blob/master/src/Roadkill.Api/Authorization/Roles/AuthorizeWithBearerAttribute.cs) that sets the scheme to be Bearer otherwise ASP.NET uses "Basic". For example to create an admin user, the controller action is decorated with: `[Authorize(Policy = PolicyNames.CreateAdminUser)]`

There are only Editor and Admin role definitions for now, but it is trivial to create more in future. Definitions could even be retrieved from the database, however PolicyNames will always be part of the C# source.

#### Some good JWT resources

- https://www.youtube.com/watch?v=-Z57Ss_uiuc
- https://www.youtube.com/watch?v=AU0TIOZhGqs
