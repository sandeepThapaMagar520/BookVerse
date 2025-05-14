using BookLibraryStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("Customer/api/[controller]/[action]")]
    [ApiController]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var recentBroadcasts = await _context.OrderBroadcasts
                .OrderByDescending(o => o.BroadcastTime)
                .Take(20)
                .Select(o => new { o.Id, o.Message, o.BroadcastTime })
                .ToListAsync();

            return Ok(recentBroadcasts);
        }
    }

}
