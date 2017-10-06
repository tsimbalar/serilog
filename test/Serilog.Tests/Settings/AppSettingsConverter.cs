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
        const string UsingDirective = "using";
        const string LevelSwitchDirective = "level-switch";
        const string AuditToDirective = "audit-to";
        const string WriteToDirective = "write-to";
        const string MinimumLevelDirective = "minimum-level";
        const string MinimumLevelControlledByDirective = "minimum-level:controlled-by";
        const string EnrichWithDirective = "enrich";
        const string EnrichWithPropertyDirective = "enrich:with-property";
        const string FilterDirective = "filter";

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
                var method = methodCall.Method;
                var methodName = method.Name;
                MemberExpression leftSide;
                IReadOnlyList<Expression> methodArguments;
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
                                new KeyValuePair<string, string>($"{MinimumLevelDirective}:override:{overrideNamespace}", overrideLevel)
                            };
                            continue;
                        }
                        if (!Enum.TryParse(methodName, out LogEventLevel minimumLevel))
                            throw new NotImplementedException($"Not supported : MinimumLevel.{methodName}");
                        yield return new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(MinimumLevelDirective, minimumLevel.ToString())
                        };
                        continue;
                    case nameof(LoggerConfiguration.Enrich):
                        if (methodName == nameof(LoggerEnrichmentConfiguration.WithProperty))
                        {
                            var enrichPropertyName = ((ConstantExpression)methodArguments[0]).Value.ToString();
                            var enrichWithArgument = methodArguments[1];
                            var enrichmentValue = ExtractStringValue(enrichWithArgument);
                            yield return new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>($"{EnrichWithPropertyDirective}:{enrichPropertyName}", enrichmentValue)
                            };
                            continue;
                        }
                        else
                        {
                            yield return SerializeMethodInvocation(EnrichWithDirective, methodCall);
                            continue;

                        }
                    case nameof(LoggerConfiguration.WriteTo):
                        yield return SerializeMethodInvocation(WriteToDirective, methodCall);
                        continue;
                    case nameof(LoggerConfiguration.AuditTo):
                        yield return SerializeMethodInvocation(AuditToDirective, methodCall);
                        continue;
                    default:
                        throw new NotSupportedException($"Not supported : LoggerConfiguration.{leftSide.Member.Name}");
                }
            }
        }

        static List<KeyValuePair<string, string>> SerializeMethodInvocation(string directivePrefix, MethodCallExpression methodCall)
        {
            // this is probably an extension method
            var methodArguments = methodCall.Arguments;
            var method = methodCall.Method;
            var methodName = method.Name;
            var enrichDirectives = new List<KeyValuePair<string, string>>();
            // using 
            var enrichAssembly = method.DeclaringType.GetTypeInfo().Assembly;
            var assemblyShortName = enrichAssembly.GetName().Name;
            if (assemblyShortName != "Serilog")
            {
                enrichDirectives.Add(new KeyValuePair<string, string>($"{UsingDirective}:{assemblyShortName}", $"{enrichAssembly.FullName}"));
            }
            var enrichArgs = methodArguments
                .Zip(method.GetParameters(), (expression, param) => new
                {
                    MethodArgument = expression,
                    Parameter = param
                })
                .Skip(1) // it's an extension method, the first item is the target
                .Select(x => new
                {
                    ParamName = x.Parameter.Name,
                    ParamValue = ExtractStringValue(x.MethodArgument)
                })
                .Where(x => x.ParamValue != null);

            var directives2 = enrichArgs.Select(x => new KeyValuePair<string, string>($"{directivePrefix}:{methodName}.{x.ParamName}", x.ParamValue)).ToList();
            if (directives2.Count > 0)
            {
                enrichDirectives.AddRange(directives2);
            }
            else
            {
                enrichDirectives.Add(new KeyValuePair<string, string>($"{directivePrefix}:{methodName}", ""));
            }
            return enrichDirectives;
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
