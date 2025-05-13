using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = SD.Role_Customer)]
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookmarkController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var bookmarks = await _context.Bookmarks
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.AddedAt)
                .ToListAsync();

            return View(bookmarks);
        }


        [HttpPost]
        public async Task<IActionResult> Add(int bookId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Check if already bookmarked
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);

            if (existingBookmark != null)
            {
                TempData["error"] = "This book is already in your bookmarks.";
                return RedirectToAction("Details", "Home", new { id = bookId });
            }

            var bookmark = new Bookmark
            {
                BookId = bookId,
                UserId = userId!,
                AddedAt = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();

            TempData["success"] = "Book added to your bookmarks!";
            return RedirectToAction("Details", "Home", new { id = bookId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (bookmark == null)
            {
                return NotFound();
            }

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            TempData["success"] = "Bookmark removed!";
            return RedirectToAction(nameof(Index));
        }

    }
}
