using LexManagerProMax.Domain.Entities.Anagrafica;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexManagerProMax.Infrastructure.Persistence.Configurations;

public class FascicoloConfiguration : IEntityTypeConfiguration<Fascicolo>
{
    public void Configure(EntityTypeBuilder<Fascicolo> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Numero).HasMaxLength(50).IsRequired();
        builder.Property(f => f.Oggetto).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Tribunale).HasMaxLength(200);
        builder.Property(f => f.NrRG).HasMaxLength(50);
        builder.Property(f => f.Giudice).HasMaxLength(200);

        builder.HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Controparti)
                .WithOne(fc => fc.Fascicolo)
                .HasForeignKey(fc => fc.FascicoloId);

        builder.HasIndex(f => f.Numero).IsUnique();
    }
}

