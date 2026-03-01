using Microsoft.AspNetCore.Identity;

namespace Loco1.Data.Models
    {
    public class ApplicationUser : IdentityUser
        {
        // Soft-delete flag; original fields allow full restore if reactivated
        public bool IsDeactivated { get; set; }

        public string? OriginalEmail { get; set; }
        public string? OriginalUserName { get; set; }

        // New properties for full names
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        }
    }