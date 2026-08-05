using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class SolicitudHistorialConfiguration : IEntityTypeConfiguration<SolicitudHistorial>
{
    public void Configure(EntityTypeBuilder<SolicitudHistorial> builder)
    {
        builder.ToTable("SolicitudHistorial");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Etapa)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToEtapaSolicitud(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.Accion).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.Comentario);
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();

        builder.HasOne(entity => entity.Solicitud)
            .WithMany(solicitud => solicitud.Historial)
            .HasForeignKey(entity => entity.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Actor)
            .WithMany()
            .HasForeignKey(entity => entity.ActorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}