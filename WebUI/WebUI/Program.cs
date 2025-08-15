using Microsoft.AspNetCore.Components.Authorization;
using WebUI.Client.Pages;
using WebUI.Components;
using WebUI.Services;

namespace WebUI;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        // Add session support
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ITokenService, TokenService>();

        // Authentication services (NO cookie authentication needed)
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState(); // This is what was missing
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthService>();

        // HTTP client for API calls
        builder.Services.AddScoped(sp =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7191/") };

            // Try to get token and set authorization header
            var tokenService = sp.GetRequiredService<ITokenService>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var sessionId = httpContextAccessor.HttpContext?.Request.Cookies["AuthSession"];

            if (!string.IsNullOrEmpty(sessionId))
            {
                var token = tokenService.GetToken(sessionId);
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            return httpClient;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}
