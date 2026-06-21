using Microsoft.AspNetCore.Identity;

namespace SmartStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public ShoppingCart? ShoppingCart { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
