using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DHXHelperDemo.Code.DHX
{
    /// <summary>
    /// Borrowed from McYntire's mvc. datatables impl.  This helper class provides necessary code to 
    /// be able to build Column Definition (ColDef) appropriately.  
    /// </summary>
    public static class StaticReflectionHelper
    {
        public static MethodInfo MethodInfo(this Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null) throw new ArgumentNullException("method");
            MethodCallExpression methodExpr = null;
            if (lambda.Body.NodeType == ExpressionType.Call)
                methodExpr = lambda.Body as MethodCallExpression;

            if (methodExpr == null) throw new ArgumentNullException("method");
            return methodExpr.Method;
        }

    }
}