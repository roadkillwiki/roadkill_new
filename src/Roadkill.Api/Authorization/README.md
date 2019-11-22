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

