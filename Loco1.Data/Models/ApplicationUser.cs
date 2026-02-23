using Microsoft.AspNetCore.Identity;

namespace Loco1.Data.Models
    {
    public class ApplicationUser : IdentityUser
        {
        // EN: Soft-delete support (full restore needs the originals)
        public bool IsDeactivated { get; set; }

        public string? OriginalEmail { get; set; }
        public string? OriginalUserName { get; set; }
        }
    }