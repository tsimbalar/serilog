using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serilog.Configuration;
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
                current = leftSide.Expression;

                if (leftSide.Member.DeclaringType != typeof(LoggerConfiguration)) continue;

                switch (leftSide.Member.Name)
                {
                    case nameof(LoggerConfiguration.MinimumLevel):
                        if (Enum.TryParse(methodCall.Method.Name, out LogEventLevel minimumLevel))
                        {
                            yield return new KeyValuePair<string, string>("minimum-level", minimumLevel.ToString());
                            continue;
                        }
                        throw new NotImplementedException($"Not supported : MinimumLevel.{methodCall.Method.Name}");
                    case nameof(LoggerConfiguration.Enrich):
                        if (methodCall.Method.Name != nameof(LoggerEnrichmentConfiguration.WithProperty))
                            throw new NotImplementedException($"Not supported : Enrich.{methodCall.Method.Name}");
                        var enrichPropertyName = ((ConstantExpression)methodCall.Arguments[0]).Value.ToString();
                        var enrichWithArgument = methodCall.Arguments[1];
                        var enrichmentValue = ExtractStringValue(enrichWithArgument);
                        yield return new KeyValuePair<string, string>($"enrich:with-property:{enrichPropertyName}", enrichmentValue);
                        continue;
                    default:
                        throw new NotSupportedException($"Not supported : LoggerConfiguration.{leftSide.Member.Name}");
                }
            }
        }

        static string ExtractStringValue(Expression exp)
        {
            if (exp == null) throw new ArgumentNullException(nameof(exp));
            switch (exp)
            {
                case ConstantExpression constantExp:
                    return $"{constantExp.Value}";

                case UnaryExpression unaryExp:
                    return $"{unaryExp.Operand}";

                case NewExpression newExp:
                    if (newExp.Type == typeof(Uri))
                    {
                        return ((ConstantExpression)newExp.Arguments[0]).Value.ToString();
                    }
                    throw new NotImplementedException($"Not supported : new {newExp.Type}(...)");

                default:
                    throw new NotImplementedException($"Cannot extract a string value from `{exp}`");
            }
        }
    }
}
