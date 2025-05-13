using System.Diagnostics;
using System.Security.Claims;
using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var books = _context.Books
                .Where(b => b.IsActive && b.StockQuantity > 0)
                .OrderByDescending(b => b.CreatedAt)
                .Take(20)
                .ToList();

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
                // Logged-in: separate user’s review and other reviews
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

        public IActionResult Catalog(string search, string genre, string language, string sortOrder, int page = 1)
        {
            int pageSize = 9;

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

            var languages = _context.Books
                .Where(b => b.IsActive && !string.IsNullOrEmpty(b.Language))
                .Select(b => b.Language)
                .Distinct()
                .ToList();

            ViewBag.Languages = languages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.Genre = genre;
            ViewBag.Language = language;
            ViewBag.SortOrder = sortOrder;

            return View(books);
        }

    }
}
