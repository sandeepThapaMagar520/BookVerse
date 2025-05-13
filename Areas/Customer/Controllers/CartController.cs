using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var cartItems = _context.CartItems
                .Include(c => c.Book)
                .Where(c => c.UserId == userId)
                .ToList();

            return View(cartItems);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Customer)]
        public IActionResult AddToCart(int bookId, int quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var book = _context.Books.FirstOrDefault(b => b.Id == bookId && b.IsActive);
            if (book == null)
            {
                return NotFound();
            }

            var existingCartItem = _context.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.BookId == bookId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow,
                    UnitPrice = book.Price
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index)); // Redirect to cart page (optional)
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _context.CartItems.Include(c => c.Book).FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

            var book = cartFromDb.Book;
            if (book == null)
            {
                TempData["error"] = "Book not found.";
                return RedirectToAction(nameof(Index));
            }

            if (cartFromDb.Quantity + 1 > book.StockQuantity)
            {
                TempData["error"] = "Insufficient stock.";
                return RedirectToAction(nameof(Index));
            }

            cartFromDb.Quantity += 1;
            cartFromDb.UpdatedAt = DateTime.UtcNow;

            _context.CartItems.Update(cartFromDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

            if (cartFromDb.Quantity <= 1)
            {
                // Remove item from cart
                _context.CartItems.Remove(cartFromDb);
            }
            else
            {
                // Decrease quantity by 1
                cartFromDb.Quantity -= 1;
                cartFromDb.UpdatedAt = DateTime.UtcNow;
                _context.CartItems.Update(cartFromDb);
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                TempData["error"] = "Cart item not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.CartItems.Remove(cartFromDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
