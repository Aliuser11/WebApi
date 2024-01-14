using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace MyBGList.Controllers.v3
{
    [Route("v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ApiVersion("3.0")]

    public class CodeOnDemandController : ControllerBase
    {
        [HttpGet(Name = "Test2")]
        [EnableCors("AnyOrigin_GetOnly")]
        [ResponseCache(NoStore = true)]
        public ContentResult Test2(int? minutesToAdd = null) //3,4,4 the addMinutes GET parameter must be renamed to minutesToAdd, without affecting version 2.
        {
            var dateTime = DateTime.UtcNow;
            if (minutesToAdd.HasValue)
            {
                dateTime = dateTime.AddMinutes(minutesToAdd.Value);
            }

            return Content("<script>" +
            "window.alert('Your client supports JavaScript!" +
            "\\r\\n\\r\\n" +
            $"Server time (UTC): {dateTime.ToString("o")}" + 
            "\\r\\n" +
            "Client time (UTC): ' + new Date().toISOString());" +
            "</script>" +
            "<noscript>Your client does not support JavaScript</noscript>",
            "text/html");
        }
    }
}
          