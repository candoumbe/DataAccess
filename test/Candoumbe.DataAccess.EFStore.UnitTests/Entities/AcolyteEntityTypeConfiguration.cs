namespace Candoumbe.DataAccess.EFStore.UnitTests.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AcolyteEntityTypeConfiguration : IEntityTypeConfiguration<Acolyte>
{
    public void Configure(EntityTypeBuilder<Acolyte> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name);
        builder.HasMany(x => x.Weapons)
            .WithOne();
    }
}