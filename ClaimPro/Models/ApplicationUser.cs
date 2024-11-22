using Microsoft.AspNetCore.Identity;

namespace ClaimPro.Models
{
    public class ApplicationUser : IdentityUser
    {
        // First name of the user, marked as required
        public required string FirstName { get; set; }

        // Last name of the user, marked as required
        public required string LastName { get; set; }

        // Collection of claims associated with the user
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>(); 
    }
}

