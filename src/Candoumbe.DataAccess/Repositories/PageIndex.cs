namespace Candoumbe.DataAccess.Repositories
{
    using Candoumbe.Types.Numerics;

    using System;
#if NET7_0_OR_GREATER
    using System.Numerics;
#endif

    /// <summary>
    /// A class that allow to specify the index of a page in a result set.
    /// </summary>
    public record struct PageIndex
#if NET7_0_OR_GREATER
       : IAdditionOperators<PageIndex, PositiveInteger, PageIndex>,
         ISubtractionOperators<PageIndex, PositiveInteger, PageIndex>,
         IMinMaxValue<PageIndex>,
         IMultiplyOperators<PageIndex, PositiveInteger, PageIndex>,
         IComparisonOperators<PageIndex, NonNegativeInteger, bool>
#endif
    {
        private static readonly Lazy<PageIndex> MaxPageIndex = new(() => new PageIndex(PositiveInteger.MaxValue));
        private static readonly Lazy<PageIndex> MinPageIndex = new(() => new PageIndex(PositiveInteger.MinValue));
        private static readonly Lazy<PageIndex> OnePageIndex = new(() => new PageIndex(PositiveInteger.One));

        /// <summary>
        /// The value of the page size
        /// </summary>
        public PositiveInteger Value
        {
            get;
#if NET6_0_OR_GREATER
            init;
#else
            private set;
#endif
        }

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Gets the maximum value of <see cref="PageIndex"/>
        /// </summary>
#endif
        public static PageIndex MaxValue => MaxPageIndex.Value;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Gets the minimum value of <see cref="PageIndex"/>
        /// </summary>
#endif
        public static PageIndex MinValue => One;

        /// <summary>
        /// The minimum acceptable value for a <see cref="PageIndex"/>
        /// </summary>
        public static PageIndex One => OnePageIndex.Value;

        /// <summary>
        /// Builds a new <see cref="PageIndex"/> instance.
        /// </summary>
        /// <param name="value"></param>
        public PageIndex(PositiveInteger value) => Value = value;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Adds two values together and computes their sum
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/></returns>
#endif
        public static PageIndex operator +(PageIndex left, PositiveInteger right) => left with { Value = left.Value + right };

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Substracts two values together and computes their sum
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/></returns>
#endif
        public static PageIndex operator -(PageIndex left, PositiveInteger right) => left with { Value = left.Value - right };

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Multiplies two values together to compute their product.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The product of <paramref name="left"/> multiplied by <paramref name="right"/>.</returns>
#endif
        public static PageIndex operator *(PageIndex left, PositiveInteger right) => left with { Value = left.Value * right };

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine which is greater
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is strictly greater than <paramref name="right"/> and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator >(PageIndex left, NonNegativeInteger right) => left.Value > right;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine which is greater
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/> and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator >=(PageIndex left, NonNegativeInteger right) => left.Value >= right;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine which is less
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/> and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator <(PageIndex left, NonNegativeInteger right) => left.Value < right;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine which is less
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is strictly greater than <paramref name="right"/> and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator <=(PageIndex left, NonNegativeInteger right)
            => left.Value <= right;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine equality
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator ==(PageIndex left, NonNegativeInteger right)
            => left.Value == right;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Compares the two values to determine inequality
        /// </summary>
        /// <param name="left">the value to compare with <paramref name="right"/>.</param>
        /// <param name="right">the value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal and <see langword="false"/> otherwise</returns>
#endif
        public static bool operator !=(PageIndex left, NonNegativeInteger right)
            => !(left == right);

        /// <summary>
        /// Implicit cast of <see cref="PageIndex"/> into <see langword="int"/>.
        /// </summary>
        /// <param name="index"></param>
        public static implicit operator int(PageIndex index) => index.Value;
    }
}