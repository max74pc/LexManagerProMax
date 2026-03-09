using LexManagerProMax.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexManagerProMax.Infrastructure.Persistence.Configurations;

public class SoggettoConfiguration : IEntityTypeConfiguration<Soggetto>
{
    public void Configure(EntityTypeBuilder<Soggetto> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Cognome).HasMaxLength(100);
        builder.Property(s => s.Nome).HasMaxLength(100);
        builder.Property(s => s.CodiceFiscale).HasMaxLength(16);
        builder.Property(s => s.RagioneSociale).HasMaxLength(200);
        builder.Property(s => s.PartitaIva).HasMaxLength(11);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Pec).HasMaxLength(150);
        builder.Property(s => s.Telefono).HasMaxLength(20);
        builder.Property(s => s.Via).HasMaxLength(200);
        builder.Property(s => s.Citta).HasMaxLength(100);
        builder.Property(s => s.Cap).HasMaxLength(10);
        builder.Property(s => s.Provincia).HasMaxLength(2);

        builder.HasIndex(s => s.CodiceFiscale).IsUnique().HasFilter("[CodiceFiscale] IS NOT NULL");
        builder.HasIndex(s => s.PartitaIva).IsUnique().HasFilter("[PartitaIva] IS NOT NULL");
        builder.HasIndex(s => new { s.Cognome, s.Nome });
    }
}