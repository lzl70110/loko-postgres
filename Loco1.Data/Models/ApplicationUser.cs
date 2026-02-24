using Microsoft.AspNetCore.Identity;

namespace Loco1.Data.Models
    {
    public class ApplicationUser : IdentityUser
        {
        // Soft-delete flag; original fields allow full restore if reactivated
        public bool IsDeactivated { get; set; }

        public string? OriginalEmail { get; set; }
        public string? OriginalUserName { get; set; }
        }
    }