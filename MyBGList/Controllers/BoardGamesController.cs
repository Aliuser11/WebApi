using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class BoardGamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BoardGamesController> _logger;

        public BoardGamesController(
            ILogger<BoardGamesController> logger,
            ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        //[HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        //public async Task <RestDTO<BoardGame[]>> Get()
        //{
        //    var query = _context.BoardGames;
        //    return new RestDTO<BoardGame[]>()
        //    {
        //        Data = await query.ToArrayAsync(), 
        //        Links = new List<LinkDTO> {
        //            new LinkDTO(
        //                Url.Action(null, "BoardGames", null, Request.Scheme)!,
        //                "self",
        //                "GET"),
        //        }
        //    };
        //} 

        /* Get method with paging */
        [HttpGet(Name = "GetBoardGames")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        public async Task <RestDTO<BoardGame[]>> Get(
            int pageIndex = 0,
            int pageSize = 10)//set a default value to the PageIndex and PageSize parameters(respectively, 0 and 10):
        {
            var query = _context.BoardGames
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            return new RestDTO<BoardGame[]>()
            {
                Data = await query.ToArrayAsync(),
                PageIndex = pageIndex, //Add the pageIndex and pageSize input parameters,
                PageSize = pageSize,
                RecordCount = await _context.BoardGames.CountAsync(), //Calculate the RecordCount

                Links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", new {pageIndex, pageSize }, Request.Scheme)!,
                        "self",
                        "GET"),
                    //example https://localhost:40443/BoardGames?pageIndex=0&pageSize=5
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