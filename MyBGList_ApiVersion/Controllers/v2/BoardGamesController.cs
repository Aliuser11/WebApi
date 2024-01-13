using Microsoft.AspNetCore.Mvc;
using MyBGList.DTO.v2;

namespace MyBGList.Controllers.v2
{
    [Route("v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]

    public class BoardGamesController : ControllerBase
    {
        private readonly ILogger<BoardGamesController> _logger;
        public BoardGamesController(ILogger<BoardGamesController> logger)
        {
            _logger = logger;
        }
        [HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 120)] //3.4.2 exercise set up a private cache with a max-age of 120 seconds for that cache  response
        public RestDTO<BoardGame[]> Get()
        {
            return new RestDTO<BoardGame[]> ()
            {
                Items = new BoardGame[] {
                    new BoardGame() {
                        Id = 1,
                        Name = "Axis & Allies",
                        Year = 1981,
                        MinPlayers = 2,
                        MaxPlayers = 5
                    },
                    new BoardGame() {
                        Id = 2,
                        Name = "Citadels",
                        Year = 2000
                    },
                    new BoardGame() {
                        Id = 3,
                        Name = "Terraforming Mars",
                        Year = 2016,
                        MinPlayers = 1,
                        MaxPlayers = 11
                    }
                },
                Links = new List<DTO.v1.LinkDTO> { 
                    new DTO.v1.LinkDTO( 
                        Url.Action(null, "BoardGames", null, Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
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