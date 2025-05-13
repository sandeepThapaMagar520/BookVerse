using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using BookLibraryStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibraryStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var books = _context.Books
                .Where(b => b.IsActive)
                .ToList();

            return View(books);
        }

        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateBook(Book book, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("CoverImageUrl", "The file field is required.");
                return View(book);
            }

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var bookPath = Path.Combine(wwwRootPath, @"images\book");

            // Ensure directory exists
            if (!Directory.Exists(bookPath))
            {
                Directory.CreateDirectory(bookPath);
            }

            using var fileStream = new FileStream(Path.Combine(bookPath, fileName), FileMode.Create);
            file.CopyTo(fileStream);

            book.CoverImageUrl = @"/images/book/" + fileName;

            _context.Books.Add(book);
            _context.SaveChanges();
            TempData["success"] = "Book added successful";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id && b.IsActive);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        public IActionResult Edit(Book updatedBook)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedBook);
            }

            var existingBook = _context.Books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Update fields (except CoverImageUrl)
            existingBook.Title = updatedBook.Title;
            existingBook.ISBN = updatedBook.ISBN;
            existingBook.Author = updatedBook.Author;
            existingBook.Description = updatedBook.Description;
            existingBook.Genre = updatedBook.Genre;
            existingBook.Publisher = updatedBook.Publisher;
            existingBook.Language = updatedBook.Language;
            existingBook.Format = updatedBook.Format;
            existingBook.EditionType = updatedBook.EditionType;
            existingBook.PageCount = updatedBook.PageCount;
            existingBook.Price = updatedBook.Price;
            existingBook.IsOnSale = updatedBook.IsOnSale;
            existingBook.DiscountPercentage = updatedBook.DiscountPercentage;
            existingBook.DiscountStartDate = updatedBook.DiscountStartDate?.ToUniversalTime();
            existingBook.DiscountEndDate = updatedBook.DiscountEndDate?.ToUniversalTime();
            existingBook.StockQuantity = updatedBook.StockQuantity;
            existingBook.IsAvailableInLibrary = updatedBook.IsAvailableInLibrary;
            existingBook.IsActive = updatedBook.IsActive;
            existingBook.PublicationDate = updatedBook.PublicationDate.ToUniversalTime();
            existingBook.IsAwardWinner = updatedBook.IsAwardWinner;
            existingBook.IsComingSoon = updatedBook.IsComingSoon;
            existingBook.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            TempData["success"] = "Book Updated successful";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);

            book.IsActive = false;
            _context.SaveChanges();
            TempData["success"] = "Book deleted successful";
            return RedirectToAction(nameof(Index));
        }
    }
}
