using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Sprache;

namespace LinqyCalculator
{
    static class ExpressionParser
    {
        public static Expression<Func<decimal>> ParseExpression(string text)
        {
            var r = Expr.Parse(text);
            if (r is Success<Expression>)
                return Expression.Lambda<Func<decimal>>(((Success<Expression>) r).Result);

            throw new ArgumentException("Parsing error: " + ((Failure<Expression>)r).Message);
        }

        static Parser<ExpressionType> Operator(string op, ExpressionType opType)
        {
            return Parse.String(op).Token().Return(opType);
        }

        static readonly Parser<ExpressionType> Add = Operator("+", ExpressionType.AddChecked);
        static readonly Parser<ExpressionType> Subtract = Operator("-", ExpressionType.SubtractChecked);
        static readonly Parser<ExpressionType> Multiply = Operator("*", ExpressionType.MultiplyChecked);
        static readonly Parser<ExpressionType> Divide = Operator("/", ExpressionType.Divide);

        static readonly Parser<Expression> Number =
            (from integral in Parse.Numeric.AtLeastOnce().Text()
             from fraction in Parse.String(".").Concat(Parse.Numeric.AtLeastOnce()).Text()
                                .Or(Parse.Return(""))
             select (Expression)Expression.Constant(decimal.Parse(integral + fraction))).Token();

        static readonly Parser<Expression> Factor =
            ((from lparen in Parse.Char('(')
              from expr in Parse.Ref(() => Expr)
              from rparen in Parse.Char(')')
              select expr)
             .Or(Number)).Token();

        static readonly Parser<Expression> Term = Parse.ChainOperator(Multiply.Or(Divide), Factor, Expression.MakeBinary);

        static readonly Parser<Expression> Expr = Parse.ChainOperator(Add.Or(Subtract), Term, Expression.MakeBinary);
    }
}
