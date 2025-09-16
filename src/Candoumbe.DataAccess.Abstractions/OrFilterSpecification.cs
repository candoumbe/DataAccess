using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions
{
    /// <summary>
    /// A specification that combines two other filter specifications with an OR operator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrFilterSpecification<T> : IFilterSpecification<T>
    {
        /// <inheritdoc />
        public Expression<Func<T, bool>> Filter { get; }

        /// <summary>
        /// Builds a new <see cref="OrFilterSpecification{T}"/> instance.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="left"/> or <paramref name="right"/> is <see langword="null"/>.</exception>
        public OrFilterSpecification(IFilterSpecification<T> left, IFilterSpecification<T> right)
        {
            Filter = left.Filter.OrElse(right.Filter);
        }
    
    }
}