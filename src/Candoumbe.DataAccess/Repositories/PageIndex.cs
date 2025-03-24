namespace Candoumbe.DataAccess.Repositories
{
    using System;
#if NET7_0_OR_GREATER
    using System.Numerics;
#endif

    /// <summary>
    /// A class that allow to specify the index of a page in a result set.
    /// </summary>
    public record PageIndex
#if NET7_0_OR_GREATER
       : IAdditionOperators<PageIndex, int, PageIndex>,
         ISubtractionOperators<PageIndex, int, PageIndex>,
         IMinMaxValue<PageIndex>,
         IMultiplyOperators<PageIndex, int, PageIndex>,
         IComparisonOperators<PageIndex, int, bool>
#endif
    {
        /// <summary>
        /// The value of the page size
        /// </summary>
        public int Value
        {
            get;
#if NET6_0_OR_GREATER
            private init;
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
        public static PageIndex MaxValue => From(int.MaxValue);

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
        public static PageIndex One => From(1);
        
        private PageIndex(int value) => Value = value;

        /// <summary>
        /// Creates a <see cref="PageIndex"/> from <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>a <see cref="PageIndex"/> that holds <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="value"/> &lt; 1 </exception>
        public static PageIndex From(int value)
        {
            return value < 1
                ? throw new ArgumentOutOfRangeException(nameof(value), value, $"'{nameof(value)}' cannot be less than 1")
                : new PageIndex(value);
        }

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
        public static PageIndex operator +(PageIndex left, int right)
        {
            PageIndex pageIndex;
            if (left.Value == int.MaxValue || right == int.MaxValue)
            {
                pageIndex = MaxValue;
            }
            else
            {
                checked
                {
                    try
                    {
                        pageIndex = (left.Value + right) switch
                        {
                            <= 0 => new PageIndex(Math.Min(1, left.Value)),
                            int result => From(result)
                        };
                    }
                    catch (OverflowException)
                    {
                        pageIndex = MaxValue;
                    }
                }
            }

            return pageIndex;
        }

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
        public static PageIndex operator -(PageIndex left, int right)
        {
            int result = left.Value - right;

            return result < 1
                ? One
                : From(left.Value - right);
        }

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
        public static PageIndex operator *(PageIndex left, int right)
            => (left.Value * right) switch
            {
                <= 0 => One,
                int result => From(result)
            };

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
        public static bool operator >(PageIndex left, int right) => left.Value > right;

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
        public static bool operator >=(PageIndex left, int right) => left.Value >= right;

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
        public static bool operator <(PageIndex left, int right)
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
        public static bool operator <=(PageIndex left, int right)
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
        public static bool operator ==(PageIndex left, int right)
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
        public static bool operator !=(PageIndex left, int right)
            => !(left == right);

        /// <summary>
        /// Implicit cast of <see cref="PageIndex"/> into <see langword="int"/>.
        /// </summary>
        /// <param name="index"></param>
        public static implicit operator int(PageIndex index) => index.Value;
    }
}