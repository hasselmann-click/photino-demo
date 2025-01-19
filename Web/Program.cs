var Configurations = new
{
    BaseUrl = "http://localhost",
    BackendServerPort = 5000,
    Dev_FrontendServerPort = 3000
};

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// app.UseCors("AllowAll");
// app.MapControllers(); // Map API controllers
// app.MapFallbackToFile("index.html"); // Serve index.html for all other routes

// do controllers before calling "UseSpa"
app.MapGet("/greetings", () => "Hello from ASP.NET");

// Middleware ordering, as recommended here: 
// https://learn.microsoft.com/en-gb/aspnet/core/diagnostics/asp0014?view=aspnetcore-9.0#when-to-suppress-warnings
#pragma warning disable ASP0014
app.UseRouting();
app.UseEndpoints(e => { });
#pragma warning restore ASP0014

Console.WriteLine($"Running in Debug Mode: {app.Environment.IsDevelopment()}");
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

app.Run();
