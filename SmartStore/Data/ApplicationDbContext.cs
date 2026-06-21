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
        }
    }
}
