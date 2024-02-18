using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.DTO;
using MyBGList.Models;
using System.Linq.Dynamic.Core;
using MyBGList.Constants;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class BoardGamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BoardGamesController> _logger;
        private readonly IMemoryCache _memoryCache;  //Injecting the IMemoryCache interface

        public BoardGamesController(
            ILogger<BoardGamesController> logger,
            ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _logger = logger;
            _memoryCache = memoryCache;
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
        //[HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
        //public async Task <RestDTO<BoardGame[]>> Get(
        //    int pageIndex = 0,

        //    /*using the [Range] attribute: to set pageSize limitations*/
        //    [Range(1, 100) ]int pageSize = 10,//set a default value to the PageIndex and PageSize parameters(respectively, 0 and 10):

        //    //string? sortColumn = "Name", //// old way
        //    [SortColumnValidator(typeof(BoardGameDTO))] string? sortColumn = "Name", ////using new custom validation attribute,

        //    /*string? sortOrder = "ASC", */ //old way
        //    [SortOrderValidator] string? sortOrder = "ASC", // custom validation attribute using .SortOrderValidator class !! 
        //    /*[RegularExpression("ASC|DESC")] string? sortOrder = "ASC",*/ // Using the RegularExpression Attribute

        //    string? filterQuery = null)

        //{
        //    var query = _context.BoardGames.AsQueryable();
        //    /*
        //     It's also worth noting that we had to use the AsQueryable() method to
        //    explicitly cast the DbSet<BoardGame> to an IQueryable<BoardGame>
        //    interface, so that we could "chain" the extension methods to build the
        //    expression tree
        //     */
        //    if (!string.IsNullOrEmpty(filterQuery))
        //        query = query.Where(b => b.Name.Contains(filterQuery));
        //    //query = query.Where(b => b.Name.StartsWith(filterQuery)); /* 5.6.2 modify get method exercise. change b.Name.Contains(filterQuery) to Name.StartsWith  filterQuery*/

        //    var recordCount = await query.CountAsync();
        //    query = query     
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
        //            //example https://localhost:40443/BoardGames?filterQuery=war - to retrieve thefirst 10 board games sorted by Name (ascending) with a Name containing "war".
        //        }
        //    };
        //}


        /* from chapter 6 : Now that we have the RequestDTO class, we can use it to replace the simple type parameters of the BoardGamesController's Get method */
        [HttpGet(Name = "GetBoardGames")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)] //set up a public cache with a max-age of 60 seconds for that cache  response
      
        [ResponseCache(CacheProfileName = "Any-60")] // cache profiles from chapter 8 implementation:
        //[ResponseCache(CacheProfileName = "Client-120")] //=> 8.5.2 exercise cache profile 
        public async Task <RestDTO<BoardGame[]>> Get(
            /*use the [FromQuery] attribute to tell the routing middleware that we want to get the input values from the query string*/
            [FromQuery] RequestDTO <BoardGameDTO> input) /*<BoardGameDTO> => specified the BoardGameDTO as a generic type parameter*/
        {
            //_logger.LogInformation("get method started.");             //logging test
            //_logger.LogInformation(50110, "Get method started.");             // Setting custom event IDs
            _logger.LogInformation(CustomLogEvents.BoardGamesController_Get, "Get method started");  // using CustomLogEvents
                                                                                                     //_logger.LogInformation(CustomLogEvents.BoardGamesController_Get, $"Get method started at {DateTime.Now:HH:mm}"); /*string interpolation: */

            /*the message template syntax allows us to use string-based placeholders instead of numeric ones, which definitely improves readability.*/
            //_logger.LogInformation(CustomLogEvents.BoardGamesController_Get,
            //"Get method started at {StartTime:HH:mm}.", DateTime.Now);

            //Implementing an in-memory caching strategy
            BoardGame[]? result = null;
            var cacheKey =
                $"{input.GetType()} - {JsonSerializer.Serialize(input)}";
            if (!_memoryCache.TryGetValue<BoardGame[]>(cacheKey, out result))
            {
                var query = _context.BoardGames.AsQueryable();
                if (!string.IsNullOrEmpty(input.FilterQuery))
                    query = query.Where(b => b.Name.Contains(input.FilterQuery));
                query = query
                        .OrderBy($"{input.SortColumn} {input.SortOrder}")
                        .Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize);
                result = await query.ToArrayAsync();
                _memoryCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
            }

            return new RestDTO<BoardGame[]>()
            {
                //Data = await query.ToArrayAsync(),
                Data = result, // from chapter 8 that is
                PageIndex = input.PageIndex, //Add the pageIndex and pageSize input parameters,
                PageSize = input.PageSize,
                RecordCount = await _context.BoardGames.CountAsync(), //Calculate the RecordCount

                Links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", new {input.PageIndex, input.PageSize }, 
                        Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };

            // 8.5.4 in memory caching Record Count property
            /* recordCount from RestDRO object */
            /*(int recordCount, BoardGame[]? result) dataTuple = (0, null); //*
            var cacheKey = $"{input.GetType()} - {JsonSerializer.Serialize(input)}";
            if (!_memoryCache.TryGetValue(cacheKey, out dataTuple))  //*
            {
                var query = _context.BoardGames.AsQueryable();
                if (!string.IsNullOrEmpty(input.FilterQuery))
                {
                    query = query.Where(b => b.Name.Contains(input.FilterQuery));
                }
                dataTuple.recordCount = await query.CountAsync();//*
                query = query.
                        OrderBy($"{input.SortColumn} {input.SortOrder}")
                        .Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize);
                dataTuple.result = await query.ToArrayAsync();//*
                _memoryCache.Set(cacheKey, dataTuple, new TimeSpan(0, 0, 120));//*
            }
            return new RestDTO<BoardGame[]>()
            {
                Data = dataTuple.result, //*
                PageIndex = input.PageIndex,
                PageSize = input.PageSize,
                RecordCount = dataTuple.recordCount,//*
                Links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(null, "BoardGames", new {input.PageIndex, input.PageSize },
                        Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };*/
        }

        /* POST method */
        [HttpPost(Name = "UpdateBoardGame")]
        [ResponseCache(NoStore =true)]
        public async Task<RestDTO<BoardGame?>> Post(BoardGameDTO model)
        {
            var boardgame = await _context.BoardGames
                            .Where(b => b.Id == model.Id)
                            .FirstOrDefaultAsync();
            if (boardgame != null)
            {
                if(!string.IsNullOrEmpty(model.Name))
                {
                    boardgame.Name = model.Name;
                }
                if(model.Year.HasValue && model.Year.Value > 0)
                {
                    boardgame.Year = model.Year.Value;
                }
                // and 5.6.3 Update => table: MinPlayers, MaxPlayers, PlayTime, MinAge.
                //if (model.MinPlayers.HasValue && model.MinPlayers.Value > 0)
                //{
                //    boardgame.MinPlayers = model.MinPlayers.Value;
                //}
                //if(model.MaxPlayers.HasValue && model.MaxPlayers.Value > 0)
                //{
                //    boardgame.MaxPlayers = model.MaxPlayers.Value;
                //}
                //if (model.PlayTime.HasValue && model.PlayTime.Value > 0)
                //{
                //    boardgame.PlayTime = model.PlayTime.Value;
                //}
                //if(model.MinAge.HasValue && model.MinAge.Value > 0 )
                //{
                //    boardgame.MinAge = model.MinAge.Value;
                //}

                boardgame.LastModifiedDate = DateTime.Now;
                _context.BoardGames.Update(boardgame);
                await _context.SaveChangesAsync();
            };
            return new RestDTO<BoardGame?>()
            {
                Data = boardgame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "BoardGames",model,Request.Scheme)!,
                        "self",
                        "POST"),
                }
            };
            /* TO DO : 
             Fill the JSON request body with the following values to change the Name of
            the Risk board game (Id 181) to "Risk!", and the publishing Year from 1959
            to 1980:
                {
                "id": 181,
                "name": "Risk!",
                "year": 1980
                }*/
        }

        /*DELETE METHOD*/

        [HttpDelete(Name = "DeleteBoardGame")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<BoardGame?>> Delete(int id)
        {

            var boardgame = await _context.BoardGames
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();
            if (boardgame != null)
            {
                _context.BoardGames.Remove(boardgame);
                await _context.SaveChangesAsync();
            };

            return new RestDTO<BoardGame?>()
            {
                Data = boardgame,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        //Url.Action(null, "BoardGames", id, Request.Scheme)!,
                        Url.Action(null, "BoardGames", id, Request.Scheme)!,
                        "self",
                        "DELETE"),
                }
            };
        }

        ///*/ 5.exercise delete method*/
        //[HttpDelete(Name = "DeleteBoardGame")]
        //[ResponseCache(NoStore = true)]
        //public async Task<RestDTO<BoardGame[]?>> Delete(string idList)
        //{
        //    /*idList string parameter, which will be used by clients to specify a comma-separated list of Id keys*/
        //    var idArray = idList.Split(",").Select(x => int.Parse(x)); // each single id contained in the idList parameter is of an integer type
        //    var deleteBG = new List<BoardGame>();
        //    foreach (int id in idArray)
        //    {
        //        var boardgame = await _context.BoardGames
        //        .Where(b => b.Id == id)
        //        .FirstOrDefaultAsync();

        //        if (boardgame != null)
        //        {
        //            deleteBG.Add(boardgame);  // ad to list all that was deleted 
        //            _context.BoardGames.Remove(boardgame); ////Delete all the board games matching one of the given id keys.
        //            await _context.SaveChangesAsync();
        //        };
        //    }

        //    return new RestDTO<BoardGame[]?>()
        //    {
        //        Data = deleteBG.Count > 0 ? deleteBG.ToArray() : null, ///object containing all the deleted board games in the data array.
        //        Links = new List<LinkDTO>
        //            {
        //                new LinkDTO(
        //                        Url.Action(
        //                            null,
        //                            "BoardGames",
        //                            idList,
        //                            Request.Scheme)!,
        //                        "self",
        //                        "DELETE"),
        //            }
        //    };
    //}
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