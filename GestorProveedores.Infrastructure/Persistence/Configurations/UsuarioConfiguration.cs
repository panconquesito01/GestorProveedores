using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Nombre).HasMaxLength(250).IsRequired();
        builder.Property(entity => entity.Email).HasMaxLength(320).IsRequired();
        builder.Property(entity => entity.Username).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.Activo).IsRequired();
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.Property(entity => entity.Rol)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToRolUsuario(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(entity => entity.Email).IsUnique();
        builder.HasIndex(entity => entity.Username).IsUnique();

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}