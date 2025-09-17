using Candoumbe.DataAccess.Repositories;
using FsCheck;
using FsCheck.Fluent;

namespace Candoumbe.DataAccess.UnitTests.Generators;

/// <summary>
/// Generators for FsCheck
/// </summary>
public static class Generators
{
    /// <summary>
    /// Produces <see cref="Arbitrary{PageSize}"/>
    /// </summary>
    public static Arbitrary<PageSize> PageSizes()
    {
        Gen<PageSize> randomPageSizeGenerator = ArbMap.Default.ArbFor<PositiveInt>()
            .Generator
            .Select(positiveInt => PageSize.From(positiveInt.Item));
        return Gen.OneOf(randomPageSizeGenerator, Gen.Constant(PageSize.MinValue), Gen.Constant(PageSize.MaxValue))
            .ToArbitrary();
    }

    /// <summary>
    /// Produces <see cref="Arbitrary{PageIndex}"/>
    /// </summary>
    public static Arbitrary<PageIndex> PageIndexes()
    {
        Gen<PageIndex> randomPageIndexGenerator = ArbMap.Default.ArbFor<PositiveInt>()
            .Generator
            .Select(positiveInt => PageIndex.From(positiveInt.Item));
        return Gen.OneOf(randomPageIndexGenerator, Gen.Constant(PageIndex.MinValue), Gen.Constant(PageIndex.MaxValue))
            .ToArbitrary();
    }
}