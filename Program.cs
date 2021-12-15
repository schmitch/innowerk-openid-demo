using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
        options.SlidingExpiration = false;
    })
    .AddOpenIdConnect(options =>
    {
        // Note: these settings must match the application details
        // inserted in the database at the server level.
        options.ClientId = "Innowerk";
        
        // hier das Innowerk ClientSecret einfügen, z.B. über eine appsettings.Development.json
        options.ClientSecret = builder.Configuration.GetValue<string>("OpenIdConnect:ClientSecret");

        options.RequireHttpsMetadata = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        
        // speichert die tokens im cookie
        // (meistens keine gute idee, weil diese super riesig sind, i.e. das dürfte bei uns etwa 8kb cookies generieren)
        options.SaveTokens = false;

        // Use the authorization code flow.
        options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;

        // Note: setting the Authority allows the OIDC client middleware to automatically
        // retrieve the identity provider's configuration and spare you from setting
        // the different endpoints URIs or the token validation parameters explicitly.
        options.Authority = "https://asvg-salestool.envisia.io/";

        options.Scope.Add("email");
        options.Scope.Add("roles");
        options.Scope.Add("openid");

        options.SecurityTokenValidator = new JwtSecurityTokenHandler
        {
            // Disable the built-in JWT claims mapping feature.
            InboundClaimTypeMap = new Dictionary<string, string>()
        };

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
    });

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();