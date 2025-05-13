using System.Diagnostics;
using System.Security.Claims;
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
    public class HomeController : Controller
    {
        

        public IActionResult Index()
        {
            // Main book list - latest 20
            var books = _context.Books
                .Where(b => b.IsActive && b.StockQuantity > 0)
                .OrderByDescending(b => b.CreatedAt)
                .Take(6)
                .ToList();

            // Get top 3 most ordered book IDs
            var topOrderedBookIds = _context.OrderItems
                .GroupBy(oi => oi.BookId)
                .Select(group => new
                {
                    BookId = group.Key,
                    TotalQuantity = group.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(3)
                .Select(g => g.BookId)
                .ToList();

            List<Book> topOrderedBooks;

            if (topOrderedBookIds.Any())
            {
                // Fetch top ordered books
                topOrderedBooks = _context.Books
                    .Where(b => topOrderedBookIds.Contains(b.Id) && b.IsActive && b.StockQuantity > 0)
                    .ToList();
            }
            else
            {
                // Fetch 3 random books if no orders exist
                topOrderedBooks = _context.Books
                    .Where(b => b.IsActive && b.StockQuantity > 0)
                    .OrderBy(b => Guid.NewGuid())
                    .Take(3)
                    .ToList();
            }

            ViewBag.TopOrderedBooks = topOrderedBooks;

            return View(books);
        }


        public IActionResult Details(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id && b.IsActive);
            if (book == null)
            {
                return NotFound();
            }

            var hasPurchase = false;
            var isReview = false;
            string userId = null;

            if (User.Identity is { IsAuthenticated: true })
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                hasPurchase = _context.OrderItems
                    .Include(o => o.Order)
                    .Any(o => o.BookId == id && o.Order.UserId == userId && o.Order.Status == "Completed");

                isReview = _context.Reviews.Any(r => r.UserId == userId && r.BookId == id);
            }

            ViewBag.hasPurchase = hasPurchase;
            ViewBag.isReview = isReview;

            // Get all reviews for this book
            var reviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == id)
                .AsQueryable();

            Review userReview = null;
            List<Review> otherReviews;

            if (!string.IsNullOrEmpty(userId))
            {
                // Logged-in: separate user�s review and other reviews
                userReview = reviewsQuery.FirstOrDefault(r => r.UserId == userId);
                otherReviews = reviewsQuery.Where(r => r.UserId != userId).ToList();
            }
            else
            {
                // Not logged-in: all reviews as others
                otherReviews = reviewsQuery.ToList();
            }

            ViewBag.UserReview = userReview;
            ViewBag.OtherReviews = otherReviews;

            return View(book);
        }

        // Updated Controller Method
        public IActionResult Catalog(string search, string genre, string language, string sortOrder, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Books.Where(b => b.IsActive);

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            // Genre filter
            if (!string.IsNullOrEmpty(genre))
            {
                query = query.Where(b => b.Genre == genre);
            }

            // Language filter
            if (!string.IsNullOrEmpty(language))
            {
                query = query.Where(b => b.Language == language);
            }

            // Sorting
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(b => b.Price);
                    break;
                case "rating":
                    query = query.OrderByDescending(b => b.AverageRating);
                    break;
                case "newest":
                    query = query.OrderByDescending(b => b.PublicationDate);
                    break;
                default:
                    query = query.OrderBy(b => b.Title);
                    break;
            }

            // Pagination
            var totalItems = query.Count();
            var books = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Get distinct list of languages
            var languages = _context.Books
                .Where(b => b.IsActive && !string.IsNullOrEmpty(b.Language))
                .Select(b => b.Language)
                .Distinct()
                .ToList();

            // Get distinct list of genres
            var genres = _context.Books
                .Where(b => b.IsActive && !string.IsNullOrEmpty(b.Genre))
                .Select(b => b.Genre)
                .Distinct()
                .ToList();

            ViewBag.Genres = genres; // List of all available genres
            ViewBag.Languages = languages; // List of all available languages
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SelectedGenre = genre; // Currently selected genre - for dropdown
            ViewBag.Genre = genre; // Keep this for backward compatibility with pagination
            ViewBag.Language = language; // Currently selected language
            ViewBag.SortOrder = sortOrder;

            return View(books);
        }

        [Authorize]
        public IActionResult UserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == userId);

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["error"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(UserProfile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(UserProfile));
            }

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction(nameof(UserProfile));
            }

            var error = string.Join(" ", result.Errors.Select(e => e.Description));
            TempData["error"] = error;
            return RedirectToAction(nameof(UserProfile));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction("UserProfile");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.StreetAddress = model.StreetAddress;
            user.City = model.City;
            user.State = model.State;
            user.PostalCode = model.PostalCode;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["success"] = "Profile updated successfully.";
            return RedirectToAction("UserProfile");
        }

    }
}
