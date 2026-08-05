using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class AsignacionContadorConfiguration : IEntityTypeConfiguration<AsignacionContador>
{
    public void Configure(EntityTypeBuilder<AsignacionContador> builder)
    {
        builder.ToTable("AsignacionContadores");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("Rol")
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToRolUsuario(value))
            .HasMaxLength(30)
            .ValueGeneratedNever();

        builder.Ignore(entity => entity.Rol);
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.UltimoIndice).IsRequired();
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}