namespace Candoumbe.DataAccess.EFStore.UnitTests.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WeaponEntityTypeConfiguration : IEntityTypeConfiguration<Weapon>
{
    public void Configure(EntityTypeBuilder<Weapon> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Level);
        builder.Property(x => x.Name);
    }
}