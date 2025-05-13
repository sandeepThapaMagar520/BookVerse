using System.ComponentModel.DataAnnotations;

namespace BookLibraryStore.Models.EntityModel
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; }

        [Required]
        public string Type { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Tracking fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
