namespace SmartStore.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<CartItemEntity> CartItems { get; set; } = new List<CartItemEntity>();
    }
}
