# SecureTodoList
Learning how to use IdentityServer4 with ASP.NET Core by making a basic todo list application.

_Please help me improve myself by improving this app. If ever I do something wrong or if there is something I can improve, please file an issue or a pull request. Thank you._

---

**This README will only contain the IdentityServer4 Setup.**
Table of Contents:
1. [API Setup](https://github.com/reiniellematt/SecureTodoList/wiki/1-WebAPI-Setup)
2. [Web Application Setup](https://github.com/reiniellematt/SecureTodoList/wiki/2-Web-Application-Setup)
3. [IdentityServer Setup](https://github.com/reiniellematt/SecureTodoList/wiki/3-IdentityServer4-Setup)



## IdentityServer4 Setup
### This project aims to help myself on how to create a basic application secured by [IdentityServer4](https://identityserver4.readthedocs.io/).

From the [wiki](https://github.com/reiniellematt/SecureTodoList/wiki)

1. Install the IdentityServer4 templates using the command:
    ```
    dotnet new -i IdentityServer4.Templates
    ```

2. Add a new IdentityServer4 project into the solution by going into the SecureTodoList folder and opening the command prompt.
    ```
    dotnet new is4aspid -n SecureTodoList.IdServ
    ```

    later on you will be asked with:
    ```
    ...
    Manual instructions: Seeds the initial database
    Actual command: dotnet run /seed
    Do you want to run this action (Y|N)?
    ```

    Enter ```y``` in order to seed our database with users.

3. Edit the  ```Apis``` collection inside the ```Config.cs``` to add our API.
    ```cs
    public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource("SecureTodoList.Api", "SecureTodoList API")
            };
    ```

4. Edit the ```Clients``` collection inside the same file to add our Razor Pages client.
    ```cs
    public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // MVC client using code flow + pkce
                new Client
                {
                    ClientId = "SecureTodoList.Web",
                    ClientName = "SecureTodoList Razor Pages Client",

                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RequirePkce = true,
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    RedirectUris = { "https://localhost:5002/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5002/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "SecureTodoList.Api" }
                },
            };
    ```

    The ```ClientId``` and ```ClientSecrets``` basically is like the username/password for the application. Remember to store this in a safe location.

### Configuring the WebAPI

1. Install the ```Microsoft.AspNetCore.Authentication.JwtBearer``` NuGet package into the API.

2. Update the ```ConfigureServices``` section of the ```Startup.cs``` to use authentication services with the DI so that it can validate incoming tokens making sure it came from a trusted issuer and validate if it can be used with this API (a.k.a. Audience).
    ```cs
    public void ConfigureServices(IServiceCollection services)
        {
            // ...

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "http://localhost:5000";

                    options.RequireHttpsMetadata = false;

                    options.Audience = "SecureTodoList.Api";
                });

            // ...
        }
    ```

3. Edit the ```Configure``` method inside the ```Startup.cs``` to use Authentication.
    ```cs
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...

        app.UseAuthentication();
        app.UseAuthorization();

        // ...
    }
    ```

### Configuring the Web Application

1. Install the ```Microsoft.AspNetCore.Authentication.OpenIdConnect``` NuGet package to enable OpenID Connect authentication.

2. Add the following code into the ```ConfigureServices``` method in ```Startup.cs``` inside the Web App.

    ```cs
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = "http://localhost:5000";
            options.RequireHttpsMetadata = false;

            options.ClientId = "SecureTodoList.Web";
            options.ClientSecret = "secret";
            options.ResponseType = "code";

            options.SaveTokens = true;

            options.Scope.Add("SecureTodoList.Api");
            options.Scope.Add("offline_access");
        });
    ```
    You can copy the ```ClientId``` and ```ClientSecret``` inside the ```Config.cs``` of the ```SecureTodoList.ISHost```project. Remember that the ```.Sha256()``` method at the end of the ClientSecret is not needed since this will be done by IdentityServer.

3. Modify the ```Index.cshtml.cs``` and ```Create.cshtml.cs``` and add an ```[Authorize]``` attribute.
    ```cs
    // ...
    using Microsoft.AspNetCore.Authorization; 

    [Authorize]
    public class IndexModel : PageModel
    {
       // ... 
    }
    ```

    ```cs
    // ...
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class CreateModel : PageModel 
    {
        // ...
    }
    ```
4. Setting the Bearer token for requests.

    We can set the Bearer token by adding another header for our HttpClient, but to make our code cleaner, we need to install the ```IdentityModel``` NuGet package inside ```SecureTodoList.Web``` project. This will allow us to set the bearer token using the ```.SetBearerToken()``` extension method for our client.

    Now edit the ```Index.cshtml.cs``` file:
    ```cs
    using IdentityModel.Client;
    // ...

    public async Task OnGet()
    {
        var token = await HttpContext.GetTokenAsync("access_token");

        using (var client = _clientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://localhost:5001");
            client.SetBearerToken(token);

            // ...
    ```
    ```cs
    // ...

    public async Task<IActionResult> OnPostMarkDoneAsync(int itemId)
    {

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await HttpContext.GetTokenAsync("access_token");

        using (var client = _clientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://localhost:5001");
            client.SetBearerToken(token);

            // ...
    ```

    And edit the ```Create.cshtml.cs```
    ```cs
    using IdentityModel.Client;
    // ...
    
    public async Task OnGet()
    {
        var token = await HttpContext.GetTokenAsync("access_token");

        using (var client = _clientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://localhost:5001");
            client.SetBearerToken(token);

            // ...
    ```

    Now try the application. You will need to redirected to a log-in page. Log-in using:
    
    Username: ```alice```

    Password: ```Pass123$```

    For the next section, we will now let each user have a custom todo list that will only let them see their todo list.

### Each user, each set of todo items

My approach would be using the Id of the current user and set it for the todo items and we will be handling it in the API.

1. Edit the ```TodoController.cs``` in the API. First, the ```Get``` method.
    ```cs
    // GET api/todo
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var items = await _context.TodoItems.Where(t => !t.IsDone && t.UserId.Equals(userId)).ToListAsync();

        return Ok(items);
    }
    ```

2. Edit the ```Post``` method:
    ```cs
    // POST api/todo
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TodoItem newItem)
    {
        newItem.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _context.Entry(newItem).State = EntityState.Added;

        var ok = await _context.SaveChangesAsync();

        if (ok.Equals(1))
        {
            // Will return the newly created item.
            return Ok(newItem);
        }
        else
        {
            return BadRequest();
        }
    }
    ```

### Now run and use the application!