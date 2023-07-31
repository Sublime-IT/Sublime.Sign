# Sublime.Sign
Used for secure signing and validation. Useful for authenticating between different services / graphql fusion.

## To install

For .NET 7 use 
> dotnet add package Sublime.Sign --version 1.0.0

For .NET 6 use
> dotnet add package Sublime.Sign --version 1.0.0-net6

## Example Usecase

**Problem:** 

We use hot chocolate fusion to do schema stiching on our GraphQL schemas on Gubi Ecommerce Engine.
Our JWT tokens are signed with roles, but not with their respective permissions. We are unable to change how the JWT token is signed because we use AWS Cognito as login provider.

Only the ecommerce engine knows which roles has which permissions.
Orders are stored on a order microservice and schema stiching is used for querying orders.


**Solution:**

With GraphQL fusion we can forward headers:

```CSharp
builder.Services
.AddCors( /*omitted*/ ) 
.AddHeaderPropagation(c => {
    c.Headers.Add("GraphQL-Preflight");
    c.Headers.Add("Authorization");
    c.Headers.Add("X-Permissions");
    c.Headers.Add("X-Permissions-Signature");
  });

builder.Services.AddFusionGatewayServer("gateway")
```

From the **order microservice** we can validate the authorization header and get the user groups that the querying user belongs to:
```CSharp
internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
  // ...
  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
  {
    if (context.User == null)
    {
      context.Fail();
      return;
    }

    var groups = context.User.Claims.Where(c => c.Type == "cognito:groups").Select(c => c.Value).ToList();
  }
  //...
}
```

Given the groups, we need to find out which permissions those groups has. This is where **Sublime.Sign** comes in:
On Ecommerce Engine (where the fusion gateway server is added) we

1. Add middleware to intercept requests from authenticated users.
2. On the intercepted request, get permissions and add them to the request header:
```CSharp
 public async Task InvokeAsync(HttpContext httpContext)
  {
    try
    {
      if (httpContext.User == null)
      {
        // unauthenticated user
        await _next(httpContext);
        return;
      }

      // gets all group names that the user is part of, gets all permissions for those groups
      // return as Dictionary<string, List<string>> where key is group name and List<string> is permission list.
      var permissionsByGroup = await GetPermissions(); 

      var headerAccessor = new HeaderAccessor<Dictionary<string, List<string>>>(
        _credentials.GetCredentials<string>("order_api_key")
      );

      headerAccessor.SetHeader(httpContext.Request, "X-Permissions", permissionsByGroup);

      await _next(httpContext);
    }
    catch (Exception)
    {
    }
  }
```
3. **Sublime.Sign** then automatically adds the headers "X-Permissions" and "X-Permissions-Signature" to the request.
4. From our order service, when we validate our request in *HandleRequirementAsync* above, we can simply add the following to check & verify permissions:

The "GetHeader" method automatically throws an *InvalidSignatureException* if the signature mismatches, meaning the permissions to groups mapping can only be signed by a service that has the correct key.
```CSharp
       var groups = GetGroups(context);

        if (IsSuperAdmin(groups))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;

        }

        var headerAccessor = GetHeaderAccessor();

        try
        {
            var permissions = headerAccessor.GetHeader(_httpContextAccessor.HttpContext!.Request, "X-Permissions");

            if (permissions == null)
            {
                context.Fail();
                return;
            }

            // Get a sum of user permissions from aws groups
            var userPermissions = new List<string>();
            foreach (var group in groups)
            {
                if (permissions!.ContainsKey(group))
                {
                    userPermissions.AddRange(permissions[group]);
                }
            }

            // Check if the user has the permission
            if (!userPermissions.Contains(requirement.Permission))
            {
                context.Fail();
                return;
            }

        }
        catch (InvalidSignatureException)
        {
            // Someone tried to tamper with the signature, or a service is using wrong key.
            context.Fail();
            return;
        }
```




