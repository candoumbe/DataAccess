using System;
using System.Linq.Expressions;

namespace MedEasy.DAL.Repositories
{
    /// <summary>
    /// An instance of this class holds an <see cref="LambdaExpression"/> which defines a property and its related <see cref="SortDirection"/> to use to order collections
    /// </summary>
    /// <typeparam name="T">Type of the object which the <see cref="OrderClause{T}"/> c</typeparam>
    public class OrderClause<T>
    {
        private readonly LambdaExpression _expression;
        private readonly SortDirection _direction;

        private OrderClause(LambdaExpression keySelector, SortDirection direction)
        {
            _expression = keySelector;
            _direction = direction;
        }

        /// <summary>
        /// Creates a new instance of <see cref="OrderClause{T}"/>
        /// </summary>
        /// <typeparam name="TProperty">Type of the property which will serve to order collections' items</typeparam>
        /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
        /// <param name="direction">Order direction</param>
        /// <returns>A fully built OrderClause</returns>
        public static OrderClause<T> Create<TProperty>(Expression<Func<T, TProperty>> keySelector,
            SortDirection direction = SortDirection.Ascending)
        {
            return new OrderClause<T>(keySelector, direction);
        }

        /// <summary>
        /// Creates a new instance of <see cref="OrderClause{T}"/>
        /// </summary>
        /// <param name="keySelector">Expression used to select the property to used to order items in a collection</param>
        /// <param name="direction">Order direction</param>
        /// <returns>A fully built OrderClause</returns>
        public static OrderClause<T> Create(LambdaExpression keySelector,
            SortDirection direction = SortDirection.Ascending)
        {
            return new OrderClause<T>(keySelector, direction);
        }

        public LambdaExpression Expression => _expression;
        /// <summary>
        /// 
        /// </summary>
        public SortDirection Direction => _direction;
    }
}