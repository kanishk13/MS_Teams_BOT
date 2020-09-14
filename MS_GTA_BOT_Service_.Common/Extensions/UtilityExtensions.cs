//----------------------------------------------------------------------------
// <copyright company="Microsoft Corporation" file="UtilityExtensions.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------
namespace MS.GTA.BOTService.Common.Utility
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Common Utility Extension class for miltiple entities
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Returns a Expression which is always true
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Expression</returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        /// <summary>
        /// Returns an Expression which is always false
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Expression</returns>
        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        /// <summary>
        /// Extension method to Append an 'OrElse' condition to the expression
        /// </summary>
        /// <typeparam name="T">Tyoe of Object</typeparam>
        /// <param name="expr1">Base Expression</param>
        /// <param name="expr2">Expression to be appended</param>
        /// <returns>Final Expression</returns>
        public static Expression<Func<T, bool>> OrElse<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var secondBody = expr2.Body.Replace(expr2.Parameters[0], expr1.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, secondBody), expr1.Parameters);
        }

        /// <summary>
        /// Extension method to Append an 'AndAlso' condition to the expression
        /// </summary>
        /// <typeparam name="T">Tyoe of Object</typeparam>
        /// <param name="expr1">Base Expression</param>
        /// <param name="expr2">Expression to be appended</param>
        /// <returns>Final Expression</returns>
        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var secondBody = expr2.Body.Replace(expr2.Parameters[0], expr1.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, secondBody), expr1.Parameters);
        }

        /// <summary>
        /// Returns a new instance of 'ReplaceVisitor' for given search and replace expressions
        /// </summary>
        /// <param name="expression">Base Expression</param>
        /// <param name="searchEx">Search Expression</param>
        /// <param name="replaceEx">Replace Expression</param>
        /// <returns>ReplaceVisitor instance</returns>
        public static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
        {
            return new ReplaceVisitor(searchEx, replaceEx).Visit(expression);
        }

        /// <summary>
        /// An Expression Visitor to traverse through LINQ Expression
        /// </summary>
        internal class ReplaceVisitor : ExpressionVisitor
        {
            private readonly Expression from;
            private readonly Expression to;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReplaceVisitor"/> class.
            /// </summary>
            /// <param name="from">From Expression</param>
            /// <param name="to">To Expresion</param>
            public ReplaceVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }

            /// <summary>
            /// Visits the input expression in Expression tree
            /// </summary>
            /// <param name="node">Expression to visit</param>
            /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression</returns>
            public override Expression Visit(Expression node)
            {
                return node == this.from ? this.to : base.Visit(node);
            }
        }
    }
}
