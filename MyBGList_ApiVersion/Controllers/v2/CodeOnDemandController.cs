using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace MyBGList.Controllers.v2
{
    [Route("v{version:apiVersion}/[controller]/[action]")] //3.4.3 //action method
    [ApiController]
    [ApiVersion("2.0")]

    public class CodeOnDemandController : ControllerBase
    {
        [HttpGet(Name = "Test")]
        [EnableCors("AnyOrigin_GetOnly")]
        [ResponseCache(NoStore = true)]
        public ContentResult Test()
        {
            return Content("<script>" +
            "window.alert('Your client supports JavaScript!" +
            "\\r\\n\\r\\n" +
            $"Server time (UTC): {DateTime.UtcNow.ToString("o")}" +
            "\\r\\n" +
            "Client time (UTC): ' + new Date().toISOString());" +
            "</script>" +
            "<noscript>Your client does not support JavaScript</noscript>",
            "text/html");
        }
        [HttpGet(Name = "Test2")]
        [EnableCors("AnyOrigin_GetOnly")]
        [ResponseCache(NoStore = true)]
        public ContentResult Test2(int? addMinutes = null) // int? => needs to accept an optional addMinutes GET parameter of integer type.
        {
            var dateTime = DateTime.UtcNow;
            if (addMinutes.HasValue)  //If such parameter is present, it must be added to the server time before it's sent to the client
            { 
                dateTime = dateTime.AddMinutes(addMinutes.Value); 
            }

            return Content("<script>" +
            "window.alert('Your client supports JavaScript!" +
            "\\r\\n\\r\\n" +
            $"Server time (UTC): {dateTime.ToString("o")}" + // time chages 
            "\\r\\n" +
            "Client time (UTC): ' + new Date().toISOString());" +
            "</script>" +
            "<noscript>Your client does not support JavaScript</noscript>",
            "text/html");

            /*needs to accept an optional
addMinutes GET parameter of integer type. If such parameter is present,
it must be added to the server time before it's sent to the client, so that
the server time (UTC) value shown by the alert window rendered by
the script can be altered by the HTTP request.*/
        }
    }
}
/*Replaced the previous IEnumerable<BoardGame> return value with a
new RestDTO<BoardGame[]> return value, since that's what we're going
to return now.
Replaced the previous anonymous type, which only contained the data,
with the new RestDTO type, which contains the data and the descriptive
links.
Added the HATEOAS descriptive links: for the time being, we only
support a single BoardGame-related endpoint, therefore we've just added
it using the "self" relationship reference.*/