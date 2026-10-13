using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Sku).HasMaxLength(Product.SkuMaxLength).IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(product => product.Category).HasMaxLength(Product.CategoryMaxLength).IsRequired();
        builder.HasIndex(product => product.Category);

        builder.ComplexProperty(product => product.Price, price =>
        {
            price.Property(money => money.Amount).HasColumnName("price_amount").HasPrecision(18, 2);
            price.Property(money => money.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });

        builder.Ignore(product => product.DomainEvents);
    }
}
