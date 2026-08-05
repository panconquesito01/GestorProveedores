using GestorProveedores.Domain.Entities;
using GestorProveedores.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorProveedores.Infrastructure.Persistence.Configurations;

internal sealed class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> builder)
    {
        builder.ToTable("Documentos");

        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
        builder.Ignore(entity => entity.DomainEvents);

        builder.Property(entity => entity.Tipo)
            .HasConversion(
                value => EnumDatabaseValues.ToDatabaseValue(value),
                value => EnumDatabaseValues.ToTipoDocumento(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.NombreArchivo).HasMaxLength(260).IsRequired();
        builder.Property(entity => entity.MimeType).HasMaxLength(150).IsRequired();
        builder.Property(entity => entity.Contenido).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(entity => entity.CreatedAt).HasPrecision(0).IsRequired();
        builder.Property(entity => entity.UpdatedAt).HasPrecision(0).IsRequired();
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasOne(entity => entity.Solicitud)
            .WithMany(solicitud => solicitud.Documentos)
            .HasForeignKey(entity => entity.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.ProveedorCandidato)
            .WithMany()
            .HasForeignKey(entity => entity.ProveedorCandidatoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(entity => entity.UsuarioSubio)
            .WithMany()
            .HasForeignKey(entity => entity.SubidoPor)
            .OnDelete(DeleteBehavior.NoAction);
    }
}