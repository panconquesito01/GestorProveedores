using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable("Solicitudes");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Radicado).HasMaxLength(50).IsRequired();
        builder.Property(entity => entity.Titulo).HasMaxLength(250).IsRequired();
        builder.Property(entity => entity.Descripcion).IsRequired();
        builder.Property(entity => entity.Frecuencia).HasMaxLength(100);

        builder.Property(entity => entity.ProveedorOrigen)
            .HasConversion(
                value => value.HasValue ? EnumDatabaseValues.ToDatabaseValue(value.Value) : null,
                value => value == null ? null : EnumDatabaseValues.ToOrigenProveedor(value))
            .HasMaxLength(30);

        builder.Property(entity => entity.Etapa)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToEtapaSolicitud(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.Estado)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToEstadoSolicitud(value))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(entity => entity.Radicado).IsUnique();

        builder.HasOne(entity => entity.Solicitante)
            .WithMany()
            .HasForeignKey(entity => entity.SolicitanteId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Empresa)
            .WithMany()
            .HasForeignKey(entity => entity.EmpresaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Aprobador)
            .WithMany()
            .HasForeignKey(entity => entity.AprobadorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Auxiliar)
            .WithMany()
            .HasForeignKey(entity => entity.AuxiliarId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.Analista)
            .WithMany()
            .HasForeignKey(entity => entity.AnalistaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(entity => entity.Proveedores).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.Documentos).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(entity => entity.Historial).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}