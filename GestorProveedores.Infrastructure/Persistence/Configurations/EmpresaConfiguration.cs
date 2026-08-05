using GestorProveedores.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Nombre).HasMaxLength(250).IsRequired();
        builder.Property(entity => entity.Nit).HasMaxLength(50).IsRequired();
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();

        builder.HasIndex(entity => entity.Nit).IsUnique();
    }
}