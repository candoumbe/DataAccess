namespace Candoumbe.DataAccess.EFStore.UnitTests.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class HeroEntityTypeConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasMany(x => x.Acolytes)
            .WithOne();
    }
}