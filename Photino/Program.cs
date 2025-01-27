using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Photino.NET;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Backend;

namespace Photino;
//NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
// Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".

class Program
{

#if DEBUG
    public static bool IsDebugMode = true;     // serve files from ui dev server
#else
    public static bool IsDebugMode = false;     // serve files from asp.net runtime
#endif

    private static class Configurations
    {
        public static string BaseUrl = "http://localhost";
        public static int BackendServerPort = 5000;
        public static int Dev_FrontendServerPort = 3000;
    }

    /// <summary>
    /// Create a static file server.
    /// The builder setup is taken from <see href="https://github.com/tryphotino/photino.NET.Server/blob/master/Photino.NET.Server/Photino.NET.Server.cs">Photino.NET.Server</see>.
    /// We added the SPA app routing.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="startPort"></param>
    /// <param name="portRange"></param>
    /// <param name="webRootFolder"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    /// <exception cref="SystemException"></exception>
    /// <see href="https://github.com/tryphotino/photino.NET.Server/blob/master/Photino.NET.Server/Photino.NET.Server.cs"/>
    public static WebApplication CreateStaticFileServer(
        string[] args, int startPort, int portRange,
        string webRootFolder, out string baseUrl)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = args,
            // WebRootPath = // don't create web root path on disk
        });

        // get files from embedded resources
        var manifestEmbeddedFileProvider = new ManifestEmbeddedFileProvider(
                System.Reflection.Assembly.GetEntryAssembly()!, // not null, because code is managed
                $"Resources/{webRootFolder}");
        builder.Environment.WebRootFileProvider = manifestEmbeddedFileProvider;

        // set base url
        // try ports until available port is found
        int port = startPort;
        while (IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(x => x.Port == port))
        {
            if (port > port + portRange)
                throw new SystemException($"Couldn't find open port within range {port - portRange} - {port}.");
            port++;
        }
        baseUrl = $"{Configurations.BaseUrl}:{port}";
        builder.WebHost.UseUrls(baseUrl);
        builder.Services.AddControllers()
            // use extension method from Backend project
            .AddBackendControllers();

        WebApplication app = builder.Build();

        // app.UseCors("AllowAll");
        app.MapControllers(); // Map API controllers
        // app.MapFallbackToFile("index.html"); // Serve index.html for all other routes

        // do controllers before calling "UseSpa"
        // app.MapGet("/greetings", () => "Hello from ASP.NET");

        // Middleware ordering, as recommended here: 
        // https://learn.microsoft.com/en-gb/aspnet/core/diagnostics/asp0014?view=aspnetcore-9.0#when-to-suppress-warnings
#pragma warning disable ASP0014 
        app.UseRouting();
        app.UseEndpoints(e => { });
#pragma warning restore ASP0014

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
            app.UseStaticFiles(); // uses the WebRootFileProvider
            app.UseSpa(spa => { /* empty config */ }); // reroutes every request to index.html
        }

        return app;
    }

    [STAThread]
    static async Task Main(string[] args)
    {

        // start the asp net server
        var server = CreateStaticFileServer(args, Configurations.BackendServerPort, 100, "wwwroot", out string appUrl);
        _ = server.StartAsync();

        Console.WriteLine($"Running in Debug Mode: {IsDebugMode}");
        Console.WriteLine($"Serving React app at {appUrl}");

        // Start photino window
        // Creating a wrapping thread because of STA and async main discussion
        // https://github.com/tryphotino/photino.NET/issues/52#issuecomment-2074325176
        var thread = new Thread(() =>
        {
            // Creating a new PhotinoWindow instance
            var window = new PhotinoWindow()
                .SetTitle("Photino.React Demo App")
                .SetUseOsDefaultSize(true)
                // .SetSize(new Size(2048, 1024))
                // Center window in the middle of the screen
                .Center()
                // Users can resize windows by default.
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
                });
            window.Load(appUrl); // Can be used with relative path strings or "new URI()" instance to load a website.
            window.WaitForClose(); // Starts the application event loop
        });
        // Configure thread for STA (Single Thread Apartment)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            thread.SetApartmentState(ApartmentState.STA);
        }
        thread.Start(); // Start running STA thread for action
        thread.Join(); // Sync back to running thread

        await server.StopAsync(); // Stop the AspNet server
    }
}
