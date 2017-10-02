using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serilog.Events;

namespace Serilog.Tests.Settings
{
    

    public static class AppSettingsConverter
    {
        public static IEnumerable<KeyValuePair<string, string>> From(Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            return FromRightToLeft(exp).Reverse();
        }

        private static IEnumerable<KeyValuePair<string, string>> FromRightToLeft(Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            if (exp == null) throw new ArgumentNullException(nameof(exp));

            var current = exp.Body;

            while (current is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)current;
                var leftSide = (MemberExpression)((MethodCallExpression)current).Object;
                if (leftSide.Member.DeclaringType == typeof(LoggerConfiguration))
                {
                    if (leftSide.Member.Name == nameof(LoggerConfiguration.MinimumLevel))
                    {
                        if (Enum.TryParse(methodCall.Method.Name, out LogEventLevel minimumLevel))
                        {
                            yield return new KeyValuePair<string, string>("minimum-level", minimumLevel.ToString());
                        }
                    }
                }

                current = leftSide.Expression;
            }

            //var instanceMethodCall = (MethodCallExpression)exp.Body;
            //var objectExpression = (MemberExpression)instanceMethodCall.Object;



            //if (objectExpression.Member.DeclaringType == typeof(LoggerConfiguration) && objectExpression.Member.Name == nameof(LoggerConfiguration.MinimumLevel))
            //{
            //    LogEventLevel minimumLevel;
            //    if (Enum.TryParse(instanceMethodCall.Method.Name, out minimumLevel))
            //    {
            //        yield return new KeyValuePair<string, string>("minimum-level", minimumLevel.ToString());
            //    }

            //}

            //if (objectExpression.Expression is MethodCallExpression)
            //{
            //    instanceMethodCall = (MethodCallExpression) objectExpression.Expression;
            //    objectExpression = (MemberExpression) instanceMethodCall.Object;

            //    if (objectExpression.Member.DeclaringType == typeof(LoggerConfiguration) && objectExpression.Member.Name == nameof(LoggerConfiguration.MinimumLevel))
            //    {
            //        LogEventLevel minimumLevel;
            //        if (Enum.TryParse(instanceMethodCall.Method.Name, out minimumLevel))
            //        {
            //            yield return new KeyValuePair<string, string>("minimum-level", minimumLevel.ToString());
            //        }

            //    }
            //}
        }

    }
}
