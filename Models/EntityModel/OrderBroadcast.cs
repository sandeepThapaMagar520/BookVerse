using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLibraryStore.Models.EntityModel
{
    public class OrderBroadcast
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        [ValidateNever]
        public Order Order { get; set; }

        [Required]
        [StringLength(200)]
        public string Message { get; set; }

        public DateTime BroadcastTime { get; set; } = DateTime.UtcNow;
        public bool IsDisplayed { get; set; } = true;

        public int DisplayDuration { get; set; } = 30;
    }
}
