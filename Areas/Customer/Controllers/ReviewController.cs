using System.Security.Claims;
using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult SubmitReview(Review review)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            review.UserId = userId;

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid review data.";
                return RedirectToAction("Details", "Home", new { id = review.BookId });
            }

            // Get OrderItemId (ensure valid purchase)
            var orderItem = _context.OrderItems
                .Include(o => o.Order)
                .FirstOrDefault(o => o.BookId == review.BookId && o.Order.UserId == userId && o.Order.Status == "Completed");

            if (orderItem == null)
            {
                TempData["error"] = "Cannot review this book without purchase.";
                return RedirectToAction("Details", "Home", new { id = review.BookId });
            }

            review.OrderItemId = orderItem.Id;

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // recalculate AverageRating and ReviewCount
            var book = _context.Books.FirstOrDefault(b => b.Id == review.BookId);
            if (book != null)
            {
                var allReviews = _context.Reviews.Where(r => r.BookId == review.BookId).ToList();

                book.ReviewCount = allReviews.Count;
                book.AverageRating = allReviews.Average(r => r.Rating);

                _context.Books.Update(book);
                _context.SaveChanges();
            }

            TempData["success"] = "Review submitted successfully!";
            return RedirectToAction("Details", "Home", new { id = review.BookId });
        }
    }
}
