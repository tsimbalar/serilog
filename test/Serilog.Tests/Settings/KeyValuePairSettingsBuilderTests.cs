using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Serilog.Events;
using Serilog.Tests.Support;
using Xunit;

namespace Serilog.Tests.Settings
{
    public class KeyValuePairSettingsBuilderTests
    {
        [Fact]
        public void BuilderCombinesDifferentSources()
        {
            var keyValuePairs = new KeyValuePairSettingsBuilder()
                .AddExpression(lc => lc
                        .MinimumLevel.Fatal()
                        .MinimumLevel.Override("System", LogEventLevel.Error)
                        .Enrich.WithProperty("EnrichedProperty", "EnrichValue", false)

                        )
                .AddKeyValuePairs(new Dictionary<string, string>(){
                    { "minimum-level", "Error"},
                    { "enrich:with-property:Enriched2", "Enrichement2"}
                })
                .AddKeyValuePair("minimum-level", "Information")
                .AddKeyValuePair("enrich:with-property:Enriched3", "Enrichement3")
                .Build()
                ;

            var expected = new Dictionary<string, string>()
            {
                { "minimum-level", "Information"},
                { "minimum-level:override:System", "Error"},
                { "enrich:with-property:EnrichedProperty", "EnrichValue"},
                { "enrich:with-property:Enriched2", "Enrichement2"},
                { "enrich:with-property:Enriched3", "Enrichement3"},
            }.ToList();

            Assert.Equal(expected, keyValuePairs.ToList(), new KeyValuePairComparer<string, string>());
        }
    }

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
