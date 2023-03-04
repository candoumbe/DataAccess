namespace Candoumbe.DataAccess.Repositories
{
    using Candoumbe.Types.Numerics;

    using System;
    using System.Numerics;

    /// <summary>
    /// Value object that holds a page size.
    /// </summary>
    /// <remarks>
    /// An instance of this class behave mostly like an <see langword="int"/>.
    /// </remarks>
    public readonly record struct PageSize
#if NET7_0_OR_GREATER
        : IMultiplyOperators<PageSize, PageIndex, NonNegativeInteger>,
          IMultiplyOperators<PageSize, int, int>,
          IComparisonOperators<PageSize, int, bool>,
          IEqualityOperators<PageSize, long, bool>,
          IMinMaxValue<PageSize>,
          ISubtractionOperators<PageSize, int, PageSize>
#endif

    {

        private static readonly Lazy<PageSize> MaxPageSize = new(() => new PageSize(PositiveInteger.MaxValue));
        private static readonly Lazy<PageSize> MinPageSize = new(() => new PageSize(PositiveInteger.MinValue));
        private static readonly Lazy<PageSize> OnePageSize = new(() => new PageSize(PositiveInteger.One));

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Represents the smallest possible value of <see cref="PageSize"/>.
        /// </summary>
#endif
        public static PageSize MinValue => One;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Represents the largest possible value of <see cref="PageSize"/>.
        /// </summary>
#endif
        public static PageSize MaxValue => MaxPageSize.Value;

        /// <summary>
        /// The value <c>1</c> for the current type
        /// </summary>
        public static PageSize One => OnePageSize.Value;

        /// <summary>
        /// The value of the page size
        /// </summary>
        public PositiveInteger Value { get; }

        /// <summary>
        /// Builds a <see cref="PageSize"/> instance with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        public PageSize(PositiveInteger value) => Value = value;

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Multiplies a <see cref="PageSize"/> by a <see cref="PageIndex"/>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The result of <paramref name="left"/> * <paramref name="right"/>.</returns>
#endif
        public static NonNegativeInteger operator *(PageSize left, PageIndex right)
            => NonNegativeInteger.From(left * right.Value);

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Multiplies two values together to compute their product
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>the result of <paramref name="left"/> multiplied by <paramref name="right"/>.</returns>
#endif
        public static int operator *(PageSize left, int right)
            => left.Value * right;

        ///<inheritdoc/>
        public static implicit operator int(PageSize right) => right.Value;

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
        public static bool operator >(PageSize left, int right) => left.Value > right;

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
        public static bool operator >=(PageSize left, int right) => left.Value >= right;

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
        public static bool operator <(PageSize left, int right)
            => left.Value < right;

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
        public static bool operator <=(PageSize left, int right)
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
        public static bool operator ==(PageSize left, int right)
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
        public static bool operator !=(PageSize left, int right)
            => !(left == right);

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
        public static bool operator ==(PageSize left, long right)
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
        public static bool operator !=(PageSize left, long right)
            => !(left == right);

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Adds two values together and computes their sum
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
#endif
        public static PageSize operator +(PageSize left, int right)
            => new PageSize(PositiveInteger.From(left.Value * right));

#if NET7_0_OR_GREATER
        ///<inheritdoc/>
#else
        /// <summary>
        /// Substracts two values together and computes their sum
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>The result of <paramref name="left"/> <c>-</c> <paramref name="right"/>.</returns>
#endif
        public static PageSize operator -(PageSize left, int right)
            => new PageSize(PositiveInteger.From(left.Value * right));
    }
}