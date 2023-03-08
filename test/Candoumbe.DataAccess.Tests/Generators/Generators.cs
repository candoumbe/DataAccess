namespace Candoumbe.DataAccess.Tests
{
    using Candoumbe.DataAccess.Repositories;
    using Candoumbe.Types.Numerics;

    using FsCheck;
    using FsCheck.Fluent;

    using System.Linq;

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
                                                                  .Select(positiveInt => new PageSize(PositiveInteger.From(positiveInt.Item)));
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
                                                            .Select(positiveInt => new PageIndex(PositiveInteger.From(positiveInt.Item)));
            return Gen.OneOf(randomPageIndexGenerator, Gen.Constant(PageIndex.MinValue), Gen.Constant(PageIndex.MaxValue))
                      .ToArbitrary();
        }


        public static Arbitrary<PositiveInteger> PositiveIntegers()
            => ArbMap.Default.ArbFor<PositiveInt>().Generator
                             .Select(positiveInt => PositiveInteger.From(positiveInt.Item))
                             .ToArbitrary();

    }
}
