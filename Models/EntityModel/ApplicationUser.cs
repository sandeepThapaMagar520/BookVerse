using Microsoft.AspNetCore.Identity;

namespace BookLibraryStore.Models.EntityModel
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MembershipId { get; set; }
        public int SuccessfulOrderCount { get; set; } = 0;
        public bool HasStackableDiscount { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }
}
