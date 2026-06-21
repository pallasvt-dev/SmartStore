using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStore.Models;

namespace SmartStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Brand> Brands => Set<Brand>();

        public DbSet<Size> Sizes => Set<Size>();

        public DbSet<Color> Colors => Set<Color>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();

        public DbSet<CartItemEntity> CartItemEntities => Set<CartItemEntity>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        public DbSet<ProductReviewImage> ProductReviewImages => Set<ProductReviewImage>();

        public DbSet<OutfitSuggestion> OutfitSuggestions => Set<OutfitSuggestion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(category => category.Name).IsUnique();
                entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
                entity.Property(category => category.Slug).HasMaxLength(120);
                entity.Property(category => category.Description).HasMaxLength(300);
                entity.Property(category => category.IsActive).HasDefaultValue(true);
            });

            builder.Entity<Brand>(entity =>
            {
                entity.HasIndex(brand => brand.Name).IsUnique();
                entity.Property(brand => brand.Name).HasMaxLength(100).IsRequired();
                entity.Property(brand => brand.Description).HasMaxLength(300);
                entity.Property(brand => brand.IsActive).HasDefaultValue(true);
            });

            builder.Entity<Size>(entity =>
            {
                entity.HasIndex(size => size.Name).IsUnique();
                entity.Property(size => size.Name).HasMaxLength(50).IsRequired();
                entity.Property(size => size.IsActive).HasDefaultValue(true);
            });

            builder.Entity<Color>(entity =>
            {
                entity.HasIndex(color => color.Name).IsUnique();
                entity.Property(color => color.Name).HasMaxLength(50).IsRequired();
                entity.Property(color => color.HexCode).HasMaxLength(20);
                entity.Property(color => color.IsActive).HasDefaultValue(true);
            });

            builder.Entity<Product>(entity =>
            {
                entity.Property(product => product.Name).HasMaxLength(150).IsRequired();
                entity.Property(product => product.Slug).HasMaxLength(180);
                entity.Property(product => product.Description).HasMaxLength(1000).IsRequired();
                entity.Property(product => product.Material).HasMaxLength(100);
                entity.Property(product => product.Gender).HasMaxLength(30);
                entity.Property(product => product.Price).HasColumnType("decimal(18,2)");
                entity.Property(product => product.OldPrice).HasColumnType("decimal(18,2)");
                entity.Property(product => product.Badge).HasMaxLength(30);
                entity.Property(product => product.IsActive).HasDefaultValue(true);

                entity.HasOne(product => product.Category)
                    .WithMany(category => category.Products)
                    .HasForeignKey(product => product.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(product => product.Brand)
                    .WithMany(brand => brand.Products)
                    .HasForeignKey(product => product.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ProductVariant>(entity =>
            {
                entity.HasIndex(variant => variant.Sku).IsUnique();
                entity.Property(variant => variant.Sku).HasMaxLength(80).IsRequired();
                entity.Property(variant => variant.Price).HasColumnType("decimal(18,2)");
                entity.Property(variant => variant.IsActive).HasDefaultValue(true);

                entity.HasOne(variant => variant.Product)
                    .WithMany(product => product.ProductVariants)
                    .HasForeignKey(variant => variant.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(variant => variant.Size)
                    .WithMany(size => size.ProductVariants)
                    .HasForeignKey(variant => variant.SizeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(variant => variant.Color)
                    .WithMany(color => color.ProductVariants)
                    .HasForeignKey(variant => variant.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.Property(image => image.ImageUrl).IsRequired();

                entity.HasOne(image => image.Product)
                    .WithMany(product => product.ProductImages)
                    .HasForeignKey(image => image.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ShoppingCart>(entity =>
            {
                entity.HasIndex(cart => cart.UserId).IsUnique();
                entity.Property(cart => cart.UserId).IsRequired();

                entity.HasOne(cart => cart.User)
                    .WithOne(user => user.ShoppingCart)
                    .HasForeignKey<ShoppingCart>(cart => cart.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItemEntity>(entity =>
            {
                entity.HasIndex(item => new { item.ShoppingCartId, item.ProductVariantId }).IsUnique();
                entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(item => item.ShoppingCart)
                    .WithMany(cart => cart.CartItems)
                    .HasForeignKey(item => item.ShoppingCartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(item => item.ProductVariant)
                    .WithMany(variant => variant.CartItems)
                    .HasForeignKey(item => item.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Order>(entity =>
            {
                entity.HasIndex(order => order.OrderCode).IsUnique();
                entity.Property(order => order.OrderCode).HasMaxLength(30).IsRequired();
                entity.Property(order => order.UserId).IsRequired();
                entity.Property(order => order.CustomerName).HasMaxLength(120).IsRequired();
                entity.Property(order => order.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(order => order.Email).HasMaxLength(120);
                entity.Property(order => order.ShippingAddress).HasMaxLength(300).IsRequired();
                entity.Property(order => order.Note).HasMaxLength(500);
                entity.Property(order => order.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(order => order.ShippingFee).HasColumnType("decimal(18,2)");
                entity.Property(order => order.Discount).HasColumnType("decimal(18,2)");
                entity.Property(order => order.Total).HasColumnType("decimal(18,2)");
                entity.Property(order => order.OrderStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
                entity.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
                entity.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(30).IsRequired();

                entity.HasOne(order => order.User)
                    .WithMany(user => user.Orders)
                    .HasForeignKey(order => order.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.Property(item => item.ProductName).HasMaxLength(150).IsRequired();
                entity.Property(item => item.SizeName).HasMaxLength(50).IsRequired();
                entity.Property(item => item.ColorName).HasMaxLength(50).IsRequired();
                entity.Property(item => item.Sku).HasMaxLength(80).IsRequired();
                entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(item => item.LineTotal).HasColumnType("decimal(18,2)");

                entity.HasOne(item => item.Order)
                    .WithMany(order => order.OrderItems)
                    .HasForeignKey(item => item.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(item => item.ProductVariant)
                    .WithMany(variant => variant.OrderItems)
                    .HasForeignKey(item => item.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
                entity.Property(item => item.UserId).IsRequired();

                entity.HasOne(item => item.User)
                    .WithMany(user => user.WishlistItems)
                    .HasForeignKey(item => item.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(item => item.Product)
                    .WithMany(product => product.WishlistItems)
                    .HasForeignKey(item => item.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ProductReview>(entity =>
            {
                entity.HasIndex(review => new { review.UserId, review.ProductId }).IsUnique();
                entity.ToTable(table => table.HasCheckConstraint("CK_ProductReviews_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
                entity.Property(review => review.UserId).IsRequired();
                entity.Property(review => review.Comment).HasMaxLength(1000).IsRequired();
                entity.Property(review => review.IsApproved).HasDefaultValue(true);

                entity.HasOne(review => review.Product)
                    .WithMany(product => product.ProductReviews)
                    .HasForeignKey(review => review.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(review => review.User)
                    .WithMany(user => user.ProductReviews)
                    .HasForeignKey(review => review.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(review => review.Order)
                    .WithMany(order => order.ProductReviews)
                    .HasForeignKey(review => review.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ProductReviewImage>(entity =>
            {
                entity.Property(image => image.ImageUrl).HasMaxLength(2000).IsRequired();

                entity.HasOne(image => image.ProductReview)
                    .WithMany(review => review.Images)
                    .HasForeignKey(image => image.ProductReviewId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<OutfitSuggestion>(entity =>
            {
                entity.HasIndex(item => new { item.ProductId, item.SuggestedProductId }).IsUnique();
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_OutfitSuggestions_DifferentProducts",
                    "[ProductId] <> [SuggestedProductId]"));
                entity.Property(item => item.Title).HasMaxLength(120);
                entity.Property(item => item.Note).HasMaxLength(300);
                entity.Property(item => item.IsActive).HasDefaultValue(true);

                entity.HasOne(item => item.Product)
                    .WithMany(product => product.OutfitSuggestions)
                    .HasForeignKey(item => item.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(item => item.SuggestedProduct)
                    .WithMany(product => product.SuggestedInOutfits)
                    .HasForeignKey(item => item.SuggestedProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
