using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions
{
    /// <summary>
    /// A filter specification that combines two other filter specifications with an AND operator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AndFilterSpecification<T> : IFilterSpecification<T>
    {
        /// <inheritdoc />
        public Expression<Func<T, bool>> Filter { get; }

        /// <summary>
        /// Builds a new <see cref="AndFilterSpecification{T}"/> instance.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
        public AndFilterSpecification(IFilterSpecification<T> left, IFilterSpecification<T> right)
        {
            Filter = left.Filter.AndAlso(right.Filter);
        }
    }
}