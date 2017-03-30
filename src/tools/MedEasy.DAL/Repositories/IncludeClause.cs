using System;
using System.Linq.Expressions;

namespace MedEasy.DAL.Repositories
{
    public class IncludeClause<T>
    {
        private readonly LambdaExpression _expression;
        

        private IncludeClause(LambdaExpression selector)
        {
            _expression = selector;   
        }

        public static IncludeClause<T> Create<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            return new IncludeClause<T>(selector);
        }

        public LambdaExpression Expression
        {
            get { return _expression; }
        }
    }
}