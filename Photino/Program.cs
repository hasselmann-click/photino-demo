using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Photino.NET;
using System.Drawing;
using System.Text;

namespace Photino;
//NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
// Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".

class Program
{

#if DEBUG
    public static bool IsDebugMode = true;     //serve files from asp.net runtime
#else
    public static bool IsDebugMode = false;     //serve files from asp.net runtime
#endif

    private static class Configurations
    {
        public static string BaseUrl = "http://localhost";
        public static int BackendServerPort = 5000;
        public static int Dev_FrontendServerPort = 3000;
    }

    static WebApplication CreateAspNetServer(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseKestrel(options =>
        {
            options.ListenAnyIP(Configurations.BackendServerPort);
        });
        // builder.Services.AddControllers();
        builder.Services.AddSpaStaticFiles(options =>
        {
            options.RootPath = "wwwroot";
        });
        var app = builder.Build();

        // app.UseCors("AllowAll");
        // app.MapControllers(); // Map API controllers
        // app.MapFallbackToFile("index.html"); // Serve index.html for all other routes

        // do controllers before calling "UseSpa"
        app.MapGet("/greetings", () => "Hello from ASP.NET");
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSpa(spa =>
            {
                // Proxy requests to the SPA development server
                spa.UseProxyToSpaDevelopmentServer($"{Configurations.BaseUrl}:{Configurations.Dev_FrontendServerPort}");
            });
        }
        else
        {
            // app.UseExceptionHandler("/Home/Error");
            // app.UseHsts();

            // Serve static files from the SPA build folder
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "wwwroot";
            });
        }

        return app;
    }

    [STAThread]
    static async Task Main(string[] args)
    {
        // PhotinoServer
        //     .CreateStaticFileServer(args, out string baseUrl)
        //     .RunAsync();
        var cancellationSource = new CancellationTokenSource();
        var server = CreateAspNetServer(args);
        _ = server.StartAsync(cancellationSource.Token);

        // The appUrl is set to the local development server when in debug mode.
        // This helps with hot reloading and debugging.
        string appUrl = $"{Configurations.BaseUrl}:{Configurations.BackendServerPort}";
        Console.WriteLine($"Serving React app at {appUrl}");

        // Window title declared here for visibility
        string windowTitle = "Photino.React Demo App";

        // Creating a new PhotinoWindow instance with the fluent API
        var window = new PhotinoWindow()
            .SetTitle(windowTitle)
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(2048, 1024))
            // Resize to a percentage of the main monitor work area
            //.Resize(50, 50, "%")
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(800, 600))
            // Center window in the middle of the screen
            .Center()
            // Users can resize windows by default.
            // Let's make this one fixed instead.
            .SetResizable(true)
            .RegisterCustomSchemeHandler("app", (object sender, string scheme, string url, out string contentType) =>
            {
                contentType = "text/javascript";
                return new MemoryStream(Encoding.UTF8.GetBytes(@"
                        (() =>{
                            window.setTimeout(() => {
                                alert(`🎉 Dynamically inserted JavaScript.`);
                            }, 1000);
                        })();
                    "));
            })
            // Most event handlers can be registered after the
            // PhotinoWindow was instantiated by calling a registration 
            // method like the following RegisterWebMessageReceivedHandler.
            // This could be added in the PhotinoWindowOptions if preferred.
            .RegisterWebMessageReceivedHandler((object sender, string message) =>
            {
                var window = (PhotinoWindow)sender;

                // The message argument is coming in from sendMessage.
                // "window.external.sendMessage(message: string)"
                string response = $"Received message: \"{message}\"";

                // Send a message back the to JavaScript event handler.
                // "window.external.receiveMessage(callback: Function)"
                window.SendWebMessage(response);
            })
            .Load(appUrl); // Can be used with relative path strings or "new URI()" instance to load a website.

        window.WaitForClose(); // Starts the application event loop
        await cancellationSource.CancelAsync(); // Stop the server
        await server.StopAsync(); // Stop the AspNet server
    }
}
