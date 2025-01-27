using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("backend")]
public class GreetingsController : ControllerBase
{
    [HttpGet]
    [Route("greetings")]
    public string Get()
    {
        return "Hello from the Backend project";
    }
}