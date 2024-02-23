using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyBGList.DTO;
using MyBGList.Extensions;
using MyBGList.Models;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace MyBGList.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MechanicsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MechanicsController> _logger;
        private readonly IDistributedCache _distributedCache;
        public MechanicsController(
            ApplicationDbContext context, 
            ILogger<MechanicsController> logger, IDistributedCache distributedCache)
        {
            _context = context;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet(Name = "GetMechanics")]
        //[ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        [ResponseCache(CacheProfileName = "Any-60")]
        //[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 120)] //=> 8.5.1 exercise response caching 
        public async Task<RestDTO<Mechanic[]>> Get(

           [FromQuery] RequestDTO<MechanicDTO> input) 
        {
            Mechanic[]? result = null; // => Injecting the IDistributedCache interface
            var cacheKey =
                $"{input.GetType()}-{JsonSerializer.Serialize(input)}";
            if (!_distributedCache.TryGetValue<Mechanic[]>(cacheKey, out result));
            {
                var query = _context.Mechanics.AsQueryable();
                if (!string.IsNullOrEmpty(input.FilterQuery))
                    query = query.Where(b => b.Name.Contains(input.FilterQuery));
                var recordCount = await query.CountAsync();
                query = query
                        .OrderBy($"{input.SortColumn} {input.SortOrder}")
                        .Skip(input.PageIndex * input.PageSize)
                        .Take(input.PageSize);
                result = await query.ToArrayAsync();
                _distributedCache.Set(cacheKey, result, new TimeSpan(0, 0, 30));
            }
            return new RestDTO<Mechanic[]>()
            {
                //Data = await query.ToArrayAsync(),
                Data = result,
                PageIndex = input.PageIndex, 
                PageSize = input.PageSize,
                RecordCount = await _context.BoardGames.CountAsync(), 

                Links = new List<LinkDTO> {
                    new LinkDTO(
                        Url.Action(null, "Mechanics", new {input.PageIndex, input.PageSize },
                        Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        /* POST */
        [Authorize]
        [HttpPost(Name = "UpdateMechanics")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Mechanic?>> Post(MechanicDTO model)
        {
            var mechanic = await _context.Mechanics
                            .Where(b => b.Id == model.Id)
                            .FirstOrDefaultAsync();
            if (mechanic != null)
            {
                if (!string.IsNullOrEmpty(model.Name))
                {
                    mechanic.Name = model.Name;
                }

                mechanic.LastModifiedDate = DateTime.Now;
                _context.Mechanics.Update(mechanic);
                await _context.SaveChangesAsync();
            };
            return new RestDTO<Mechanic?>()
            {
                Data = mechanic,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Mechanics",model,Request.Scheme)!,
                        "self",
                        "POST"),
                }
            };
        }

        [Authorize]
        /* DELETE */
        [HttpDelete(Name = "DeleteMechanics")]
        [ResponseCache(NoStore = true)]
        public async Task<RestDTO<Mechanic?>> Delete(int id)
        {

            var mechanic = await _context.Mechanics
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();
            if (mechanic != null)
            {
                _context.Mechanics.Remove(mechanic);
                await _context.SaveChangesAsync();
            };

            return new RestDTO<Mechanic?>()
            {
                Data = mechanic,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Mechanics", id, Request.Scheme)!,
                        "self",
                        "DELETE"),
                }
            };
        }
    }
}
