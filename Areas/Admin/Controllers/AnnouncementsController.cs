using BookLibraryStore.Data;
using BookLibraryStore.Models.EntityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLibraryStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var announcements = _context.Announcements.OrderByDescending(a => a.CreatedAt).ToList();
            return View(announcements);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.UtcNow;
                _context.Announcements.Add(announcement);
                _context.SaveChanges();
                TempData["success"] = "Announcement created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public IActionResult Edit(int id)
        {
            var announcement = _context.Announcements.Find(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        [HttpPost]
        public IActionResult Edit(int id, Announcement announcement)
        {
            if (id != announcement.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Announcements.Update(announcement);
                _context.SaveChanges();
                TempData["success"] = "Announcement updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public IActionResult Delete(int id)
        {
            var announcement = _context.Announcements.Find(id);
            if (announcement == null)
            {
                return NotFound();
            }

            _context.Announcements.Remove(announcement);
            _context.SaveChanges();
            TempData["success"] = "Announcement deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
