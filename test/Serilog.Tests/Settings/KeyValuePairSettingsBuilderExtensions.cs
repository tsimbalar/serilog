using System;
using System.Linq.Expressions;
using Serilog.Settings.KeyValuePairs;

namespace Serilog.Tests.Settings
{
    public static class KeyValuePairSettingsBuilderExtensions
    {
        public static KeyValuePairSettingsBuilder AddExpression(this KeyValuePairSettingsBuilder self,
            Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            var keyValuePairs = AppSettingsConverter.From(exp);

            return self.AddKeyValuePairs(keyValuePairs);
        }
    }
}