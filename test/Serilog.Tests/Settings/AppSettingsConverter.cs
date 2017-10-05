using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Tests.Settings
{


    public static class AppSettingsConverter
    {
        public static IEnumerable<KeyValuePair<string, string>> From(Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            return FromRightToLeft(exp).Reverse().SelectMany(x => x);
        }

        static IEnumerable<List<KeyValuePair<string, string>>> FromRightToLeft(Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            if (exp == null) throw new ArgumentNullException(nameof(exp));

            Expression current = (MethodCallExpression)exp.Body;

            while (current is MethodCallExpression)
            {
                var methodCall = (MethodCallExpression)current;
                MemberExpression leftSide;
                IReadOnlyList<Expression> methodArguments;
                var method = methodCall.Method;
                var methodName = method.Name;
                if (method.IsStatic)
                {
                    // extension method
                    leftSide = (MemberExpression)methodCall.Arguments[0];
                    methodArguments = methodCall.Arguments.Skip(1).ToList().AsReadOnly();
                }
                else
                {
                    // regular method
                    leftSide = (MemberExpression)methodCall.Object;
                    methodArguments = methodCall.Arguments.ToList().AsReadOnly();
                }

                current = leftSide.Expression;

                switch (leftSide.Member.Name)
                {
                    case nameof(LoggerConfiguration.MinimumLevel):
                        if (methodName == nameof(LoggerMinimumLevelConfiguration.Override))
                        {
                            var overrideNamespace = ((ConstantExpression)methodArguments[0]).Value.ToString();
                            var overrideLevel = ExtractStringValue(methodArguments[1]);

                            yield return new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>($"minimum-level:override:{overrideNamespace}", overrideLevel)
                            };
                            continue;
                        }
                        if (!Enum.TryParse(methodName, out LogEventLevel minimumLevel))
                            throw new NotImplementedException($"Not supported : MinimumLevel.{methodName}");
                        yield return new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("minimum-level", minimumLevel.ToString())
                        };
                        continue;
                    case nameof(LoggerConfiguration.Enrich):
                        if (methodName != nameof(LoggerEnrichmentConfiguration.WithProperty))
                            throw new NotImplementedException($"Not supported : Enrich.{methodName}");
                        var enrichPropertyName = ((ConstantExpression)methodArguments[0]).Value.ToString();
                        var enrichWithArgument = methodArguments[1];
                        var enrichmentValue = ExtractStringValue(enrichWithArgument);
                        yield return new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>($"enrich:with-property:{enrichPropertyName}", enrichmentValue)
                        };
                        continue;
                    case nameof(LoggerConfiguration.WriteTo):
                    case nameof(LoggerConfiguration.AuditTo):
                        var sinkDirectives = new List<KeyValuePair<string, string>>();
                        var directive = leftSide.Member.Name == nameof(LoggerConfiguration.WriteTo) ? "write-to" : "audit-to";
                        // using 
                        var assembly = methodCall.Method.DeclaringType.GetTypeInfo().Assembly;
                        sinkDirectives.Add(new KeyValuePair<string, string>($"using:{assembly.GetName().Name}", $"{assembly.FullName}"));

                        var args = methodArguments
                            .Zip(method.GetParameters().Skip(1), (expression, param) => new
                            {
                                MethodArgument = expression,
                                Parameter = param
                            })
                            .Select(x => new
                            {
                                ParamName = x.Parameter.Name,
                                ParamValue = ExtractStringValue(x.MethodArgument)
                            })
                            .Where(x => x.ParamValue != null);

                        var directives = args.Select(x => new KeyValuePair<string, string>($"{directive}:{methodName}.{x.ParamName}", x.ParamValue));
                        sinkDirectives.AddRange(directives);

                        yield return sinkDirectives;
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
                    if (constantExp.Value == null) return null;
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
