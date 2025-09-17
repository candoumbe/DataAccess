using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Candoumbe.DataAccess.Abstractions
{
    /// <inheritdoc />
    public class MultiOrderSpecification<T> : IOrderSpecification<T>
    {
        private readonly List<(Expression<Func<T, object>> Expression, OrderDirection Direction)> _orders;

        /// <inheritdoc />
        public IReadOnlyList<(Expression<Func<T, object>> Expression, OrderDirection Direction)> Orders => _orders;

        /// <summary>
        /// Builds a new <see cref="MultiOrderSpecification{T}"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <paramref name="orders"/> is <see langword="null"/>.</exception>
        public MultiOrderSpecification(IEnumerable<(Expression<Func<T, object>> Expression, OrderDirection Direction)> orders)
        {
            _orders = [.. orders];
        }
    }
}