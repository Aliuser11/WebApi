using Microsoft.AspNetCore.Mvc;

/* for handling the app.UseExceptionHandler("/error");*/

/* but now we use Minimal API in Program.cs so this code is no longer needed*/
namespace MyBGList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        [HttpGet]
        public IActionResult Error()
        {
            return Problem(); /*The Problem() method that we’re returning is a method of the
            ControllerBase class – which our ErrorController extends – that produces a ProblemDetail response .
            Since we’re calling the Problem() method without specifying any parameter,
            these values will be automatically set by ASP.NET Core using default values
            taken from the exception that has been thrown*/
        }
    }
}
