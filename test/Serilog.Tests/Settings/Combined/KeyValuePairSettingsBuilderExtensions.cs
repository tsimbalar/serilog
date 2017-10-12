using System;
using System.Linq.Expressions;
using Serilog.Settings.KeyValuePairs.Combined;

namespace Serilog.Tests.Settings.Combined
{
    public static class KeyValuePairSettingsBuilderExtensions
    {
        public static ICombinedSettingsOptions AddExpression(this ICombinedSettingsOptions self,
            Expression<Func<LoggerConfiguration, LoggerConfiguration>> exp)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            var keyValuePairs = AppSettingsConverter.From(exp);

            return self.AddKeyValuePairs(keyValuePairs);
        }
    }
}
