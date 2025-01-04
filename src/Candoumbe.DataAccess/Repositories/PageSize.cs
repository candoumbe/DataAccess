namespace Candoumbe.DataAccess.Repositories;

using System;
using System.Numerics;

/// <summary>
/// Value object that holds a page size.
/// </summary>
/// <remarks>
/// An instance of this class behave mostly like an <see langword="int"/>.
/// </remarks>
public sealed record PageSize
#if NET7_0_OR_GREATER
        : IMultiplyOperators<PageSize, PageIndex, int>,
                IMultiplyOperators<PageSize, int, int>,
                IComparisonOperators<PageSize, int, bool>,
                IEqualityOperators<PageSize, int, bool>,
                IEqualityOperators<PageSize, long, bool>,
                IMinMaxValue<PageSize>,
                ISubtractionOperators<PageSize, int, PageSize>
#endif

{
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
        public static PageSize MaxValue => From(int.MaxValue);

        /// <summary>
        /// The value <c>1</c> for the current type
        /// </summary>
        public static PageSize One => From(1);

        /// <summary>
        /// The value of the page size
        /// </summary>
        public int Value { get; }

        private PageSize(int value) => Value = value;

        /// <summary>
        /// Creates a <see cref="PageSize"/> from <paramref name="size"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>a <see cref="PageSize"/> that holds <paramref name="size"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="size"/> is negative.</exception>
        public static PageSize From(int size)
        {
                return size < 0
                        ? throw new ArgumentOutOfRangeException(nameof(size), size, $"{nameof(size)} cannot be less than 0")
                        : new PageSize(size);
        }

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
        public static int operator *(PageSize left, PageIndex right)
                => left * right.Value;

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
                => (left.Value, right) switch
                {
                        (int.MaxValue, _) or (_, int.MaxValue) => MaxValue,
                        (_, 0) => left,
                        _ => (left.Value + right) switch
                        {
                                <= 0 => One,
                                int result => From(result)
                        }
                };

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
                => From((left.Value - right) switch
                {
                        < 1 => 1,
                        int result => result
                });
}