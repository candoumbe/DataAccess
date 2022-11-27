namespace Candoumbe.DataAccess.Repositories
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Wraps a <typeparamref name="T"/> into an include clause.
    /// </summary>
    /// <typeparam name="T">Type of the property the instance will wraps</typeparam>
    public class IncludeClause<T>
    {
        private IncludeClause(LambdaExpression selector)
        {
            Expression = selector;
        }

        /// <summary>
        /// Creates a <see cref="IncludeClause{T}"/> from the provided <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="TProperty">Type of the property targeted by <paramref name="expression"/>.</typeparam>
        /// <param name="expression">The property the resulting <see cref="IncludeClause{T}"/> will be built for.</param>
        /// <returns>an <see cref="IncludeClause{T}"/> that targets the property to include</returns>
        public static IncludeClause<T> Create<TProperty>(Expression<Func<T, TProperty>> expression) => new IncludeClause<T>(expression);

        /// <summary>
        /// Lambda expression wrapped by the current instance.
        /// </summary>
        public LambdaExpression Expression { get; }
    }
}