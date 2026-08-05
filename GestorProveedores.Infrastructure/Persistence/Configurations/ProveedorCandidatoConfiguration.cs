using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class ProveedorCandidatoConfiguration : IEntityTypeConfiguration<ProveedorCandidato>
{
    public void Configure(EntityTypeBuilder<ProveedorCandidato> builder)
    {
        builder.ToTable("ProveedoresCandidatos");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Origen)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToOrigenProveedor(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.Nombre).HasMaxLength(250).IsRequired();
        builder.Property(entity => entity.Nit).HasMaxLength(50);
        builder.Property(entity => entity.IdentificadorErp).HasMaxLength(100);
        builder.Property(entity => entity.CorreoContacto).HasMaxLength(320);
        builder.Property(entity => entity.TelefonoContacto).HasMaxLength(50);
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne(entity => entity.Solicitud)
            .WithMany(solicitud => solicitud.Proveedores)
            .HasForeignKey(entity => entity.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}