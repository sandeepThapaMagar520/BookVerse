using BookLibraryStore.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookLibraryStore.ViewComponents
{
    public class AnnouncementBannerViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementBannerViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var now = DateTime.UtcNow;
            var announcements = _context.Announcements
                .Where(a => a.IsActive && a.StartDate <= now && a.EndDate >= now)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return View(announcements);
        }
    }

}
