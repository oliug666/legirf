using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TPIH.Gecco.WPF.Core
{
    public static class MvvmExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, Expression<Func<object>> expression)
        {
            if (handler != null)
            {
                if (expression.NodeType != ExpressionType.Lambda)
                {
                    throw new ArgumentException("Value must be a lamda expression", "expression");
                }
 
                var body = expression.Body as MemberExpression;
                var unary = expression.Body as UnaryExpression;

                string propertyName;
                if (body != null)
                {
                    propertyName = body.Member.Name;
                }
                else if (unary != null)
                {
                    propertyName = (unary.Operand as MemberExpression).Member.Name;
                }
                else
                {
                    throw new ArgumentException("'x' should be a member expression");
                }
                

                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
