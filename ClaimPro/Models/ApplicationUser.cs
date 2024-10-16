using Microsoft.AspNetCore.Identity;

namespace ClaimPro.Models
{
    public class ApplicationUser : IdentityUser
    {

        public required string FirstName { get; set; } 

        public required string LastName { get; set; } 

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
