using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

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
        //[HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        //public async Task <RestDTO<BoardGame[]>> Get(
        //    int pageIndex = 0,
        //    int pageSize = 10)//set a default value to the PageIndex and PageSize parameters(respectively, 0 and 10):
        //{
        //    var query = _context.BoardGames
        //        .Skip(pageIndex * pageSize)
        //        .Take(pageSize);

        //    return new RestDTO<BoardGame[]>()
        //    {
        //        Data = await query.ToArrayAsync(),
        //        PageIndex = pageIndex, //Add the pageIndex and pageSize input parameters,
        //        PageSize = pageSize,
        //        RecordCount = await _context.BoardGames.CountAsync(), //Calculate the RecordCount

        //        Links = new List<LinkDTO> {
        //            new LinkDTO(
        //                Url.Action(null, "BoardGames", new {pageIndex, pageSize }, Request.Scheme)!,
        //                "self",
        //                "GET"),
        //            //example https://localhost:40443/BoardGames?pageIndex=0&pageSize=5
        //        }
        //    };
        //}

        /* dynamically sort our records using the DBMS engine */
        //[HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        //public async Task <RestDTO<BoardGame[]>> Get(
        //    int pageIndex = 0,
        //    int pageSize = 10,//set a default value to the PageIndex and PageSize parameters(respectively, 0 and 10):
        //    string sortColumn = "Name",
        //    string sortOrder = "ASC")

        //{
        //    var query = _context.BoardGames
        //        .OrderBy($"{sortColumn} {sortOrder}") // musi być spacja! OrderBy() extension method provided by Dynamic LINQ.
        //        .Skip(pageIndex * pageSize)
        //        .Take(pageSize);

        //    return new RestDTO<BoardGame[]>()
        //    {
        //        Data = await query.ToArrayAsync(),
        //        PageIndex = pageIndex, //Add the pageIndex and pageSize input parameters,
        //        PageSize = pageSize,
        //        RecordCount = await _context.BoardGames.CountAsync(), //Calculate the RecordCount

        //        Links = new List<LinkDTO> {
        //            new LinkDTO(
        //                Url.Action(null, "BoardGames", new {pageIndex, pageSize }, Request.Scheme)!,
        //                "self",
        //                "GET"),
        //            //example https://localhost:40443/BoardGames?sortColumn=Year - to retrieve the first 10 records, sorted by Year (ascending)
        //        }
        //    };
        //}

        /* Method with paging, sorting, and new => filtering */
        [HttpGet(Name = "GetBoardGames")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        public async Task <RestDTO<BoardGame[]>> Get(
            int pageIndex = 0,
            int pageSize = 10,//set a default value to the PageIndex and PageSize parameters(respectively, 0 and 10):
            string? sortColumn = "Name",
            string? sortOrder = "ASC",
            string? filterQuery = null)

        {
            var query = _context.BoardGames.AsQueryable();
            /*
             It's also worth noting that we had to use the AsQueryable() method to
            explicitly cast the DbSet<BoardGame> to an IQueryable<BoardGame>
            interface, so that we could "chain" the extension methods to build the
            expression tree
             */
            if (!string.IsNullOrEmpty(filterQuery))
                query = query.Where(b => b.Name.Contains(filterQuery));
            var recordCount = await query.CountAsync();
            query = query     
                .OrderBy($"{sortColumn} {sortOrder}") // musi być spacja! OrderBy() extension method provided by Dynamic LINQ.
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
                    //example https://localhost:40443/BoardGames?sortColumn=Year - to retrieve the first 10 records, sorted by Year (ascending)
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