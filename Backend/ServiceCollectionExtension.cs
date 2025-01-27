using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Backend.Controllers;

namespace Backend;
public static class ServiceCollectionExtension {

    public static IMvcBuilder AddBackendControllers(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(GreetingsController).Assembly);;
        return mvcBuilder;
    }
}