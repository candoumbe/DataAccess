using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions
{
    /// <summary>
    /// Wrapper for a single order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleOrderSpecification<T> : IOrderSpecification<T>
    {
        private readonly (Expression<Func<T, object>> Expression, OrderDirection Direction) _order;

        /// <inheritdoc />
        public IReadOnlyList<(Expression<Func<T, object>> Expression, OrderDirection Direction)> Orders => [_order];

        /// <summary>
        /// Builds a new <see cref="SingleOrderSpecification{T}"/> instance.
        /// </summary>
        /// <param name="expression">Defines the property onto which the order should apply</param>
        /// <param name="direction"></param>
        /// <exception cref="ArgumentNullException">if <param name="expression"> is <see langword="null"/>.</param> </exception>
        public SingleOrderSpecification(Expression<Func<T, object>> expression, OrderDirection direction = OrderDirection.Ascending)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            _order = (expression, direction);
        }
    }
}