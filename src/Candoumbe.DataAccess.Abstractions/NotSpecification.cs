using System;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions
{
    /// <summary>
    /// A specification that negates another specification.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotSpecification<T> : IFilterSpecification<T>
    {
        /// <inheritdoc />
        public Expression<Func<T, bool>> Filter { get; }

        /// <summary>
        /// Builds a new <see cref="NotSpecification{T}"/> instance.
        /// </summary>
        /// <param name="specification">The specification onto which the NOT operator will be applied</param>
        /// <exception cref="ArgumentNullException">if <paramref name="specification"/> is <see langword="null"/>.</exception>
        public NotSpecification(IFilterSpecification<T> specification)
        {
            if (specification is null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            Filter = Expression.Lambda<Func<T, bool>>(Expression.Not(specification.Filter.Body), specification.Filter.Parameters);
        }
    }
}