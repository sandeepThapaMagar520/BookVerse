using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLibraryStore.Models.EntityModel
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string ClaimCode { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? CancellationDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string Status { get; set; }

        // Financial information
        public decimal Subtotal { get; set; } // Sum of all items before discounts
        public decimal BulkDiscountAmount { get; set; } // 5% discount for 5+ books
        public decimal LoyaltyDiscountAmount { get; set; } // 10% discount after 10 orders
        public decimal TotalDiscountAmount { get; set; } // Sum of all discounts
        public decimal GrandTotal { get; set; } // Final amount after all discounts

        // Discount flags
        public bool Is5PercentDiscountApplied { get; set; }
        public bool Is10PercentDiscountApplied { get; set; }
    }
}
