using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLibraryStore.Models.EntityModel
{
    public class Book
    {
        // Primary identifier
        [Key]
        public int Id { get; set; }

        // Core book information
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }

        // Categorization fields (for filtering)
        public string Genre { get; set; }
        public string Publisher { get; set; }
        public string Language { get; set; }

        // Format and edition details
        [Required(ErrorMessage = "Please select a format.")]
        public string Format { get; set; } // Paperback, Hardcover, eBook
        [Required(ErrorMessage = "Please select an edition type.")]
        public string EditionType { get; set; } // Signed, Limited, First, Collector's, etc.
        public int PageCount { get; set; }

        // Pricing information
        public decimal Price { get; set; }
        public bool IsOnSale { get; set; } = false;
        public decimal DiscountPercentage { get; set; } = 0;
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        // Availability information
        public int StockQuantity { get; set; } = 0;
        public bool IsAvailableInLibrary { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Dates for filtering and categorization
        public DateTime PublicationDate { get; set; }
        public DateTime? ListedDate { get; set; } = DateTime.UtcNow;

        // Special categorization flags
        public bool IsAwardWinner { get; set; } = false;
        public bool IsComingSoon { get; set; } = false;

        // Metrics for sorting and popularity
        public int SalesCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        // Admin and system fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Cover image URL
        [ValidateNever]
        public string? CoverImageUrl { get; set; }
    }
}
